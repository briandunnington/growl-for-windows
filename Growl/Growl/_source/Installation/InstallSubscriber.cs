using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Growl.Installation
{
    public partial class InstallSubscriber : Form
    {
        private const string USER_AGENT = "Growl for Windows - AutoInstaller";
        private const string TEMP_FOLDER = "__temp";

        private Growl.CoreLibrary.WebClientEx wc;
        private string uri;
        private bool appIsAlreadyRunning;
        private string tempFolder;
        private System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);
        private System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
        private DownloadProgressChangedEventArgs progress;
        private object progress_lock = new object();
        private string errorMessage;

        public InstallSubscriber()
        {
            InitializeComponent();

            // localize text
            this.Text = Properties.Resources.SubscriberInstaller_FormTitle;
            //this.InfoLabel.Text gets set below when displayed
            this.YesButton.Text = Properties.Resources.Button_Yes;
            this.NoButton.Text = Properties.Resources.Button_Cancel;
            this.OKButton.Text = Properties.Resources.Button_OK;

            this.BackColor = Color.FromArgb(240, 240, 240);
        }

        public bool LaunchInstaller(string uri, bool appIsAlreadyRunning, ref List<InternalNotification> queuedNotifications, ref int cultureCodeHash)
        {
            bool installed = false;
            this.uri = uri;
            this.appIsAlreadyRunning = appIsAlreadyRunning;
            this.tempFolder = Path.Combine(Utility.UserSettingFolder, TEMP_FOLDER);

            try
            {
                this.wc = new Growl.CoreLibrary.WebClientEx();
                wc.Headers.Add("User-Agent", USER_AGENT);

                byte[] data = wc.DownloadData(this.uri);
                string definition = Encoding.UTF8.GetString(data).Trim();
                SubscriberInfo info = SubscriberInfo.Parse(definition);
                if (info != null)
                {
                    this.InfoLabel.Text = String.Format(Utility.GetResourceString(Properties.Resources.SubscriberInstaller_Prompt), info.Name, info.Author, info.Description);
                    this.YesButton.Visible = true;
                    this.NoButton.Visible = true;
                    this.OKButton.Visible = false;

                    /* NOTE: there is a bug that is caused when Growl is launched via protocol handler (growl:) from Opera.
                     * when that happens, the call to ShowDialog hangs.
                     * i could not find any documentation on this or any reason why it would be happening (not on a non-ui thread, windows handle is already created, etc).
                     * the only fix i could find was to Show/Hide the form before calling ShowDialog. i dont even know why this works, but it does.
                     * */
                    this.Show();
                    this.Hide();

                    DialogResult result = this.ShowDialog();
                    if (result == DialogResult.Yes)
                    {
                        this.InfoLabel.Text = Utility.GetResourceString(Properties.Resources.SubscriberInstaller_Installing);
                        this.progressBar1.Value = 0;
                        this.progressBar1.Visible = true;
                        this.YesButton.Enabled = false;
                        this.NoButton.Enabled = false;
                        this.Show();
                        this.Refresh();

                        if (Directory.Exists(this.tempFolder))
                            Directory.Delete(this.tempFolder, true);
                        Directory.CreateDirectory(this.tempFolder);
                        string zipFileName = Path.Combine(this.tempFolder, String.Format("{0}.zip", System.Guid.NewGuid().ToString()));
                        info.LocalZipFileLocation = zipFileName;

                        wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                        wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);

                        StartDownload(info);

                        Utility.WriteDebugInfo(String.Format("Downloading subscriber plugin '{0}' to {1}", info.Name, info.LocalZipFileLocation));

                        System.Threading.WaitHandle[] handles = new System.Threading.WaitHandle[] { are, mre };
                        while (System.Threading.WaitHandle.WaitAny(handles) == 0)
                        {
                            lock (this.progress_lock)
                            {
                                this.progressBar1.Value = this.progress.ProgressPercentage;
                                Application.DoEvents();
                            }
                        }

                        this.progressBar1.Value = 100;
                        Application.DoEvents();

                        Utility.WriteDebugInfo(String.Format("Finished downloading subscriber plugin '{0}' to {1}", info.Name, info.LocalZipFileLocation));

                        if (this.errorMessage == null)
                        {
                            // unzip files to the correct location
                            string folder = Path.Combine(SubscriptionManager.UserPluginDirectory, Growl.CoreLibrary.PathUtility.GetSafeFolderName(info.Name));
                            if (!ApplicationMain.HasProgramLaunchedYet || !Directory.Exists(folder))
                            {
                                Utility.WriteDebugInfo(String.Format("Subscriber '{0}' downloaded - starting unzip.", info.Name));
                                Unzipper.UnZipFiles(info.LocalZipFileLocation, folder, false);

                                string text = String.Format(Properties.Resources.SubscriberInstaller_InstalledText, info.Name);
                                InternalNotification n = new InternalNotification(Properties.Resources.SubscriberInstaller_InstalledTitle, Utility.GetResourceString(text), null);
                                queuedNotifications.Add(n);

                                installed = true;

                                this.Close();
                            }
                            else
                            {
                                // display with the same name aleady exists...
                                ShowMessage(String.Format(Utility.GetResourceString(Properties.Resources.SubscriberInstaller_AlreadyInstalled), info.Name), true);
                            }

                            // clean up
                            Utility.WriteDebugInfo(String.Format("Deleteing '{0}' zip file at {1}", info.Name, info.LocalZipFileLocation));
                            if (File.Exists(info.LocalZipFileLocation)) File.Delete(info.LocalZipFileLocation);
                        }
                        else
                        {
                            Utility.WriteDebugInfo(String.Format("Error downloading subscriber plugin '{0}'.", info.Name));
                            ShowMessage(errorMessage, true);
                        }
                    }
                }
                else
                {
                    // definition file was malformed
                    ShowMessage(String.Format(Utility.GetResourceString(Properties.Resources.SubscriberInstaller_BadDefinitionFile), this.uri), true);
                }
            }
            catch (Exception ex)
            {
                // error downloading definition file
                Utility.WriteDebugInfo(String.Format("Error downloading subscriber plugin. {0} - {1}", ex.Message, ex.StackTrace));
                ShowMessage(String.Format(Utility.GetResourceString(Properties.Resources.SubscriberInstaller_NonexistentDefinitionFile), this.uri), true);
            }
            return installed;
        }

        private void StartDownload(object obj)
        {
            SubscriberInfo info = (SubscriberInfo)obj;
            this.wc.DownloadFileAsync(new Uri(info.PackageUrl), info.LocalZipFileLocation, info);
        }

        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.WriteDebugInfo(e.Error.Message);
                Utility.WriteDebugInfo(e.Error.StackTrace);
                this.errorMessage = Utility.GetResourceString(Properties.Resources.SubscriberInstaller_DownloadError);
            }
            else if (e.Cancelled)
            {
                this.errorMessage = Utility.GetResourceString(Properties.Resources.SubscriberInstaller_DownloadCancelled);
            }
            else
            {
                // sometimes the downloaded file is still being written to disk.
                // this will wait until the file is readable before returning.
                SubscriberInfo info = (SubscriberInfo)e.UserState;
                bool fileAvailable = false;
                int counter = 0;
                while (!fileAvailable && counter < 10)
                {
                    counter++;
                    try
                    {
                        FileStream fs = File.OpenRead(info.LocalZipFileLocation);
                        using (fs)
                        {
                            fileAvailable = true;
                        }
                    }
                    catch
                    {
                        // wait a bit to allow the disk I/O to complete
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }

            mre.Set();
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            lock (this.progress_lock)
            {
                this.progress = e;
                are.Set();
            }
        }

        private void ShowMessage(string message)
        {
            ShowMessage(message, false);
        }

        private void ShowMessage(string message, bool modal)
        {
            this.InfoLabel.Text = message;
            this.progressBar1.Visible = false;
            this.YesButton.Visible = false;
            this.NoButton.Visible = false;
            this.OKButton.Visible = true;
            if (this.appIsAlreadyRunning || modal)
            {
                this.Hide();
                DialogResult result = this.ShowDialog();
            }
            else
            {
                this.Show();
            }
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void YesButton_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.wc != null)
                {
                    wc.DownloadProgressChanged -= new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                    wc.DownloadFileCompleted -= new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
                    wc.Dispose();
                    wc = null;
                }

                if (this.mre != null) mre.Close();
                if (this.are != null) are.Close();
                if (this.components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        public class SubscriberInfo
        {
            private SubscriberInfo() { }

            public SubscriberInfo(string name, string author, string version, string description, string packageUrl)
            {
                this.Name = name;
                this.Author = author;
                this.Version = version;
                this.Description = description;
                this.PackageUrl = packageUrl;
            }

            public readonly string Name;
            public readonly string Author;
            public readonly string Version;
            public readonly string Description;
            public readonly string PackageUrl;
            public string LocalZipFileLocation;

            public static SubscriberInfo Parse(string data)
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(data);

                    XmlElement root = xml.DocumentElement;
                    XmlElement nameNode = root["name"];
                    XmlElement authorNode = root["author"];
                    XmlElement versionNode = root["version"];
                    XmlElement descriptionNode = root["description"];
                    XmlElement packageUrlNode = root["packageurl"];

                    string name = nameNode.InnerText.Trim();
                    string author = authorNode.InnerText.Trim();
                    string version = versionNode.InnerText.Trim();
                    string description = descriptionNode.InnerText.Trim();
                    string packageUrl = packageUrlNode.InnerText.Trim();

                    SubscriberInfo info = new SubscriberInfo(name, author, version, description, packageUrl);
                    return info;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}