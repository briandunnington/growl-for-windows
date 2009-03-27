using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Growl
{
    [Serializable]
    internal class DisplayDefault : Display
    {
        private Display realDisplay;

        public DisplayDefault()
            : base(DEFAULT_DISPLAY_LABEL, null, true)
        {
        }

        public void Update(Display display)
        {
            this.realDisplay = display;
        }

        public override string ActualName
        {
            get
            {
                return this.realDisplay.ActualName;
            }
        }

        public override string Author
        {
            get
            {
                return this.realDisplay.Author;
            }
        }

        public override string Description
        {
            get
            {
                return this.realDisplay.Description;
            }
        }

        public override Dictionary<string, object> SettingsCollection
        {
            set
            {
                this.realDisplay.SettingsCollection = value;
            }
        }

        public override Growl.DisplayStyle.SettingsPanelBase SettingsPanel
        {
            get
            {
                return this.realDisplay.SettingsPanel;
            }
        }

        public override string Version
        {
            get
            {
                return this.realDisplay.Version;
            }
        }

        public override string Website
        {
            get
            {
                return this.realDisplay.Website;
            }
        }

        internal override void CloseAllOpenNotifications()
        {
            this.realDisplay.CloseAllOpenNotifications();
        }

        internal override void CloseLastNotification()
        {
            this.realDisplay.CloseLastNotification();
        }

        public override void ProcessNotification(Growl.DisplayStyle.Notification notification, Growl.Daemon.CallbackInfo cbInfo, Growl.Daemon.RequestInfo requestInfo)
        {
            this.realDisplay.ProcessNotification(notification, cbInfo, requestInfo);
        }

        public override int GetHashCode()
        {
            return this.realDisplay.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Display d = obj as Display;
            if (d != null)
            {
                //return this.realDisplay.Name == ((Display)obj).Name;
                return d.IsDefault;
            }
            else
                return base.Equals(obj);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.SetType(typeof(DisplayDefaultSerializationHelper));
        }

        [Serializable]
        private class DisplayDefaultSerializationHelper : IObjectReference
        {
            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                return Display.Default;
            }

            #endregion
        }
    }
}
