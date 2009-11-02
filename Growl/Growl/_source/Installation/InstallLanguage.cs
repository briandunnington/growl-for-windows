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
    public partial class InstallLanguage : Form
    {
        private const string USER_AGENT = "Growl for Windows - Lanaguage Pack AutoInstaller";
        private const string TEMP_FOLDER = "__temp";

        private WebClientEx wc;
        private string uri;
        private bool appIsAlreadyRunning;
        private string tempFolder;
        private System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(false);
        private System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
        private DownloadProgressChangedEventArgs progress;
        private object progress_lock = new object();
        private string errorMessage = null;

        public InstallLanguage()
        {
            InitializeComponent();

            // localize text
            this.Text = Properties.Resources.LanguageInstaller_FormTitle;
            //this.InfoLabel.Text gets set below when displayed
            this.YesButton.Text = Properties.Resources.Button_Yes;
            this.NoButton.Text = Properties.Resources.Button_Cancel;
            this.OKButton.Text = Properties.Resources.Button_OK;

            this.BackColor = Color.FromArgb(240, 240, 240);
        }

        public bool LaunchInstaller(string uri, bool appIsAlreadyRunning, ref List<InternalNotification> queuedNotifications, ref int cultureCodeHash)
        {
            bool languageInstalled = false;
            this.uri = uri;
            this.appIsAlreadyRunning = appIsAlreadyRunning;
            this.tempFolder = Path.Combine(Utility.UserSettingFolder, TEMP_FOLDER);

            try
            {
                this.wc = new WebClientEx();
                wc.Headers.Add("User-Agent", USER_AGENT);

                byte[] data = wc.DownloadData(this.uri);
                string definition = Encoding.UTF8.GetString(data).Trim();
                LanguageInfo info = LanguageInfo.Parse(definition);
                if (info != null)
                {
                    this.InfoLabel.Text = String.Format(Utility.GetResourceString(Properties.Resources.LanguageInstaller_Prompt), info.Name);
                    this.YesButton.Visible = true;
                    this.NoButton.Visible = true;
                    this.OKButton.Visible = false;
                    DialogResult result = this.ShowDialog();
                    if (result == DialogResult.Yes)
                    {
                        this.InfoLabel.Text = Utility.GetResourceString(Properties.Resources.LanguageInstaller_Installing);
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

                        Utility.WriteDebugInfo(String.Format("Downloading language pack '{0}' to {1}", info.Name, info.LocalZipFileLocation));

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

                        Utility.WriteDebugInfo(String.Format("Finished downloading language pack '{0}' to {1}", info.Name, info.LocalZipFileLocation));

                        if (this.errorMessage == null)
                        {
                            // unzip files to the correct location
                            string languageFolder = Path.Combine(Application.StartupPath, info.CultureCode);
                            if (!ApplicationMain.HasProgramLaunchedYet || !Directory.Exists(languageFolder))
                            {
                                Utility.WriteDebugInfo(String.Format("Language '{0}' downloaded - starting unzip.", info.Name));
                                Unzipper.UnZipFiles(info.LocalZipFileLocation, languageFolder, false);

                                //InternalNotification n = new InternalNotification(Properties.Resources.DisplayInstaller_NewDisplayInstalledTitle, String.Format(Utility.GetResourceString(Properties.Resources.DisplayInstaller_NewDisplayInstalledText), info.Name), info.Name);
                                string text = String.Format(Properties.Resources.LanguageInstaller_LanguageInstalledText, info.Name);
                                if (ApplicationMain.HasProgramLaunchedYet) text += (" " + Properties.Resources.LanguageInstaller_RestartRequiredText);
                                InternalNotification n = new InternalNotification(Properties.Resources.LanguageInstaller_LanguageInstalledTitle, Utility.GetResourceString(text), null);
                                queuedNotifications.Add(n);

                                Properties.Settings.Default.CultureCode = info.CultureCode;
                                cultureCodeHash = info.CultureCode.GetHashCode();

                                languageInstalled = true;

                                this.Close();
                            }
                            else
                            {
                                // display with the same name aleady exists...
                                ShowMessage(String.Format(Utility.GetResourceString(Properties.Resources.LanguageInstaller_AlreadyInstalled), info.Name));
                            }

                            // clean up
                            Utility.WriteDebugInfo(String.Format("Deleteing '{0}' zip file at {1}", info.Name, info.LocalZipFileLocation));
                            if (File.Exists(info.LocalZipFileLocation)) File.Delete(info.LocalZipFileLocation);
                        }
                        else
                        {
                            Utility.WriteDebugInfo(String.Format("Error downloading language pack '{0}'.", info.Name));
                            ShowMessage(errorMessage);
                        }
                    }
                }
                else
                {
                    // definition file was malformed
                    ShowMessage(String.Format(Utility.GetResourceString(Properties.Resources.LanguageInstaller_BadDefinitionFile), this.uri));
                }
            }
            catch (Exception ex)
            {
                // error downloading definition file
                Utility.WriteDebugInfo(String.Format("Error downloading language pack. {0} - {1}", ex.Message, ex.StackTrace));
                ShowMessage(String.Format(Utility.GetResourceString(Properties.Resources.LanguageInstaller_NonexistentDefinitionFile), this.uri));
            }
            return languageInstalled;
        }

        private void StartDownload(object obj)
        {
            System.Diagnostics.Debug.WriteLine("InstallLanguage.StartDownload thread: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            LanguageInfo info = (LanguageInfo)obj;
            this.wc.DownloadFileAsync(new Uri(info.PackageUrl), info.LocalZipFileLocation, info);
        }

        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.WriteDebugInfo(e.Error.Message);
                Utility.WriteDebugInfo(e.Error.StackTrace);
                this.errorMessage = Utility.GetResourceString(Properties.Resources.LanguageInstaller_DownloadError);
            }
            else if (e.Cancelled)
            {
                this.errorMessage = Utility.GetResourceString(Properties.Resources.LanguageInstaller_DownloadCancelled);
            }
            else
            {
                // sometimes the downloaded file is still being written to disk.
                // this will wait until the file is readable before returning.
                LanguageInfo info = (LanguageInfo)e.UserState;
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

        public class LanguageInfo
        {
            private LanguageInfo() { }

            public LanguageInfo(string name, string cultureCode, string packageUrl)
            {
                this.Name = name;
                this.CultureCode = cultureCode;
                this.PackageUrl = packageUrl;
            }

            public readonly string Name;
            public readonly string CultureCode;
            public readonly string PackageUrl;
            public string LocalZipFileLocation;

            public static LanguageInfo Parse(string data)
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(data);

                    XmlElement root = xml.DocumentElement;
                    XmlElement nameNode = root["name"];
                    XmlElement cultureCodeNode = root["culture"];
                    XmlElement packageUrlNode = root["packageurl"];

                    string name = nameNode.InnerText.Trim();
                    string cultureCode = cultureCodeNode.InnerText.Trim();
                    string packageUrl = packageUrlNode.InnerText.Trim();

                    LanguageInfo info = new LanguageInfo(name, cultureCode, packageUrl);
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