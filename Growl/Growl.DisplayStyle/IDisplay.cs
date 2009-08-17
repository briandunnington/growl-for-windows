using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Represents the interface required when creating a new DisplayStyle.
    /// </summary>
    /// <remarks>
    /// Most developers should simply inherit from the <see cref="Display"/> or
    /// <see cref="VisualDisplay"/> classes, which provide implementations for most 
    /// common properties. If you choose to implement <see cref="IDisplay"/> directly, 
    /// note that your display class must still inherit from <see cref="MarshalByRefObject"/>.
    /// </remarks>
    public interface IDisplay
    {
        /// <summary>
        /// The name of the display as shown to the user in Growl's preference settings.
        /// </summary>
        string Name { get;}

        /// <summary>
        /// A short description of what the display is or does.
        /// </summary>
        string Description { get;}

        /// <summary>
        /// The name of the author of the display.
        /// </summary>
        string Author { get;}

        /// <summary>
        /// The website of the author or display
        /// </summary>
        string Website { get;}

        /// <summary>
        /// The version of the display.
        /// </summary>
        /// <remarks>
        /// Display developers should take care to make sure to update the version information whenever
        /// the display code is updated.
        /// </remarks>
        string Version { get;}

        /// <summary>
        /// The <see cref="SettingsPanelBase"/> used to allow user's to set display-specific settings.
        /// </summary>
        /// <remarks>
        /// If your display has no user-configurable settings, you may use the <see cref="DefaultSettingsPanel"/>
        /// class.
        /// </remarks>
        SettingsPanelBase SettingsPanel { get;set;}

        /// <summary>
        /// The full path to the installation directory of the Growl program.
        /// </summary>
        /// <remarks>
        /// This property is set by Growl when the display is loaded. Developers can use the information if
        /// required, but should not otherwise change the behavior of this property.
        /// </remarks>
        string GrowlApplicationPath { get;set;}

        /// <summary>
        /// The full path to the installation directory of this DisplayStyle.
        /// </summary>
        /// <remarks>
        /// This property is set by Growl when the display is loaded. Developers can use the information if
        /// required, but should not otherwise change the behavior of this property.
        /// </remarks>
        string DisplayStylePath { get;set;}

        /// <summary>
        /// Stores a collection of user-configurable settings that can be modified by the associated
        /// <see cref="SettingsPanel"/>.
        /// </summary>
        /// <remarks>
        /// Although the dictionary allows storing any object, all settings must be serializable
        /// (either marked with the <see cref="SerializableAttribute"/> or implementing <see cref="System.Runtime.Serialization.ISerializable"/>).
        /// </remarks>
        Dictionary<string, object> SettingsCollection {get;set;}

        /// <summary>
        /// Called when the display is first loaded, generally used for any initialization-type actions.
        /// </summary>
        void Load();

        /// <summary>
        /// Called when the display is unloaded, generally used for any last-minute cleanup.
        /// </summary>
        void Unload();

        /// <summary>
        /// Handles displaying the notification. Called each time a notification is received that is to 
        /// be handled by this display.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> information</param>
        /// <param name="displayName">A string identifying the display name (used mainly by displays that provide multiple end-user selectable display styles)</param>
        /// <returns><c>true</c> if the handling of the message is 100% complete when this method finishes;<c>false</c> if the display may send a callback later (if you are creating a visual display that can be clicked, you should return false)</returns>
        bool ProcessNotification(Notification notification, string displayName);

        /// <summary>
        /// Returns a list of end-user selectable display names that this display supports.
        /// </summary>
        /// <remarks>
        /// Most displays will only support a single end-user selectable display, so this method can usually 
        /// just return:  string[] {this.Name};
        /// For developers who wish to support multiple displays with a single DisplayStyle engine, this
        /// method can return a list of display names that will appear as options for the user. When 
        /// <see cref="ProcessNotification"/> is called, the individual display name will be passed
        /// along with the notification.
        /// </remarks>
        /// <returns>Array of display names</returns>
        string[] GetListOfAvailableDisplays();

        /// <summary>
        /// When implemented in a derived class, closes any open notifications.
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        void CloseAllOpenNotifications();

        /// <summary>
        /// When implemented in a derived class, closes the most-recently opened notification.
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        void CloseLastNotification();

        /// <summary>
        /// Forces any on-screen notifications to redraw themselves.
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        void Refresh();

        /// <summary>
        /// Fired when the notification is clicked (standard left clicks only)
        /// </summary>
        /// <remarks>
        /// This is generally only applicable to displays that show a visual element,
        /// but all displays must implement the method nonetheless.
        /// </remarks>
        event NotificationCallbackEventHandler NotificationClicked;

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
        event NotificationCallbackEventHandler NotificationClosed;
    }
}
