using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Growl
{
    public partial class MonitorIdentifier : Growl.DisplayStyle.NotificationWindow
    {
        Timer fadeOutTimer;
        int opacity = 255;
        Bitmap bitmap;

        public static void IdentifyAllMonitors()
        {
            Screen[] screens = Screen.AllScreens;
            //Console.WriteLine("total screens: " + screens.Length.ToString());
            for (int i = 0; i < screens.Length; i++)
            {
                Screen screen = screens[i];
                MonitorIdentifier tf2 = new MonitorIdentifier();
                tf2.Show(screen, i + 1);

                //Console.WriteLine("index: " + i.ToString());
                //Console.WriteLine("device: " + GetDeviceName(screen));
                //Console.WriteLine("isprimary: " + screen.Primary.ToString());
                System.Threading.Thread.Sleep(100);
            }
        }

        private static string GetDeviceName(Screen screen)
        {
            string name = screen.DeviceName;
            int pos = name.IndexOf('\0');
            if (pos >= 0)
                name = name.Substring(0, pos);
            return name;
        }

        public MonitorIdentifier()
        {
            InitializeComponent();

            this.AutoClosing += new FormClosingEventHandler(BubblesWindow_AutoClosing);

            SetAutoCloseInterval(2000);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }


        protected override void OnShown(EventArgs e)
        {
            Growl.DisplayStyle.Utility.UpdateLayeredWindow(this.bitmap, this, this.Left, this.Top, (byte)opacity);
            base.OnShown(e);
        }

        public void Show(Screen screen, int val)
        {
            Point location = screen.Bounds.Location;
            int x = (screen.Bounds.Width - this.Width) / 2;
            int y = (screen.Bounds.Height - this.Height) / 2;
            location.Offset(x, y);
            this.Location = location;

            // get the correct image (we can only go up to 9, so hopefully no one has more than 9 monitors)
            this.bitmap = Properties.Resources._1;
            switch (val)
            {
                case 2:
                    this.bitmap = Properties.Resources._2;
                    break;
                case 3:
                    this.bitmap = Properties.Resources._3;
                    break;
                case 4:
                    this.bitmap = Properties.Resources._4;
                    break;
                case 5:
                    this.bitmap = Properties.Resources._5;
                    break;
                case 6:
                    this.bitmap = Properties.Resources._6;
                    break;
                case 7:
                    this.bitmap = Properties.Resources._7;
                    break;
                case 8:
                    this.bitmap = Properties.Resources._8;
                    break;
                case 9:
                    this.bitmap = Properties.Resources._9;
                    break;
            }

            this.Show();
        }

        void BubblesWindow_AutoClosing(object sender, FormClosingEventArgs e)
        {
            this.fadeOutTimer = new Timer();
            this.fadeOutTimer.Interval = 10;
            this.fadeOutTimer.Tick += new EventHandler(fadeOutTimer_Tick);
            this.fadeOutTimer.Start();
            e.Cancel = true;    // IMPORTANT!
        }

        void fadeOutTimer_Tick(object sender, EventArgs e)
        {
            this.opacity -= 20;
            if (this.opacity <= 0)
            {
                this.fadeOutTimer.Stop();
                this.Close();
            }
            else if (this.Visible)
            {
                Growl.DisplayStyle.Utility.UpdateLayeredWindow(this.bitmap, this, this.Left, this.Top, (byte)opacity);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // This form has to have the WS_EX_LAYERED extended style
                return cp;
            }
        }
    }
}