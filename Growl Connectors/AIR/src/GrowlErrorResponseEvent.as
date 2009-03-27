package  
{
	import flash.events.Event;

	public class GrowlErrorResponseEvent extends Event
	{
		public static const ERROR_RESPONSE:String = "error_response";
		
		public var errorCode:int;
		public var errorDescription:String;
		
		public function GrowlErrorResponseEvent(errorCode:int, errorDescription:String)
		{
			super(ERROR_RESPONSE);

			this.errorCode = errorCode;
			this.errorDescription = errorDescription;
		}
	}
}