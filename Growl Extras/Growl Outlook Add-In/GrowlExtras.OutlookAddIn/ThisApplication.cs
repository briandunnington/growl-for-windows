using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.Tools.Applications.Runtime;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;

namespace GrowlExtras.OutlookAddIn
{
    public partial class ThisApplication
    {
        private const string APP_NAME = "Outlook";

        private Object _helpMenuIndex;
        private Office.CommandBar _menuBar;
        private Office.CommandBarPopup _topMenu;
        private Office.CommandBarButton _settingsMenu;

        private Growl.Connector.NotificationType reminder = new Growl.Connector.NotificationType("Outlook Reminder", "Outlook Reminder");
        private Growl.Connector.NotificationType newmail = new Growl.Connector.NotificationType("New Mail", "New Mail");
        Growl.Connector.NotificationType[] notificationTypes;
        Growl.Connector.GrowlConnector growl;
        Growl.Connector.Application application;

        private void ThisApplication_Startup(object sender, System.EventArgs e)
        {
            BuildMenu();

            this.NewMailEx += new Microsoft.Office.Interop.Outlook.ApplicationEvents_11_NewMailExEventHandler(ThisApplication_NewMailEx);
            this.Reminder += new Microsoft.Office.Interop.Outlook.ApplicationEvents_11_ReminderEventHandler(ThisApplication_Reminder);

            // setup our growl object
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Uri uri = new Uri(assembly.CodeBase);
            string path = System.IO.Path.GetDirectoryName(uri.LocalPath);
            string icon = String.Format(@"{0}\outlook.png", path);
            this.application = new Growl.Connector.Application(APP_NAME);
            this.application.Icon = icon;
            this.notificationTypes = new Growl.Connector.NotificationType[] { reminder, newmail };
            this.growl = new Growl.Connector.GrowlConnector(Properties.Settings.Default.Password);
            this.growl.EncryptionAlgorithm = Growl.Connector.Cryptography.SymmetricAlgorithmType.PlainText;
            this.growl.NotificationCallback +=new Growl.Connector.GrowlConnector.CallbackEventHandler(growl_NotificationCallback);
            this.growl.Register(this.application, this.notificationTypes);
        }

        private void ThisApplication_Shutdown(object sender, System.EventArgs e)
        {
        }

