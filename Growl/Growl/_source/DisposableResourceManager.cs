using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;

namespace Growl
{
    class DisposableResourceManager : ResourceManager
    {
        private List<IDisposable> disposables = new List<IDisposable>();

        public DisposableResourceManager(string s, System.Reflection.Assembly a) : base(s, a)
        {
        }

        public override object GetObject(string name, System.Globalization.CultureInfo culture)
        {
            object obj = base.GetObject(name, culture);
            IDisposable d = obj as IDisposable;
            if (d != null)
            {
                disposables.Add(d);
            }
            return obj;
        }

        public override void ReleaseAllResources()
        {
            while(this.disposables.Count > 0)
            {
                IDisposable d = this.disposables[0];
                if (d != null)
                {
                    d.Dispose();
                    this.disposables.RemoveAt(0);
                    d = null;
                }
            }

            base.ReleaseAllResources();
        }
    }
}
