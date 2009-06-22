using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Growl.Installation
{
    public partial class InstallDisplay : Form
    {
        private const string USER_AGENT = "Growl for Windows - Display AutoInstaller";
        private const string TEMP_FOLDER = "__temp";

        private WebClient wc;
        private string uri;
        private bool appIsAlreadyRunning;
        private string tempFolder;
        private System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);
        private System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
        private DownloadProgressChangedEventArgs progress;
        private object progress_lock = new object();
        private string errorMessage = null;

        public InstallDisplay()
        {
            InitializeComponent();

            // localize text
            //this.Text = Properties.Resources.Updater_FormTitle;
            //this.InfoLabel.Text = Properties.Resources.Updater_GrowlIsUpToDate;
            this.YesButton.Text = Properties.Resources.Button_Yes;
            this.NoButton.Text = Properties.Resources.Button_Later;
            this.OKButton.Text = Properties.Resources.Button_OK;

            this.BackColor = Color.FromArgb(240, 240, 240);
        }

        public bool LaunchInstaller(string uri, bool appIsAlreadyRunning, ref List<InternalNotification> queuedNotifications)
        {
            bool newDisplayLoaded = false;
            this.uri = uri;
            this.appIsAlreadyRunning = appIsAlreadyRunning;
            this.tempFolder = Path.Combine(Utility.UserSettingFolder, TEMP_FOLDER);

            try
            {
                this.wc = new WebClient();
                wc.Headers.Add("User-Agent", USER_AGENT);

                string definition = wc.DownloadString(this.uri);
                DisplayInfo info = DisplayInfo.Parse(definition);
                if (info != null)
                {
                    this.InfoLabel.Text = String.Format("Do you want to install the following display?\n\nName: {0}\nAuthor: {1}\nDescription: {2}", info.Name, info.Author, info.Description);
                    this.YesButton.Visible = true;
                    this.NoButton.Visible = true;
                    this.OKButton.Visible = false;
                    DialogResult result = this.ShowDialog();
                    if (result == DialogResult.Yes)
                    {
                        this.InfoLabel.Text = "Installing display...";
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

                        System.Threading.ParameterizedThreadStart pts = new System.Threading.ParameterizedThreadStart(StartDownload);
                        System.Threading.Thread t = new System.Threading.Thread(pts);
                        t.Start(info);

                        Utility.WriteDebugInfo(String.Format("Downloading display '{0}' to {1}", info.Name, info.LocalZipFileLocation));

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

                        Utility.WriteDebugInfo(String.Format("Finished downloading display '{0}' to {1}", info.Name, info.LocalZipFileLocation));

                        if (this.errorMessage == null)
                        {
                            // unzip files to the correct location
                            string newDisplayFolder = Path.Combine(DisplayStyleManager.UserDisplayStyleDirectory, Growl.CoreLibrary.PathUtility.GetSafeFolderName(info.Name));
                            if (!Directory.Exists(newDisplayFolder))
                            {
                                Utility.WriteDebugInfo(String.Format("Display '{0}' downloaded - starting unzip.", info.Name));
                                Unzipper.UnZipFiles(info.LocalZipFileLocation, newDisplayFolder, false);

                                //ShowMessage(String.Format("The display '{0}' was installed successfully.", info.Name));

                                InternalNotification n = new InternalNotification("New Display Installed", String.Format("The display '{0}' was installed successfully.", info.Name), info.Name);
                                queuedNotifications.Add(n);

                                newDisplayLoaded = true;

                                this.Close();
                            }
                            else
                            {
                                // display with the same name aleady exists...
                                // TODO: ??
                                ShowMessage(String.Format("Display '{0}' is already installed.", info.Name));
                            }

                            // clean up
                            Utility.WriteDebugInfo(String.Format("Deleteing '{0}' zip file at {1}", info.Name, info.LocalZipFileLocation));
                            if (File.Exists(info.LocalZipFileLocation)) File.Delete(info.LocalZipFileLocation);
                        }
                        else
                        {
                            Utility.WriteDebugInfo(String.Format("Error downloading display '{0}'.", info.Name));
                            ShowMessage(errorMessage);
                        }
                    }
                }
                else
                {
                    // definition file was malformed
                    ShowMessage(String.Format("The definition file '{0}' is invalid.\n\nThe display could not be installed.", this.uri));
                }
            }
            catch (Exception ex)
            {
                // error downloading definition file
                Utility.WriteDebugInfo(String.Format("Error downloading display. {0} - {1}", ex.Message, ex.StackTrace));
                ShowMessage(String.Format("The definition file '{0}' does not exist.\n\nThe display could not be installed.", this.uri));
            }
            return newDisplayLoaded;
        }

        private void StartDownload(object obj)
        {
            System.Diagnostics.Debug.WriteLine("sd thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            DisplayInfo info = (DisplayInfo)obj;
            this.wc.DownloadFileAsync(new Uri(info.PackageUrl), info.LocalZipFileLocation, info);
        }

        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.errorMessage = "An error occurred while downloading the display files.\n\nThe display was not installed.";
            }
            else if (e.Cancelled)
            {
                this.errorMessage = "The installation of the display was cancelled.\n\nThe display was not installed.";
            }

            // sometimes the downloaded file is still being written to disk.
            // this will wait until the file is readable before returning.
            DisplayInfo info = (DisplayInfo)e.UserState;
            bool fileAvailable = false;
            int counter = 0;
            while(!fileAvailable && counter < 10)
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
            this.InfoLabel.Text = message;
            this.progressBar1.Visible = false;
            this.YesButton.Visible = false;
            this.NoButton.Visible = false;
            this.OKButton.Visible = true;
            if (this.appIsAlreadyRunning)
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
                if (this.mre != null) mre.Close();
                if (this.are != null) are.Close();
                if (this.components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        public class DisplayInfo
        {
            private DisplayInfo() { }

            public DisplayInfo(string name, string author, string version, string description, string packageUrl)
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

            public static DisplayInfo Parse(string data)
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

                    DisplayInfo info = new DisplayInfo(name, author, version, description, packageUrl);
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