package  
{
	import flash.events.*;
	import flash.external.ExternalInterface;
	import GrowlAIRConnector;
	
	public class GrowlJSConnector 
	{
		private var scope:String;
		private var growl:GrowlAIRConnector;

		private var connect_cb:String          = null;
        private var disconnect_cb:String       = null;
        private var receive_cb:String          = null;
        private var ioerror_cb:String          = null;
        private var securityerror_cb:String    = null;
        private var progressevent_cb:String    = null;
		private var okresponse_cb:String       = null;
		private var callbackresponse_cb:String = null;
		private var errorresponse_cb:String    = null;
		
		public function GrowlJSConnector(host:String, port:int, scope:String)
		{
			this.scope = scope;
			this.growl = new GrowlAIRConnector(host, port);
			
			ExternalInterface.addCallback("register", register);
			ExternalInterface.addCallback("notify", notify);
			ExternalInterface.addCallback("setPassword", setPassword);
			ExternalInterface.addCallback("setPasswordHashAlgorithm", setPasswordHashAlgorithm);
			ExternalInterface.addCallback("setEncryptionAlgorithm", setEncryptionAlgorithm);

			connect_cb = getJSFunctionName("onconnect");
			disconnect_cb = getJSFunctionName("ondisconnect");
			receive_cb = getJSFunctionName("ondata");
			ioerror_cb = getJSFunctionName("onioerror");
			securityerror_cb = getJSFunctionName("onsecurityerror");
			progressevent_cb = getJSFunctionName("onprogressevent");
			okresponse_cb = getJSFunctionName("onok");
			callbackresponse_cb = getJSFunctionName("oncallback");
			errorresponse_cb = getJSFunctionName("onerror");

			//this.growl.addEventListener("", listenerfunction);
			//this.growl.addEventListener("", listenerfunction);
			//this.growl.addEventListener("", listenerfunction);
			//this.growl.addEventListener("", listenerfunction);
			//this.growl.addEventListener("", listenerfunction);
			//this.growl.addEventListener("", listenerfunction);

			this.growl.addEventListener(Event.CONNECT, function(e:Event):void
			{
				handleEvent(e, connect_cb);
			});

			this.growl.addEventListener(IOErrorEvent.IO_ERROR, function(e:Event):void
			{
				handleEvent(e, ioerror_cb);
			});

			this.growl.addEventListener(SecurityErrorEvent.SECURITY_ERROR, function(e:Event):void
			{
				handleEvent(e, securityerror_cb);
			});

			this.growl.addEventListener(ProgressEvent.SOCKET_DATA, function(e:Event):void
			{
				handleEvent(e, progressevent_cb);
			});
			
			this.growl.addEventListener(GrowlOKResponseEvent.OK_RESPONSE, function(e:Event):void
			{
				handleEvent(e, okresponse_cb);
			});

			this.growl.addEventListener(GrowlErrorResponseEvent.ERROR_RESPONSE, function(e:GrowlErrorResponseEvent):void
			{
				// have to use ExternalInterface directly if we want to pass the arguments individually (as handleEvent passes them as an array)
				ExternalInterface.call(errorresponse_cb, e.errorCode, e.errorDescription);
			});

			this.growl.addEventListener(GrowlCallbackResponseEvent.CALLBACK_RESPONSE, function(e:GrowlCallbackResponseEvent):void
			{
				// have to use ExternalInterface directly if we want to pass the arguments individually (as handleEvent passes them as an array)
				ExternalInterface.call(callbackresponse_cb, e.notificationID, e.action, e.context, e.contextType, e.timestamp);
			});

			ExternalInterface.call(getJSFunctionName("onready"));
		}

		public function register(application:Object, notificationTypes:Array):void
		{
			this.growl.register(application, notificationTypes);
		}
		
		public function notify(appName:String, notification:Object):void
		{
			this.growl.notify(appName, notification);
		}
		
		public function setPassword(password:String):void
		{
			this.growl.setPassword(password);
		}
		
		public function setPasswordHashAlgorithm(passwordHashAlgorithm:String):void
		{
			this.growl.setPasswordHashAlgorithm(passwordHashAlgorithm);
		}
		
		public function setEncryptionAlgorithm(encryptionAlgorithm:String):void
		{
			this.growl.setEncryptionAlgorithm(encryptionAlgorithm);
		}
		
		private function handleEvent(event:Event, callback:String, ... rest):void
		{
			//ExternalInterface.call("alert", "handleEvent - " + callback);
			
			if (callback != null)
				ExternalInterface.call(callback, rest);
		}
		
		private function getJSFunctionName(functionName:String):String
		{
			var fullName:String = functionName;
			if (this.scope != null)
			{
				fullName = this.scope + "." + functionName;
			}
			return fullName;
		}
	}
}