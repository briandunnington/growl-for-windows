using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Net;

namespace Growl.AutoUpdate
{
    /* in the actual app:
 * - add this component
 * - call 'CheckForUpdate'
 * - if no update available, do nothing
 * - if update is available, show form to prompt user
 * - if user says no, do nothing
 * - if user says yes, download .msi
 * - when download is complete, launch GrowlAuto.Update.exe and **terminate this app**
 * 
 * in Growl.AutoUpdate.exe:
 * - wait a bit for original app to close
 * - launch .msi file
 * - close this program
 * */
    public class Updater
    {
        private const string UPDATE_FOLDER = "__update";
        private const string MANIFEST_FILE_NAME = "update.manifest";

        public event CheckForUpdateCompleteEventHandler CheckForUpdateComplete;
        public event ProgressChangedEventHandler DownloadProgressChanged;
        public event EventHandler DownloadComplete;
        public event UpdateErrorEventHandler UpdateError;

        private WebClient checker;
        private string appPath;
        private string manifestFile;
        private string currentVersion;
        private string updateLocation;
        private Manifest updatedManifest;
        private bool updateAvailable;
        private string updateTempFolder;

        public Updater(string appPath)
            : this(appPath, null, null)
        {
            ReadCurrentManifest();
        }

        public Updater(string appPath, string currentVersion, string updateLocation)
        {
            this.appPath = appPath;
            this.manifestFile = Path.Combine(this.appPath, MANIFEST_FILE_NAME);
            this.currentVersion = currentVersion;
            this.updateLocation = updateLocation;
            this.updateTempFolder = Path.Combine(this.appPath, UPDATE_FOLDER);

            this.checker = new WebClient();
            checker.Headers.Add("User-Agent", "Element.AutoUpdate.Updater");
            checker.DownloadStringCompleted += new DownloadStringCompletedEventHandler(checker_DownloadStringCompleted);
        }

        public string CurrentVersion
        {
            get
            {
                return this.currentVersion;
            }
        }

        public void CheckForUpdate(bool userInitiated)
        {
            if (!String.IsNullOrEmpty(this.updateLocation))
            {
                string qs = "v=" + this.currentVersion;
                UriBuilder ub = new UriBuilder(this.updateLocation);
                if (ub.Query.Length > 1)
                    qs = ub.Query.Substring(1) + qs;
                ub.Query = qs;

                if (checker.IsBusy) checker.CancelAsync();
                checker.DownloadStringAsync(ub.Uri, userInitiated);
            }
        }

        void checker_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                CheckForUpdateCompleteEventArgs args;
                bool userInitiated = (bool)e.UserState;

                if (e.Error == null)
                {
                    this.updatedManifest = Manifest.Parse(e.Result);
                    args = new CheckForUpdateCompleteEventArgs(this.updatedManifest, this.currentVersion, userInitiated, null);
                    this.updateAvailable = args.UpdateAvailable;
                }
                else
                {
                    UpdateErrorEventArgs errorArgs = new UpdateErrorEventArgs(e.Error, "Growl was unable to determine if a newer version is available. Please try again later.");
                    args = new CheckForUpdateCompleteEventArgs(null, this.currentVersion, userInitiated, errorArgs);
                }
                this.OnCheckForUpdateComplete(args);
            }
        }

        public void Update()
        {
            if (this.updatedManifest == null)
                throw new UpdateException("You must call CheckForUpdate first to determine if an update is available.");

            if (this.updateAvailable)
            {
                if (Directory.Exists(this.updateTempFolder))
                    Directory.Delete(updateTempFolder, true);
                Directory.CreateDirectory(this.updateTempFolder);
                string installerFileName = Path.Combine(updateTempFolder, "update.msi");

                WebClient downloader = new WebClient();
                downloader.Headers.Add("User-Agent", "Element.AutoUpdate.Updater");
                downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
                downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
                downloader.DownloadFileAsync(new Uri(this.updatedManifest.InstallerLocation), installerFileName, installerFileName);
            }
        }

        void downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressChangedEventArgs args = new ProgressChangedEventArgs(e.ProgressPercentage, null);
            this.OnDownloadProgressChanged(args);
        }

        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                UpdateErrorEventArgs args = new UpdateErrorEventArgs(e.Error, "An error occurred while downloading the necessary update files. Please try again later.");
                this.OnUpdateError(args);
            }
            else if (e.Cancelled)
            {
                UpdateException ex = new UpdateException("Update was cancelled");
                UpdateErrorEventArgs args = new UpdateErrorEventArgs(ex, "Update cancelled.");
                this.OnUpdateError(args);
            }
            else
            {
                this.OnDownloadComplete(EventArgs.Empty);

                // start the update installer
                string msiFile = (string)e.UserState;
                //string appUpdaterFile = this.updater.AppUpdaterEXE;
                System.Diagnostics.ProcessStartInfo si = new System.Diagnostics.ProcessStartInfo(msiFile);
                System.Diagnostics.Process.Start(si);

                // exit this application
                ApplicationMain.Program.ExitApp();
            }
        }

        protected void OnCheckForUpdateComplete(CheckForUpdateCompleteEventArgs args)
        {
            if (this.CheckForUpdateComplete != null)
            {
                this.CheckForUpdateComplete(this, args);
            }
        }

        protected void OnDownloadProgressChanged(ProgressChangedEventArgs args)
        {
            if (this.DownloadProgressChanged != null)
            {
                this.DownloadProgressChanged(this, args);
            }
        }

        protected void OnDownloadComplete(EventArgs e)
        {
            if (this.DownloadComplete != null)
            {
                this.DownloadComplete(this, e);
            }
        }

        protected void OnUpdateError(UpdateErrorEventArgs e)
        {
            if (this.UpdateError != null)
            {
                this.UpdateError(this, e);
            }
        }

        private void ReadCurrentManifest()
        {
            bool exists = File.Exists(this.manifestFile);
            if (exists)
            {
                string xml = File.ReadAllText(this.manifestFile);
                Manifest manifest = Manifest.Parse(xml);
                this.currentVersion = manifest.Version;
                this.updateLocation = manifest.UpdateLocation;
            }
        }
    }
}
