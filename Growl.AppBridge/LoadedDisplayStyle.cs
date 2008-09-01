using System;
using Growl.DisplayStyle;

namespace Growl.AppBridge
{
	public class LoadedDisplayStyle
	{
		private AppDomain appDomain;
		private RemoteLoader remoteLoaderProxy;

		public LoadedDisplayStyle(AppDomain appDomain, RemoteLoader remoteLoader)
		{
			this.appDomain = appDomain;
			this.remoteLoaderProxy = remoteLoader;
		}

		public AppDomain AppDomain
		{
			get
			{
				return this.appDomain;
			}
		}

		private RemoteLoader RemoteLoader
		{
			get
			{
				return (RemoteLoader) this.remoteLoaderProxy;
			}
		}

		public string FriendlyName
		{
			get
			{
				return this.appDomain.FriendlyName;
			}
		}

        public string GrowlApplicationPath
        {
            set
            {
                this.Display.GrowlApplicationPath = value;
            }
        }

        public string DisplayStylePath
        {
            set
            {
                this.Display.DisplayStylePath = value;
            }
        }

		public void Load()
		{
            this.Display.Load();
		}

		public void Unload()
		{
			this.Display.Unload();
		}

		public IDisplay Display
		{
			get
			{
				return this.RemoteLoader.Display;
			}
		}
	}
}
