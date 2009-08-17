using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Provides the base implementation of the <see cref="IDisplay"/> interface.
    /// </summary>
    /// <remarks>
    /// If you are developing a visual display, you should inherit from <see cref="VisualDisplay"/>
    /// as it provides implementations for common visual display tasks. If you are creating a non-visual
    /// display (email, text-to-speech, etc), you should inherit from this class. 
    /// If you choose not to inherit from this class and instead to implement <see cref="IDisplay"/> 
    /// directly, note that your display class must still inherit from <see cref="MarshalByRefObject"/>.
    /// </remarks>
    public abstract class Display : MarshalByRefObject, IDisplay
    {
        /// <summary>
        /// The full path to the installation directory of Growl
        /// </summary>
        private string growlApplicationPath;

        /// <summary>
        /// The full path to the installation directory of this display
        /// </summary>
        private string displayStylePath;

        /// <summary>
        /// The <see cref="SettingsPanelBase"/> used to allow user's to set display-specific settings.
        /// </summary>
        private SettingsPanelBase settingsPanel;

        /// <summary>
        /// A collection of user-configurable settings that can be modified by the associated <see cref="SettingsPanelBase"/>.
        /// </summary>
        private Dictionary<string, object> settingsCollection;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        protected Display()
        {
            this.settingsPanel = new DefaultSettingsPanel();
            this.settingsCollection = new Dictionary<string, object>();
        }

        #region IDisplay Members

        /// <summary>
        /// The name of the display as shown to the user in Growl's preference settings.
        /// </summary>
        /// <value>Ex: Mailman</value>
        public abstract string Name{get;}

        /// <summary>
        /// A short description of what the display is or does.
        /// </summary>
        /// <value>Ex: Mailman delivers Growl notifications via email</value>
        public abstract string Description { get;}

        /// <summary>
        /// The name of the author of the display.
        /// </summary>
        /// <value>Ex: Joe Schmoe</value>
        public abstract string Author { get;}

        /// <summary>
        /// The website of the author or display.
        /// </summary>
        /// <value>Ex: http://www.website.com</value>
        public abstract string Website { get;}

        /// <summary>
        /// The version of the display.
        /// </summary>
        /// <value>Ex: 2.0.1</value>
        public abstract string Version { get; }

        /// <summary>
        /// The <see cref="SettingsPanelBase"/> used to allow user's to set display-specific settings.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// If your display has no user-configurable settings, you may use the <see cref="DefaultSettingsPanel"/>
        /// class.
        /// </remarks>
        public SettingsPanelBase SettingsPanel
        {
            get
            {
                return this.settingsPanel;
            }
            set
            {
                this.settingsPanel = value;
            }
        }

        /// <summary>
        /// The full path to the installation directory of the Growl program.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// This property is set by Growl when the display is loaded. Developers can use the information if
        /// required, but should not otherwise change the behavior of this property.
        /// </remarks>
        public string GrowlApplicationPath
        {
            get
            {
                return this.growlApplicationPath;
            }
            set
            {
                this.growlApplicationPath = value;
            }
        }

        /// <summary>
        /// The full path to the installation directory of this DisplayStyle.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// This property is set by Growl when the display is loaded. Developers can use the information if
        /// required, but should not otherwise change the behavior of this property.
        /// </remarks>
        public string DisplayStylePath
        {
            get
            {
                return this.displayStylePath;
            }
            set
            {
                this.displayStylePath = value;
            }
        }

        /// <summary>
        /// Stores a collection of user-configurable settings that can be modified by the associated
        /// <see cref="SettingsPanel"/>.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// Although the dictionary allows storing any object, all settings must be serializable
        /// (either marked with the <see cref="SerializableAttribute"/> or implementing <see cref="System.Runtime.Serialization.ISerializable"/>).
        /// </remarks>
        public Dictionary<string, object> SettingsCollection
        {
            get
            {
                return this.settingsCollection;
            }
            set
            {
                this.settingsCollection = value;
            }
        }

        /// <summary>
        /// Called when the display is first loaded, generally used for any initialization-type actions.
        /// </summary>
        public virtual void Load()
        {
        }

        /// <summary>
        /// Called when the display is unloaded, generally used for any last-minute cleanup.
        /// </summary>
        public virtual void Unload()
        {
        }

        /// <summary>
        /// Handles displaying the notification. Called each time a notification is received that is to
        /// be handled by this display.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> information</param>
        /// <param name="displayName">A string identifying the display name (used mainly by displays that provide multiple end-user selectable display styles)</param>
        /// <returns><c>true</c> if the handling of the message is 100% complete when this method finishes;<c>false</c> if the display may send a callback later (if you are creating a visual display that can be clicked, you should return false)</returns>
        public virtual bool ProcessNotification(Notification notification, string displayName)
        {
            HandleNotification(notification, displayName);

            // returning true indicates that the handling of the message is 100% complete (ie: there will be no callbacks later)
            return true;
        }

        /// <summary>
        /// Handles displaying the notification. Called each time a notification is received that is to
        /// be handled by this display.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> information</param>
        /// <param name="displayName">A string identifying the display name (used mainly by displays that provide multiple end-user selectable display styles)</param>
        protected abstract void HandleNotification(Notification notification, string displayName);

        /// <summary>
        /// Returns a list of end-user selectable display names that this display supports.
        /// </summary>
        /// <returns>Array of display names</returns>
        /// <remarks>
        /// Most displays will only support a single end-user selectable display, so this method can usually
        /// just return:  string[] {this.Name};
        /// For developers who wish to support multiple displays with a single DisplayStyle engine, this
        /// method can return a list of display names that will appear as options for the user. When
        /// <see cref="HandleNotification"/> is called, the individual display name will be passed
        /// along with the notification.
        /// </remarks>
        public virtual string[] GetListOfAvailableDisplays()
        {
            return new string[] { this.Name };
        }

        /// <summary>
        /// When implemented in a derived class, closes any open notifications.
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        public abstract void CloseAllOpenNotifications();

        /// <summary>
        /// When implemented in a derived class, closes the most-recently shown notification.
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        public abstract void CloseLastNotification();

        /// <summary>
        /// Forces any on-screen notifications to redraw themselves.
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        public virtual void Refresh()
        {
            // do nothing
        }

        /// <summary>
        /// Fired when the notification is clicked (standard left clicks only)
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        public abstract event NotificationCallbackEventHandler NotificationClicked;

        /// <summary>
        /// Fired when the notification is closed (either explicitly by the user, or
        /// automatically after a period of time, etc)
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// In the current version of Growl, a right mouse click explicitly closes 
        /// the notification. In this instance, the NotificationClosed event is fired,
        /// not the NotificationClicked event.
        /// </remarks>
        public abstract event NotificationCallbackEventHandler NotificationClosed;

        #endregion

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"></see> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"></see> property.
        /// </returns>
        /// <remarks>
        /// Developers who do not inherit from this class and implement <see cref="IDisplay"/> directly must take care to manage
        /// their leases properly. If a lease expires while Growl is still running and the display is later accessed, a
        /// RemotingException will occur.
        /// </remarks>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            // This lease never expires.
            return null;
        }
    }
}
