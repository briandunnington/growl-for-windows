using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CoreAudioApi;
using CoreAudioApi.Interfaces;
using Growl.Connector;

namespace Volumeter
{
    class AppContext : ApplicationContext
    {
        const long buffer = 100;

        int spkrVolumeControlID = -1;
        int spkrMuteControlID = -1;
        Hwnd hwnd;
        GrowlConnector growl;
        string appName = "Volumeter";
        string ntNameVolumeUp = "Volume Up";
        string ntNameVolumeDown = "Volume Down";
        int currentVolume;
        System.Timers.Timer timer;
        Stack<int> pendingChanges = new Stack<int>();
        int queueCount = 0;

        // CONTROL TYPES
        const int spkrVolumeControl = MM.MIXERCONTROL_CONTROLTYPE_VOLUME;
        const int spkrMuteControl = MM.MIXERCONTROL_CONTROLTYPE_MUTE;

        // COMPONENT TYPES
        int spkrComponent = MM.MIXERLINE_COMPONENTTYPE_DST_SPEAKERS;

        MMDevice defaultDevice;

        public AppContext()
            : base()
        {
            bool ok = false;
            if (Environment.OSVersion.Version.Major > 5)
            {
                // Vista & higher
                ok = true;

                MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
                this.defaultDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                this.defaultDevice.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
            }
            else
            {
                this.spkrVolumeControlID = MM.GetControlID(spkrComponent, spkrVolumeControl);
                this.spkrMuteControlID = MM.GetControlID(spkrComponent, spkrMuteControl);

                if (this.spkrVolumeControlID > 0)
                {
                    ok = true;

                    int v = MM.GetVolume(spkrVolumeControl, spkrComponent);
                    this.currentVolume = ConvertToPercentage(v);

                    this.hwnd = new Hwnd(WndProc);
                    int iw = (int)this.hwnd.Handle;

                    // ... and we can now activate the message monitor 
                    bool b = MM.MonitorControl(iw);
                }
            }

            if (ok)
            {
                NotificationType ntUp = new NotificationType(ntNameVolumeUp);
                NotificationType ntDown = new NotificationType(ntNameVolumeDown);
                NotificationType[] types = new NotificationType[] { ntUp, ntDown };
                Growl.Connector.Application app = new Growl.Connector.Application(appName);
                app.Icon = Properties.Resources.volumeter;

                this.growl = new GrowlConnector();
                this.growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;
                this.growl.Register(app, types);

                this.timer = new System.Timers.Timer(buffer);
                this.timer.AutoReset = false;
                this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            }
            else
            {
                MessageBox.Show("No speaker/line out component found to monitor");
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.queueCount == this.pendingChanges.Count)
            {
                lock (this.pendingChanges)
                {
                    int percent = this.pendingChanges.Pop();
                    SendNotification(percent);
                    this.pendingChanges.Clear();
                    this.queueCount = 0;
                    Console.WriteLine("SENT: " + percent.ToString());
                }
            }
        }

        void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            int percent = ConvertToPercentage(data.MasterVolume);
            if (data.Muted) percent = 0;

            QueueNotification(percent);

            Console.WriteLine(String.Format("{0} - {1}", percent, DateTime.Now.Ticks));
        }

        private void WndProc(ref Message m)
        {
            if (m.Msg == MM.MM_MIXM_CONTROL_CHANGE)     // Code 0x3D1 indicates a control change
            {
                int i = (int)(m.LParam);

                if (i == spkrVolumeControlID || i == spkrMuteControlID)
                {
                    long now = DateTime.Now.Ticks;
                    string type = this.ntNameVolumeDown;
                    int percent = 0;

                    // check for Mute first
                    bool wasMuted = false;
                    if (i == spkrMuteControlID)
                    {
                        int isMuted = MM.GetVolume(spkrMuteControl, spkrComponent);
                        if (isMuted > 0) wasMuted = true;
                    }

                    // now handle volume
                    if (!wasMuted || i == spkrVolumeControlID)
                    {
                        int v = MM.GetVolume(spkrVolumeControl, spkrComponent);
                        percent = ConvertToPercentage(v);
                    }

                    QueueNotification(percent);

                    Console.WriteLine(String.Format("{0} - {1}", percent, DateTime.Now.Ticks));
                }
            }
        }

        private void QueueNotification(int percent)
        {
            this.timer.Stop();
            this.pendingChanges.Push(percent);
            this.queueCount = this.pendingChanges.Count;
            this.timer.Start();
        }

        private void SendNotification(int percent)
        {
            string type = ntNameVolumeDown;
            if (percent > this.currentVolume)
                type = ntNameVolumeUp;

            string text = String.Format("The volume is currently at {0}%.", percent);
            Notification n = new Notification(this.appName, type, DateTime.Now.Ticks.ToString(), "Volume Changed", text);
            n.CoalescingID = this.appName;
            n.CustomTextAttributes.Add("Progress-Value", percent.ToString());
            this.growl.Notify(n);

            this.currentVolume = percent;
        }

        private int ConvertToPercentage(int volume)
        {
            // v will be between 0 (essentially muted) to 65536 (max volume)
            int percent = (volume * 100) / 65536;
            return percent;
        }

        private int ConvertToPercentage(float volume)
        {
            // v will be between 0 and 1
            int percent = Convert.ToInt32(Math.Round(volume * 100));
            return percent;
        }
    }
}
