package
{
    import flash.events.*;
    import flash.net.Socket;
    import flash.system.Security;
    import flash.utils.ByteArray;
	//import flash.external.ExternalInterface;

    public class RemotingSocket extends Socket
    {
        private var _queue:Array = new Array();

        //public function RemotingSocket(server:String, port:int) 
		public function RemotingSocket() 
        {
            this.addEventListener(ProgressEvent.SOCKET_DATA, dataHandler);
            this.addEventListener(Event.CONNECT, connectHandler);
        }

        /**
         * Helper function to write to a socket
         */
        public function write(ba:ByteArray):void 
        {
            if(this.connected)
            {
                this.writeBytes(ba, 0, ba.length);
                this.flush();
            }
            else
            {
                _queue.push(ba);
            }
        }

        /**
         * When socket connects, send out any buffered messages
         */
        private function connectHandler(event:Event):void 
        {
            if (connected) 
            {
                var count:int = _queue.length;
                for (var i:int = 0; i < count; i++)
                {
                    var tmp:ByteArray = _queue.pop();
                    this.writeBytes(tmp, 0, tmp.length);
                    this.flush();
                }
            }
        }

        /**
         * Received data, create a new event to send it on
         */
        private function dataHandler(event:ProgressEvent):void 
        {
            while (this.bytesAvailable > 0) 
            {
                var b:String = this.readUTFBytes(this.bytesAvailable);

				//ExternalInterface.call("alert", b);
				
                var remotingEvent:RemotingEvent = new RemotingEvent(b);
                this.dispatchEvent(remotingEvent);
            }
        }
    }
}