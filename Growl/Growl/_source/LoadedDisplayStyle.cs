using System;
using Growl.DisplayStyle;

namespace Growl
{
	public class LoadedDisplayStyle
	{
		private IDisplay display;

		public LoadedDisplayStyle(IDisplay display)
		{
            this.display = display;
		}

		public string FriendlyName
		{
			get
			{
				return this.display.Name;
			}
		}

        public void SetGrowlApplicationPath(string path)
        {
            this.display.GrowlApplicationPath = path;
        }

        public void SetDisplayStylePath(string path)
        {
            this.display.DisplayStylePath = path;
        }

		public void Load()
		{
            this.display.Load();
		}

		public void Unload()
		{
            this.display.Unload();
		}

		public IDisplay Display
		{
			get
			{
                return this.display;
			}
		}
	}
}
