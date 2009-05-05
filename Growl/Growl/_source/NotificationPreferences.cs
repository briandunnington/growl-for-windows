using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    [Serializable]
    public class NotificationPreferences
    {
        private PrefEnabled prefEnabled = PrefEnabled.True;
        private Display prefDisplay = Display.Default;
        private PrefDuration prefDuration = PrefDuration.Default;
        private PrefSticky prefSticky = PrefSticky.Default;
        private PrefForward prefForward = PrefForward.Default;
        private List<string> prefForwardCustomList = new List<string>();
        private PrefPriority prefPriority = PrefPriority.Default;
        private PrefSound prefSound = PrefSound.Default;

        public static NotificationPreferences Default
        {
            get
            {
                return new NotificationPreferences();
            }
        }

        public virtual PrefEnabled PrefEnabled
        {
            get
            {
                return this.prefEnabled;
            }
            set
            {
                this.prefEnabled = value;
            }
        }

        public virtual Display PrefDisplay
        {
            get
            {
                return this.prefDisplay;
            }
            set
            {
                this.prefDisplay = value;
            }
        }

        public virtual PrefDuration PrefDuration
        {
            get
            {
                if (this.prefDuration == null) this.prefDuration = PrefDuration.Default;
                return this.prefDuration;
            }
            set
            {
                this.prefDuration = value;
            }
        }

        public virtual PrefSticky PrefSticky
        {
            get
            {
                return this.prefSticky;
            }
            set
            {
                this.prefSticky = value;
            }
        }

        public virtual PrefForward PrefForward
        {
            get
            {
                return this.prefForward;
            }
            set
            {
                this.prefForward = value;
            }
        }

        public virtual List<string> PrefForwardCustomList
        {
            get
            {
                return this.prefForwardCustomList;
            }
            set
            {
                this.prefForwardCustomList = value;
            }
        }

        public virtual PrefPriority PrefPriority
        {
            get
            {
                return this.prefPriority;
            }
            set
            {
                this.prefPriority = value;
            }
        }

        public virtual PrefSound PrefSound
        {
            get
            {
                return this.prefSound;
            }
            set
            {
                this.prefSound = value;
            }
        }
    }
}
