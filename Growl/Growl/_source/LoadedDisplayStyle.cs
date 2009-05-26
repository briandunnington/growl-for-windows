using System;
using Growl.DisplayStyle;

namespace Growl
{
	public class LoadedDisplayStyle
	{
		private DisplayLoader displayLoader;

		public LoadedDisplayStyle(DisplayLoader displayLoader)
		{
            this.displayLoader = displayLoader;
		}

		public string FriendlyName
		{
			get
			{
				return this.Display.Name;
			}
		}

        public void SetGrowlApplicationPath(string path)
        {
            this.Display.GrowlApplicationPath = path;
        }

        public void SetDisplayStylePath(string path)
        {
            this.Display.DisplayStylePath = path;
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
				return this.displayLoader.Display;
			}
		}
	}
}
