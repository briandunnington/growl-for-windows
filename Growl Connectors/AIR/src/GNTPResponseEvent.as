package  
{
	import flash.events.Event;

	public class GNTPResponseEvent extends Event
	{
		public static const RESPONSE:String = "gntpResponse";
		
		public var message:String;
		public var gntpSocket:GNTPSocket;
		
		public function GNTPResponseEvent(message:String, gntpSocket:GNTPSocket) 
		{
			super(RESPONSE);
			
			this.message = message;
			this.gntpSocket = gntpSocket;
		}
		
		public override function clone():Event
		{
			return new GNTPResponseEvent(this.message, this.gntpSocket);
		}
	}
}