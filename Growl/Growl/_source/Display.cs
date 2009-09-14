using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using Growl;

namespace Growl
{
    [Serializable]
    public class Display : DefaultablePreference, ISerializable
    {
        public delegate void NotificationCallbackEventHandler(Growl.Daemon.CallbackInfo cbInfo, Growl.CoreLibrary.CallbackResult result);
        public event NotificationCallbackEventHandler NotificationCallback;

        internal static DisplayDefault Default = new DisplayDefault();
        internal static DisplayNone None = new DisplayNone();
        private Growl.DisplayStyle.IDisplay display;
        private Dictionary<string, Growl.Daemon.CallbackInfo> notificationsAwaitingCallback;
        private DisplayNotificationCallbackDelegate d = new DisplayNotificationCallbackDelegate();

        public Display(string name, Growl.DisplayStyle.IDisplay display) : this(name, display, false)
        {
        }

        protected Display(string name, Growl.DisplayStyle.IDisplay display, bool isDefault)
        {
            Initialize(name, display, isDefault);
        }

        protected Display()
        {
            // if you use this constructor, you MUST call .Initialize yourself
        }

        protected void Initialize(string name, Growl.DisplayStyle.IDisplay display, bool isDefault)
        {
            this.Name = name;
            this.IsDefault = isDefault;
            this.display = display;

            this.notificationsAwaitingCallback = new Dictionary<string, Growl.Daemon.CallbackInfo>();

            if (this.display != null)
            {
                this.display.NotificationClicked += d.OnNotificationCallback;
                this.display.NotificationClosed += d.OnNotificationCallback;
                d.NotificationCallback += new Growl.CoreLibrary.NotificationCallbackEventHandler(display_NotificationCallback);
            }
        }

        void display_NotificationCallback(Growl.CoreLibrary.NotificationCallbackEventArgs args)
        {
            if (args.NotificationUUID != null && this.notificationsAwaitingCallback.ContainsKey(args.NotificationUUID))
            {
                Growl.Daemon.CallbackInfo cbInfo = this.notificationsAwaitingCallback[args.NotificationUUID];
                this.notificationsAwaitingCallback.Remove(args.NotificationUUID);

                if (this.NotificationCallback != null)
                {
                    if(cbInfo != null) cbInfo.SetAdditionalInfo(args.CustomInfo);
                    this.NotificationCallback(cbInfo, args.Result);
                }
            }
        }

        public virtual void ProcessNotification(DisplayStyle.Notification notification, Growl.Daemon.CallbackInfo cbInfo, Growl.Daemon.RequestInfo requestInfo)
        {
            try
            {
                //System.Threading.ParameterizedThreadStart pts = new System.Threading.ParameterizedThreadStart(DoShowNotification);
                //System.Threading.Thread t = new System.Threading.Thread(pts);
                //t.Start(notification);

                //this.display.HandleNotification(notification, this.name);
                bool done = this.display.ProcessNotification(notification, this.ActualName);

                // for any notifications that remain open (essentially, any visual notifications), add them to a list so we
                // can handle their callbacks later
                if (!done)
                {
                    this.notificationsAwaitingCallback.Add(notification.UUID, cbInfo);
                }
            }
            catch(Exception ex)
            {
                // suppress any exceptions here (in case the display fails for some reason)
                Utility.WriteDebugInfo(String.Format("Display failed to process notification: '{0}' - {1}", ex.Message, ex.StackTrace));
            }
        }

        public virtual Growl.DisplayStyle.SettingsPanelBase SettingsPanel
        {
            get
            {
                return DisplayStyleManager.GetSettingsPanel(this.display.DisplayStylePath);
            }
        }

        public virtual Dictionary<string, object> SettingsCollection
        {
            set
            {
                this.display.SettingsCollection = value;
            }
        }

        public virtual string Description
        {
            get
            {
                return this.display.Description;
            }
        }

        public virtual string Author
        {
            get
            {
                return this.display.Author;
            }
        }

        public virtual string Website
        {
            get
            {
                return this.display.Website;
            }
        }

        public virtual string Version
        {
            get
            {
                return this.display.Version;
            }
        }

        #region ISerializable Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(DisplaySerializationHelper));
            info.AddValue("displayName", this.ActualName, typeof(string));
        }

        #endregion

        internal void Refresh()
        {
            this.display.Refresh();
        }

        internal virtual void CloseAllOpenNotifications()
        {
            if(this.display != null)    // some displays (appears to be WPF only) are not loaded 100% yet, so this can fail if they have not been used yet
                this.display.CloseAllOpenNotifications();
        }

        internal virtual void CloseLastNotification()
        {
            if(this.display != null)
                this.display.CloseLastNotification();
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Display d = obj as Display;
            if (d != null)
            {
                return this.Name == d.Name;
            }
            else
                return base.Equals(obj);
        }

        [Serializable]
        private class DisplaySerializationHelper : IObjectReference
        {
            private string displayName = null; 

            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                if (this.displayName != null)
                    return DisplayStyleManager.FindDisplayStyle(this.displayName);

                return null;
            }

            #endregion
        }
    }
}
