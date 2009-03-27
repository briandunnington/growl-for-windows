package  
{
	import flash.events.Event;

	public class GrowlOKResponseEvent extends Event
	{
		public static const OK_RESPONSE:String = "ok_response";
		
		public function GrowlOKResponseEvent()
		{
			super(OK_RESPONSE);
		}
	}
}