package 
{
	import flash.display.Sprite;
	import flash.external.ExternalInterface;

	public class Main extends Sprite
	{
		public function Main():void
		{
			//ExternalInterface.marshallExceptions = true;
			
			var scope:String = root.loaderInfo.parameters.scope;
			if (scope == null) scope = "Growl";

			var host:String = root.loaderInfo.parameters.host;
			if (host == null) host = "127.0.0.1";
			
			var port:int = root.loaderInfo.parameters.port;
			if (port == 0) port = 23053;
			
			var growl:GrowlJSConnector = new GrowlJSConnector(host, port, scope);
		}
	}
}