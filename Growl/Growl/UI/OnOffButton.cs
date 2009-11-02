using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl.UI
{
    public class OnOffSwitchedEventArgs : EventArgs
    {
        private bool cancel;

        public bool Cancel
        {
            get
            {
                return this.cancel;
            }
            set
            {
                this.cancel = value;
            }
        }
    }

    public delegate void OnOffSwitchedEventHandler(OnOffSwitchedEventArgs args);

    public partial class OnOffButton : PictureBox
    {
        public event OnOffSwitchedEventHandler Switched;

        private bool on;

        public OnOffButton()
        {
            InitializeComponent();
        }

        protected override void OnClick(EventArgs e)
        {
            this.On = !this.on;

            //this.OnSwitched(new SwitchedEventArgs());
            //this.On = !this.on;
            //base.OnClick(e);
        }

        protected void OnSwitched(OnOffSwitchedEventArgs args)
        {
            if (this.Switched != null)
            {
                this.Switched(args);
            }

            if (!args.Cancel)
            {
                Switch();
            }
        }

        public bool On
        {
            get
            {
                return this.on;
            }
            set
            {
                bool wasSwitched = (this.on != value);
                if (wasSwitched)
                {
                    OnOffSwitchedEventArgs args = new OnOffSwitchedEventArgs();
                    this.OnSwitched(args);
                }
            }
        }

        private void Switch()
        {
            this.on = !this.on;

            if (this.on)
            {
                this.Image = global::Growl.Properties.Resources.on_button;
            }
            else
            {
                this.Image = global::Growl.Properties.Resources.off_button;
            }
        }
    }
}
