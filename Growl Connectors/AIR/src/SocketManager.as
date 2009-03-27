package  
{
	import flash.events.*;
	import flash.net.Socket;
	import flash.utils.ByteArray;
	import flash.utils.Dictionary;
	import flash.external.ExternalInterface;

	public class SocketManager extends EventDispatcher
	{
		private var sockets:Dictionary;
		private var host:String;
		private var port:int;

		public function SocketManager() 
		{
		}
		
		public function setServer(host:String, port:int):void
		{
			this.host = host;
			this.port = port;
		}
		
		public function send(bytes:ByteArray, waitForCallback:Boolean):void
        {
			var socketid:String = "socket_" + Math.round(Math.random() * 1000);
			var socket:GNTPSocket = new GNTPSocket(socketid, waitForCallback);
			this.sockets = new Dictionary(false);
			this.sockets[socketid] = socket;
			
			socket.addEventListener(GNTPResponseEvent.RESPONSE, this.dispatchEvent);
			socket.addEventListener(Event.CONNECT, this.dispatchEvent);
			socket.addEventListener(IOErrorEvent.IO_ERROR, this.dispatchEvent);
			socket.addEventListener(SecurityErrorEvent.SECURITY_ERROR, this.dispatchEvent);
			socket.addEventListener(ProgressEvent.SOCKET_DATA, this.dispatchEvent);
			
			socket.addEventListener(Event.CLOSE, function(e:Event):void
			{
				disconnectHandler(socketid);
			});

			socket.connect(this.host, this.port);
            socket.write(bytes);
        }
		
		private function disconnectHandler(id:String):void 
		{
			//ExternalInterface.call("alert", "SocketManager.disconnect");
			
			delete sockets[id];
		}
	}
}