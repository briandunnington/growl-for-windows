package  
{
	import flash.events.Event;

	public class GrowlCallbackResponseEvent extends Event
	{
		public static const CALLBACK_RESPONSE:String = "callback_response";
		
		public var notificationID:String;
		public var action:String;
		public var context:String;
		public var contextType:String;
		public var timestamp:Date;
		
		public function GrowlCallbackResponseEvent(notificationID:String, action:String, context:String, contextType:String, timestamp:Date)
		{
			super(CALLBACK_RESPONSE);
			
			this.notificationID = notificationID;
			this.action = action;
			this.context = context;
			this.contextType = contextType;
			this.timestamp = timestamp;
		}
	}
}