        void ThisApplication_NewMailEx(string EntryIDCollection)
        {
            if (Properties.Settings.Default.EnableNewMailNotifications)
            {
                if (EntryIDCollection != null)
                {
                    string title = null;
                    string text = null;

                    string[] ids = EntryIDCollection.Split(',');
                    if (ids.Length > 4)
                    {
                        title = "New Mail";
                        text = String.Format("You have {0} new messages", ids.Length);

                        Growl.Connector.Notification notification = new Growl.Connector.Notification(this.application.Name, newmail.Name, String.Empty, title, text);
                        Growl.Connector.CallbackContext callbackContext = new Growl.Connector.CallbackContext("null", "multimessage");
                        growl.Notify(notification, callbackContext);
                    }
                    else
                    {
                        foreach (string id in ids)
                        {
                            object obj = this.Session.GetItemFromID(id.Trim(), Type.Missing);
                            if (obj is Outlook.MailItem)
                            {
                                Outlook.MailItem message = (Outlook.MailItem)obj;
                                string body = message.Body.Replace("\r\n", "\n");
                                body = (body.Length > 50 ? body.Substring(0, 50) + "..." : body);

                                // just saving this for future reference
                                //Outlook.MAPIFolder folder = message.Parent as Outlook.MAPIFolder;
                                //Outlook.NameSpace outlookNameSpace = this.GetNamespace("MAPI");
                                //Outlook.MAPIFolder junkFolder = outlookNameSpace.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders..olFolderJunk);

                                title = message.Subject;
                                if (!String.IsNullOrEmpty(title)) title = title.Trim();
                                title = (String.IsNullOrEmpty(title) ? "[No Subject]" : message.Subject);
                                text = String.Format("From: {0}\n{1}", message.SenderName, body);

                                Growl.Connector.Priority priority = Growl.Connector.Priority.Normal;
                                if (message.Importance == Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh) priority = Growl.Connector.Priority.High;
                                else if (message.Importance == Microsoft.Office.Interop.Outlook.OlImportance.olImportanceLow) priority = Growl.Connector.Priority.Moderate;

                                Growl.Connector.Notification notification = new Growl.Connector.Notification(this.application.Name, newmail.Name, String.Empty, title, text);
                                notification.Priority = priority;
                                Growl.Connector.CallbackContext callbackContext = new Growl.Connector.CallbackContext(id, "mailmessage");
                                growl.Notify(notification, callbackContext);
                            }
                            else if (obj is Outlook.MeetingItem)
                            {
                                Outlook.MeetingItem message = (Outlook.MeetingItem)obj;
                                string body = message.Body.Replace("\r\n", "\n");
                                body = (body.Length > 50 ? body.Substring(0, 50) + "..." : body);

                                title = message.Subject;
                                if (!String.IsNullOrEmpty(title)) title = title.Trim();
                                title = (String.IsNullOrEmpty(title) ? "[No Subject]" : message.Subject);
                                text = String.Format("From: {0}\n{1}", message.SenderName, body);

                                Growl.Connector.Priority priority = Growl.Connector.Priority.Normal;
                                if (message.Importance == Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh) priority = Growl.Connector.Priority.High;
                                else if (message.Importance == Microsoft.Office.Interop.Outlook.OlImportance.olImportanceLow) priority = Growl.Connector.Priority.Moderate;

                                Growl.Connector.Notification notification = new Growl.Connector.Notification(this.application.Name, newmail.Name, String.Empty, title, text);
                                notification.Priority = priority;
                                Growl.Connector.CallbackContext callbackContext = new Growl.Connector.CallbackContext(id, "mailmessage");
                                growl.Notify(notification, callbackContext);
                            }
                        }
                    }
                }
            }
        }

        void ThisApplication_Reminder(object item)
        {
            if (Properties.Settings.Default.EnableReminderNotifications)
            {
                string title;
                string reminderMsg;
                string data;
                string type;

                if (item is Outlook.MailItem)
                {
                    Outlook.MailItem reminderMail;
                    reminderMail = (Outlook.MailItem)item;
                    title = "Outlook Mail Reminder";
                    reminderMsg = String.Format("{0}\n\nReminder time: {1:t}", reminderMail.Subject, reminderMail.ReminderTime);
                    data = reminderMail.EntryID;
                    type = "remindermail";
                }
                else if (item is Outlook.AppointmentItem)
                {
                    Outlook.AppointmentItem reminderAppt;
                    reminderAppt = (Outlook.AppointmentItem)item;
                    title = "Outlook Appointment Reminder";
                    reminderMsg = String.Format("{0}\n\nLocation: {1}\nReminder time: {2:t}", reminderAppt.Subject, reminderAppt.Location, reminderAppt.Start);
                    data = reminderAppt.EntryID;
                    type = "reminderappointment";
                }
                else if (item is Outlook.TaskItem)
                {
                    Outlook.TaskItem reminderTask;
                    reminderTask = (Outlook.TaskItem)item;
                    title = "Outlook Task Reminder";
                    reminderMsg = String.Format("{0}\n\nReminder time: {1:t}", reminderTask.Subject, reminderTask.ReminderTime);
                    data = reminderTask.EntryID;
                    type = "remindertask";
                }
                else
                {
                    // Unsupported item
                    return;
                }

                // send to growl
                Growl.Connector.Notification notification = new Growl.Connector.Notification(this.application.Name, reminder.Name, String.Empty, title, reminderMsg);
                Growl.Connector.CallbackContext callbackContext = new Growl.Connector.CallbackContext(data, type);
                growl.Notify(notification, callbackContext);
            }
        }

