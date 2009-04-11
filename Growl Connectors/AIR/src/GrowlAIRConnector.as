package
{
	import com.hurlant.util.der.ByteString;
	import flash.events.*;
	//import flash.external.ExternalInterface;
	import flash.system.Capabilities;
	import flash.system.System;
	import flash.utils.ByteArray;
	import SocketManager;
	
	public class GrowlAIRConnector extends EventDispatcher
	{
		private var host:String;
		private var port:int;
		private var sm:SocketManager;
		private var password:String;
		private var passwordHashAlgorithm:String;
		private var encryptionAlgorithm:String;
		
		private var responseRegex:RegExp = /GNTP\/1.0 -(\S*) (\S*)/;
		private var headerRegex:RegExp = /([^\r\n:]+):\s+(.+)/;
		
		private const HEADER_APPLICATION_NAME:String = "Application-Name";
		
		public function GrowlAIRConnector(host:String, port:int) 
		{
			this.host = host;
			this.port = port;
		
			this.sm = new SocketManager();
			this.sm.setServer(this.host, this.port);

			this.sm.addEventListener(GNTPResponseEvent.RESPONSE, gntpResponseHandler);
			
			this.sm.addEventListener(Event.CONNECT, function(e:Event):void
			{
				handleEvent(e);
			});

			this.sm.addEventListener(IOErrorEvent.IO_ERROR, function(e:Event):void
			{
				handleEvent(e);
			});

			this.sm.addEventListener(SecurityErrorEvent.SECURITY_ERROR, function(e:Event):void
			{
				handleEvent(e);
			});

			this.sm.addEventListener(ProgressEvent.SOCKET_DATA, function(e:Event):void
			{
				handleEvent(e);
			});
			
			
			var e:Event = new Event("ready");
			this.dispatchEvent(e);
		}
		
		public function register(application:Object, notificationTypes:Array):void
		{
			try
			{
				var mb:MessageBuilder = new MessageBuilder(MessageType.Register, this.password, this.passwordHashAlgorithm, this.encryptionAlgorithm);
				if(application.name != null) mb.addHeader(HEADER_APPLICATION_NAME, application.name);
				if(application.icon != null) mb.addHeader("Application-Icon", application.icon);
				if(notificationTypes != null) mb.addHeader("Notifications-Count", notificationTypes.length.toString());
				mb = addOriginHeaders(mb);
				
				for (var i:int = 0; i < notificationTypes.length;i++ )
				{
					var notificationType:Object = notificationTypes[i];
					if(notificationType.name != null) mb.addHeader("Notification-Name", notificationType.name);
					if(notificationType.displayName != null) mb.addHeader("Notification-Display-Name", notificationType.displayName);
					if(notificationType.icon != null) mb.addHeader("Notification-Icon", notificationType.icon);
					if(notificationType.enabled != null) mb.addHeader("Notification-Enabled", notificationType.enabled);
					mb.addBlankLine();
				}

				//ExternalInterface.call("alert", mb.toString());

				sm.send(mb.getBytes(), false);
			}
			catch(errObject:Error)
			{
				//ExternalInterface.call("alert", errObject.message);
			}
		}
		
		public function notify(appName:String, notification:Object):void
		{
			var mb:MessageBuilder = new MessageBuilder(MessageType.Notify, this.password, this.passwordHashAlgorithm, this.encryptionAlgorithm);
			if(appName != null) mb.addHeader(HEADER_APPLICATION_NAME, appName);
			if(notification.name != null) mb.addHeader("Notification-Name", notification.name);
			if(notification.id != null) mb.addHeader("Notification-ID", notification.id);
			if(notification.title != null) mb.addHeader("Notification-Title", notification.title);
			if(notification.text != null) mb.addHeader("Notification-Text", notification.text);
			if(notification.icon != null) mb.addHeader("Notification-Icon", notification.icon);
			if(notification.priority != null) mb.addHeader("Notification-Priority", notification.priority);
			if(notification.sticky != null) mb.addHeader("Notification-Sticky", notification.sticky);

			var waitForCallback:Boolean = false;
			if (notification.callback != null && notification.callback.context != null)
			{
				mb.addHeader("Notification-Callback-Context", notification.callback.context);
				mb.addHeader("Notification-Callback-Context-Type", notification.callback.type);
				if (notification.callback.target != null)
				{
					mb.addHeader("Notification-Callback-Context-Type", notification.callback.target);
					mb.addHeader("Notification-Callback-Context-Type", notification.callback.method);
				}
				else
				{
					waitForCallback = true;
				}
			}
			
			mb = addOriginHeaders(mb);

			sm.send(mb.getBytes(), waitForCallback);
		}
		
		public function setPassword(password:String):void
		{
			this.password = password;
		}
		
		public function setPasswordHashAlgorithm(passwordHashAlgorithm:String):void
		{
			this.passwordHashAlgorithm = passwordHashAlgorithm;
		}
		
		public function setEncryptionAlgorithm(encryptionAlgorithm:String):void
		{
			this.encryptionAlgorithm = encryptionAlgorithm;
		}
		
		private function addOriginHeaders(mb:MessageBuilder):MessageBuilder
		{
			mb.addHeader("Origin-Machine-Name", "localhost");
			mb.addHeader("Origin-Software-Name", "Growl Flash Connector");
			mb.addHeader("Origin-Software-Version", "v1.0");
			mb.addHeader("Origin-Platform-Name", Capabilities.os);
			mb.addHeader("Origin-Platform-Version", Capabilities.version);
			mb.addBlankLine();
			return mb;
		}
		
		private function gntpResponseHandler(event:GNTPResponseEvent):void
        {
			//ExternalInterface.call("alert", event.message);
			//ExternalInterface.call("alert", event.gntpSocket.waitForCallback);

			try
			{
				var matches:Object = this.responseRegex.exec(event.message);
				var result:String = matches[1];

				if (result == ResponseType.OK)
				{
					var e_ok:GrowlOKResponseEvent = new GrowlOKResponseEvent();
					this.handleEvent(e_ok);
					
					if (!event.gntpSocket.waitForCallback) event.gntpSocket.disconnect();
				}
				else if (result == ResponseType.Callback)
				{
					var callback:Object = parseResponse(event.message);

					var notificationID:String = callback["Notification-ID"];
					var action:String = callback["Notification-Callback-Result"];
					var context:String = callback["Notification-Callback-Context"];
					var type:String = callback["Notification-Callback-Context-Type"];
					var timestamp:Date = parseUTCDate("2008-09-11T00:39:07Z");

					var e_callback:GrowlCallbackResponseEvent = new GrowlCallbackResponseEvent(notificationID, action, context, type, timestamp);
					this.dispatchEvent(e_callback);
					
					event.gntpSocket.disconnect();
				}
				else
				{
					var response:Object = parseResponse(event.message);
					
					var errorCode:int = response["Error-Code"];
					var errorDescription:String = response["Error-Description"];
					
					var e_error:GrowlErrorResponseEvent = new GrowlErrorResponseEvent(errorCode, errorDescription);
					this.dispatchEvent(e_error);
					
					event.gntpSocket.disconnect();
				}
			}
			catch (e:Error)
			{
				//ExternalInterface.call("alert", "ERROR: " + e.message);
			}
        }
		
		private function handleEvent(event:Event, ... rest):void
		{
			this.dispatchEvent(event);
		}
		
		private function parseUTCDate(value:String):Date {
			var dateStr:String = value;
			dateStr = dateStr.replace(/-/g, "/");
			dateStr = dateStr.replace("T", " ");
			dateStr = dateStr.replace("Z", " GMT-0000");
			return new Date(Date.parse(dateStr));
		}
		
		private function parseResponse(response:String):Object
		{
			var obj:Object = new Object();
			var lines:Array = response.split("\r\n")
			
			// start at 1 to skip the message header
			for (var i:int = 1; i < lines.length; i++)
			{
				var match:Object = this.headerRegex.exec(lines[i]);
				
				if (match != null)
				{
					obj[match[1]] = match[2];
				}
			}
			
			return obj;
		}
	}
}