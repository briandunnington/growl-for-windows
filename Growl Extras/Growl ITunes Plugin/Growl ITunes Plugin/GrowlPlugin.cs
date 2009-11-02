using System;
using System.Text;
using iTunesLib;
using ITunesPluginApp;

namespace GrowlExtras.ITunesPlugin
{
    internal class GrowlPlugin : ITunesHandler
    {
        // old
        private Growl.UDPLegacy.MessageSender udpGrowl;
        private Growl.UDPLegacy.NotificationType udpNotificationType;
        private Growl.UDPLegacy.NotificationType[] udpNotificationTypes;

        // new
        private Growl.Connector.GrowlConnector growl;
        private Growl.Connector.Application application;
        private Growl.CoreLibrary.BinaryData iconData;
        Growl.Connector.NotificationType nt1;
        Growl.Connector.NotificationType[] notificationTypes;

        public GrowlPlugin()
        {
            this.OnPlayerPlayEvent += new _IiTunesEvents_OnPlayerPlayEventEventHandler(growlPlugin_OnPlayerPlayEvent);
            this.OnPlayerPlayingTrackChangedEvent += new _IiTunesEvents_OnPlayerPlayingTrackChangedEventEventHandler(growlPlugin_OnPlayerPlayingTrackChangedEvent);

            // old
            this.udpGrowl = new Growl.UDPLegacy.MessageSender("ITunes Growl Plug-in", Properties.Settings.Default.GrowlPassword);
            this.udpNotificationType = new Growl.UDPLegacy.NotificationType("Track Changed", true);
            this.udpNotificationTypes = new Growl.UDPLegacy.NotificationType[] { this.udpNotificationType };
            
            // new
            this.application = new Growl.Connector.Application("iTunes");
            this.application.Icon = String.Format(@"{0}\icon.png", System.Windows.Forms.Application.StartupPath);
            this.growl = new Growl.Connector.GrowlConnector(Properties.Settings.Default.GrowlPassword);
            this.growl.NotificationCallback +=new Growl.Connector.GrowlConnector.CallbackEventHandler(growl_NotificationCallback);
            this.growl.EncryptionAlgorithm = Growl.Connector.Cryptography.SymmetricAlgorithmType.PlainText;
            this.nt1 = new Growl.Connector.NotificationType("Track Changed", "Track Changed");
            this.notificationTypes = new Growl.Connector.NotificationType[] { this.nt1 };
        }

        internal void RegisterGNTP()
        {
            growl.Register(this.application, this.notificationTypes);
        }

        internal void RegisterUDP()
        {
            udpGrowl.Register(ref this.udpNotificationTypes);
        }

        internal void SetPassword(string password)
        {
            this.growl.Password = password;
            this.udpGrowl.Password = password;
        }

        void growl_NotificationCallback(Growl.Connector.Response response, Growl.Connector.CallbackData callbackData)
        {
            if (callbackData != null)
            {
                if (callbackData.Result == Growl.CoreLibrary.CallbackResult.CLICK)
                {
                    if (response.CustomTextAttributes.ContainsKey("Rating"))
                    {
                        string r = response.CustomTextAttributes["Rating"];
                        int rating = Convert.ToInt32(r);

                        string[] parts = callbackData.Data.Split('|');
                        int sourceID = Convert.ToInt32(parts[0]);
                        int playlistID = Convert.ToInt32(parts[1]);
                        int trackID = Convert.ToInt32(parts[2]);
                        int databaseID = Convert.ToInt32(parts[3]);

                        IITTrack song = (IITTrack) this.GetITObjectByID(sourceID, playlistID, trackID, databaseID);
                        if (song != null)
                        {
                            song.Rating = rating;
                        }
                    }
                }
            }
        }

        void growlPlugin_OnPlayerPlayEvent(object iTrack)
        {
            Notify(iTrack);
        }

