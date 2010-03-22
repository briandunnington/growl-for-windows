using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace Growl
{
    [Serializable]
    public class PastNotification : IComparable, IDisposable
    {
        private DateTime timestamp;
        private Growl.DisplayStyle.NotificationLite notification;
        private string imageFile;

        internal PastNotification(Growl.DisplayStyle.NotificationLite nl, DateTime timestamp, string imageFile)
        {
            this.notification = nl;
            this.timestamp = timestamp;
            this.imageFile = imageFile;
        }

        public Growl.DisplayStyle.NotificationLite Notification
        {
            get
            {
                return this.notification;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return this.timestamp;
            }
        }

        public bool HasImage
        {
            get
            {
                return !String.IsNullOrEmpty(this.ImageFile);
            }
        }

        public string ImageFile
        {
            get
            {
                return this.imageFile;
            }
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            PastNotification pn = obj as PastNotification;
            if (obj == null || pn == null) return -1;
            return -this.Timestamp.CompareTo(pn.Timestamp);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
        }

        #endregion
    }

    public class PastNotificationComparer : System.Collections.Generic.IComparer<PastNotification>
    {
        #region IComparer<PastNotification> Members

        public int Compare(PastNotification x, PastNotification y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.Timestamp.CompareTo(y.Timestamp);
        }

        #endregion
    }
}