        void growl_NotificationCallback(Growl.Connector.Response response, Growl.Connector.CallbackData callbackData)
        {
            if (callbackData != null && callbackData.Result == Growl.CoreLibrary.CallbackResult.CLICK)
            {
                string type = callbackData.Type;
                if (type != null && type != String.Empty)
                {
                    string id = callbackData.Data;
                    object obj = this.Session.GetItemFromID(id, Type.Missing);

                    switch (type)
                    {
                        case "mailmessage":
                            if (obj != null && obj is Outlook.MailItem)
                            {
                                Outlook.MailItem message = (Outlook.MailItem)obj;
                                EnableFullActivation();
                                message.Display(false);
                                DisableFullActivation();
                            }
                            obj = null;
                            break;
                        case "multimessage":
                            object aw = this.ActiveWindow();
                            if (aw is Microsoft.Office.Interop.Outlook.Explorer)
                            {
                                Microsoft.Office.Interop.Outlook.Explorer explorer = (Microsoft.Office.Interop.Outlook.Explorer)aw;
                                EnableFullActivation();
                                explorer.Activate();
                                DisableFullActivation();
                            }
                            else if (aw is Microsoft.Office.Interop.Outlook.Inspector)
                            {
                                Microsoft.Office.Interop.Outlook.Inspector inspector = (Microsoft.Office.Interop.Outlook.Inspector)aw;
                                EnableFullActivation();
                                inspector.Activate();
                                DisableFullActivation();
                            }
                            break;
                        case "remindermail":
                            if (obj != null && obj is Outlook.MailItem)
                            {
                                Outlook.MailItem item = (Outlook.MailItem)obj;
                                EnableFullActivation();
                                item.Display(false);
                                DisableFullActivation();
                            }
                            obj = null;
                            break;
                        case "reminderappointment":
                            if (obj != null && obj is Outlook.AppointmentItem)
                            {
                                Outlook.AppointmentItem item = (Outlook.AppointmentItem)obj;
                                EnableFullActivation();
                                item.Display(false);
                                DisableFullActivation();
                            }
                            obj = null;
                            break;
                        case "remindertask":
                            if (obj != null && obj is Outlook.TaskItem)
                            {
                                Outlook.TaskItem item = (Outlook.TaskItem)obj;
                                EnableFullActivation();
                                item.Display(false);
                                DisableFullActivation();
                            }
                            obj = null;
                            break;
                    }
                }
            }
        }

        # region window activation trickery

        public static readonly uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SystemParametersInfo", SetLastError = true)]
        public static extern bool SystemParametersInfoSet(uint action, uint param, uint vparam, uint init);


        private void EnableFullActivation()
        {
            SystemParametersInfoSet(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, 0, 0x0002 | 0x0001);
        }

        private void DisableFullActivation()
        {
            SystemParametersInfoSet(SPI_SETFOREGROUNDLOCKTIMEOUT, 200000, 200000, 0x0002 | 0x0001);
        }

        # endregion window activation trickery

        void _settingsMenu_Click(Microsoft.Office.Core.CommandBarButton Ctrl, ref bool CancelDefault)
        {
            SettingsForm frm = new SettingsForm();
            frm.Location = System.Windows.Forms.Control.MousePosition;
            DialogResult result = frm.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.growl.Password = Properties.Settings.Default.Password;
            }
        }

        private void BuildMenu()
        {
            _menuBar = this.ActiveExplorer().CommandBars.ActiveMenuBar;
            _helpMenuIndex = _menuBar.Controls.Count;

            _topMenu = (Office.CommandBarPopup)(_menuBar.Controls.Add(Office.MsoControlType.msoControlPopup, Type.Missing, Type.Missing, _helpMenuIndex, true));
            _topMenu.Caption = "Add-in Tasks";
            _topMenu.Visible = true;
            _settingsMenu = (Office.CommandBarButton)(_topMenu.Controls.Add(Office.MsoControlType.msoControlButton, Type.Missing, Type.Missing, Type.Missing, true));
            _settingsMenu.Caption = "Growl Notification Settings...";
            _settingsMenu.Visible = true;
            _settingsMenu.Enabled = true;
            _settingsMenu.Click += new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(_settingsMenu_Click);
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisApplication_Startup);
            this.Shutdown += new System.EventHandler(ThisApplication_Shutdown);
        }

        #endregion
    }
}