        void growlPlugin_OnPlayerPlayingTrackChangedEvent(object iTrack)
        {
            Notify(iTrack);
        }

        void Notify(object iTrack)
        {
            if (!Properties.Settings.Default.DisableNotifications)
            {
                IITTrack song = (IITTrack)iTrack;
                string title = Escape(song.Name);
                string text = String.Format("{0}\n{1}", Escape(song.Artist), Escape(song.Album));
                // this handles streaming radio stations
                if (this.CurrentStreamTitle != null)
                {
                    title = Escape(this.CurrentStreamTitle);
                    text = String.Format("Station: {0}", Escape(song.Name));
                }

                // TODO:
                string artworkFilePath = null;
                byte[] artworkData = null;
                if (song.Artwork != null && song.Artwork.Count > 0)
                {
                    string safeAlbumName = GetSafeFileName(song.Album);
                    string filename = String.Format("{0}.jpg", safeAlbumName);
                    artworkFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), filename);
                    if (!System.IO.File.Exists(artworkFilePath))
                        song.Artwork[1].SaveArtworkToFile(artworkFilePath);
                    artworkData = System.IO.File.ReadAllBytes(artworkFilePath);
                }

                // old
                if (Properties.Settings.Default.SendUDPNotifications)
                {
                    udpGrowl.Notify(udpNotificationType, title, text, Growl.Connector.Priority.Normal, false);
                }

                // new
                if (Properties.Settings.Default.SendGNTPNotifications)
                {
                    int sourceID;
                    int playlistID;
                    int trackID;
                    int databaseID;
                    song.GetITObjectIDs(out sourceID, out playlistID, out trackID, out databaseID);

                    Growl.Connector.CallbackContext callback = new Growl.Connector.CallbackContext("song", String.Format("{0}|{1}|{2}|{3}", sourceID, playlistID, trackID, databaseID));

                    //Growl.CoreLibrary.Resource albumIcon = artworkFilePath;
                    Growl.CoreLibrary.Resource albumIcon = (artworkData != null ? new Growl.CoreLibrary.BinaryData(artworkData) : null);
                    Growl.Connector.Notification notification = new Growl.Connector.Notification(application.Name, nt1.Name, callback.Data, title, text, albumIcon, false, Growl.Connector.Priority.Normal, null);

                    notification.CustomTextAttributes.Add("iTunes-Artist", song.Artist);
                    notification.CustomTextAttributes.Add("iTunes-Album", song.Album);
                    notification.CustomTextAttributes.Add("iTunes-Duration", song.Duration.ToString());
                    notification.CustomTextAttributes.Add("iTunes-PlayedCount", song.PlayedCount.ToString());
                    notification.CustomTextAttributes.Add("iTunes-Genre", song.Genre);
                    notification.CustomTextAttributes.Add("iTunes-Rating", song.Rating.ToString());

                    growl.Notify(notification, callback);
                }
            }
        }

        private string Escape(string s)
        {
            // do nothing for now
            return s;
        }

        /// <summary>
        /// Gets a file name consisting of filename-safe characters.
        /// </summary>
        /// <param name="name">The string to base the filename on</param>
        /// <returns><paramref name="name"/> with any invalid characters removed</returns>
        public static string GetSafeFileName(string name)
        {
            char[] disallowedChars = System.IO.Path.GetInvalidFileNameChars();
            return GetSafeName(name, disallowedChars);
        }

        /// <summary>
        /// Removes any <paramref name="disallowedChars"/> in <paramref name="name"/>
        /// and returns the resulting string.
        /// </summary>
        /// <param name="name">The string to base the safe name on</param>
        /// <param name="disallowedChars">array of <see cref="char"/>s to replace</param>
        /// <returns></returns>
        private static string GetSafeName(string name, char[] disallowedChars)
        {
            string safe = name;
            foreach (char disallowed in disallowedChars)
            {
                safe = safe.Replace(disallowed.ToString(), "");
            }
            return safe;
        }
    }
}
