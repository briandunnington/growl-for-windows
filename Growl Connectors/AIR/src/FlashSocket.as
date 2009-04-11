package
{
	//import flash.external.ExternalInterface;
	import flash.events.*;
	import flash.net.Socket;
	import flash.utils.ByteArray;

	internal class FlashSocket extends EventDispatcher
	{		
		private var socket:RemotingSocket;
		private var id:String;

		public function FlashSocket(id:String){
			this.id = id;

			socket = new RemotingSocket();
			socket.addEventListener(RemotingEvent.REMOTING,remotingSocketHandler);

			socket.addEventListener(Event.CONNECT, this.dispatchEvent);
			socket.addEventListener(Event.CLOSE, this.dispatchEvent);
			socket.addEventListener(IOErrorEvent.IO_ERROR, this.dispatchEvent);
			socket.addEventListener(SecurityErrorEvent.SECURITY_ERROR, this.dispatchEvent);
			socket.addEventListener(ProgressEvent.SOCKET_DATA, this.dispatchEvent);
		}
		
		private function remotingSocketHandler(event:RemotingEvent):void
        {
			//ExternalInterface.call("alert", "at FlashSocket");
			
            this.dispatchEvent(event);
        }

        public function writeString(str:String):void
        {
			var ba:ByteArray = new ByteArray();
			ba.writeUTFBytes(str);
            this.write(ba);
        }

        public function write(bytes:ByteArray):void
        {
            socket.write(bytes);
        }		
		
        public function disconnect():void
        {
            socket.close();

            // Shouldnt flash call this?
            if(!socket.connected)
                this.dispatchEvent(new Event(Event.CLOSE));
        }
		
		public function connect(host:String, port:int):void
		{
			socket.connect(host, port);	
		}
	}	
}