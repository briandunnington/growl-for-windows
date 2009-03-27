using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.AutoUpdate
{
    public class CheckForUpdateCompleteEventArgs : EventArgs
    {
        private Manifest manifest;
        private string currentVersion;
        private bool userInitiated;
        private UpdateErrorEventArgs errorArgs;

        public CheckForUpdateCompleteEventArgs(Manifest manifest, string currentVersion, bool userInitiated, UpdateErrorEventArgs errorArgs)
        {
            this.manifest = manifest;
            this.currentVersion = currentVersion;
            this.userInitiated = userInitiated;
            this.errorArgs = errorArgs;
        }

        public Manifest Manifest
        {
            get
            {
                return this.manifest;
            }
        }

        public string LatestVersion
        {
            get
            {
                return this.manifest.Version;
            }
        }

        public string CurrentVersion
        {
            get
            {
                return this.currentVersion;
            }
        }

        public bool UpdateAvailable
        {
            get
            {
                if (this.errorArgs != null) 
                    return false;
                else
                    return (this.LatestVersion != this.CurrentVersion);
            }
        }

        public bool UpdateRequired
        {
            get
            {
                return this.manifest.Required;
            }
        }

        public bool UserInitiated
        {
            get
            {
                return this.userInitiated;
            }
        }

        public UpdateErrorEventArgs ErrorArgs
        {
            get
            {
                return this.errorArgs;
            }
        }
    }
}
