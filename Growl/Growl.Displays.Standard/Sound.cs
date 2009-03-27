using System;
using System.Collections.Generic;
using System.Media;
using System.Text;

namespace Growl.Displays.Standard
{
    internal class Sound
    {
        SoundPlayer sp;
        private bool canPlay;

        public Sound(string systemSoundAlias)
        {
            this.sp = SystemSoundsManager.GetSound(systemSoundAlias);
            if (this.sp != null)
            {
                this.sp.LoadCompleted += new System.ComponentModel.AsyncCompletedEventHandler(sp_LoadCompleted);
                this.sp.LoadAsync();
            }
        }

        void sp_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                this.canPlay = true;
            }
            else
            {
                this.canPlay = false;
            }
        }

        public void Play()
        {
            if (this.canPlay)
            {
                this.sp.Play();
            }
        }
    }
}
