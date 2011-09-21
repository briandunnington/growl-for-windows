using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using Growl.Destinations;

namespace Growl
{
    [Serializable]
    public class ToastyForwardDestination : ForwardDestination
    {
        private const string NOTIFY_URL_FORMAT = "http://api.supertoasty.com/notify/{0}";

        private static QuietHoursDayList QuietHoursEveryday = new QuietHoursDayList(QuietHoursDayChoice.Everyday, new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday });
        private static QuietHoursDayList QuietHoursWeekdays = new QuietHoursDayList(QuietHoursDayChoice.Weekdays, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday });
        private static QuietHoursDayList QuietHoursWeekends = new QuietHoursDayList(QuietHoursDayChoice.Weekends, new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Saturday });

        private static DateTime DefaultQuietHoursStartTime = new DateTime(2010, 1, 1, 22, 0, 0);
        private static DateTime DefaultQuietHoursEndTime = DefaultQuietHoursStartTime.AddHours(1);

        private string deviceID;
        private Growl.Connector.Priority? minimumPriority = null;
        private bool onlyWhenIdle;
        private bool enableQuietHours;
        private DateTime quietHoursStart = DefaultQuietHoursStartTime;
        private DateTime quietHoursEnd = DefaultQuietHoursEndTime;
        private QuietHoursDayChoice quietHoursDaysChoice = QuietHoursDayChoice.Everyday;

        public ToastyForwardDestination(string name, bool enabled, string deviceID, Growl.Connector.Priority? minimumPriority, bool onlyWhenIdle, bool enableQuietHours, DateTime quietHoursStart, DateTime quietHoursEnd, QuietHoursDayChoice quietHoursDaysChoice)
            : base(name, enabled)
        {
            this.deviceID = deviceID;
            this.minimumPriority = minimumPriority;
            this.onlyWhenIdle = onlyWhenIdle;
            this.enableQuietHours = enableQuietHours;
            this.quietHoursStart = quietHoursStart;
            this.quietHoursEnd = quietHoursEnd;
            this.quietHoursDaysChoice = quietHoursDaysChoice;
        }

        public string DeviceID
        {
            get
            {
                return this.deviceID;
            }
            set
            {
                this.deviceID = value;
            }
        }

        public Growl.Connector.Priority? MinimumPriority
        {
            get
            {
                return this.minimumPriority;
            }
            set
            {
                this.minimumPriority = value;
            }
        }

        public bool OnlyWhenIdle
        {
            get
            {
                return this.onlyWhenIdle;
            }
            set
            {
                this.onlyWhenIdle = value;
            }
        }

        public bool EnableQuietHours
        {
            get
            {
                return this.enableQuietHours;
            }
            set
            {
                this.enableQuietHours = value;
            }
        }

        public DateTime QuietHoursStart
        {
            get
            {
                // the DateTimePicker we use to input this value only suports a minvalue = sqldatetime.minvalue
                if (this.quietHoursStart < System.Data.SqlTypes.SqlDateTime.MinValue.Value)
                    this.quietHoursStart = DefaultQuietHoursStartTime;

                return this.quietHoursStart;
            }
            set
            {
                this.quietHoursStart = value;
            }
        }

        public DateTime QuietHoursEnd
        {
            get
            {
                // the DateTimePicker we use to input this value only suports a minvalue = sqldatetime.minvalue
                if (this.quietHoursEnd < System.Data.SqlTypes.SqlDateTime.MinValue.Value)
                    this.quietHoursEnd = DefaultQuietHoursEndTime;

                return this.quietHoursEnd;
            }
            set
            {
                this.quietHoursEnd = value;
            }
        }

        public QuietHoursDayChoice QuietHoursDaysChoice
        {
            get
            {
                return this.quietHoursDaysChoice;
            }
            set
            {
                this.quietHoursDaysChoice = value;
            }
        }

        private QuietHoursDayList QuietHoursDays
        {
            get
            {
                switch (this.QuietHoursDaysChoice)
                {
                    case QuietHoursDayChoice.Weekdays :
                        return QuietHoursWeekdays;
                    case QuietHoursDayChoice.Weekends:
                        return QuietHoursWeekends;
                    default:
                        return QuietHoursEveryday;
                }
            }
        }

        public override bool Available
        {
            get
            {
                return true;
            }
            protected set
            {
                throw new NotSupportedException("The .Available property is read-only.");
            }
        }

        public override string AddressDisplay
        {
            get
            {
                string priorityText = ToastyForwardDestinationHandler.Fetch(this.MinimumPriority);
                string priorityDisplay = (this.MinimumPriority != null && this.MinimumPriority.HasValue ? priorityText : "Any Priority");
                string idleDisplay = (this.OnlyWhenIdle ? "Idle Only" : "Always");
                return String.Format("({1}/{2}) - {0}", this.DeviceID, priorityDisplay, idleDisplay);
            }
        }

        public override DestinationBase Clone()
        {
            ToastyForwardDestination clone = new ToastyForwardDestination(this.Description, this.Enabled, this.DeviceID, this.MinimumPriority, this.OnlyWhenIdle, this.EnableQuietHours, this.QuietHoursStart, this.QuietHoursEnd, this.QuietHoursDaysChoice);
            return clone;
        }

        /// <summary>
        /// Gets the icon that represents this type of forwarder.
        /// </summary>
        /// <returns><see cref="System.Drawing.Image"/></returns>
        public override System.Drawing.Image GetIcon()
        {
            return ToastyForwardDestinationHandler.GetIcon();
        }

        private bool DuringQuietHours()
        {
            bool result = false;
            if (this.EnableQuietHours)
            {
                DateTime now = DateTime.Now;

                DateTime qs = new DateTime(now.Year, now.Month, now.Day, this.QuietHoursStart.Hour, this.QuietHoursStart.Minute, 0);
                DateTime qe = new DateTime(now.Year, now.Month, now.Day, this.QuietHoursEnd.Hour, this.QuietHoursEnd.Minute, 0);
                // adjust for values that span midnight
                if (qe < qs)
                {
                    if (now > qs) qe = qe.AddDays(1); // we are dealing with today's block
                    else qs = qs.AddDays(-1); // we are dealing with yesterday's wrapped block
                }
                DayOfWeek dow = qs.DayOfWeek;

                List<DayOfWeek> list = new List<DayOfWeek>(this.QuietHoursDays.Days);
                if (list.Contains(dow))
                {
                    if (now >= qs && now <= qe)
                        result = true;
                }
            }
            return result;
        }

        public override void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Connector.RequestInfo requestInfo, bool isIdle)
        {
            // IGNORE REGISTRATION NOTIFICATIONS (since we have no way of filtering out already-registered apps at this point)
            //Send(application.Name, Properties.Resources.SystemNotification_AppRegistered_Title, String.Format(Properties.Resources.SystemNotification_AppRegistered_Text, application.Name));

            requestInfo.SaveHandlingInfo("Forwarding to Toasty cancelled - Application Registrations are not forwarded.");
        }

        public override void ForwardNotification(Growl.Connector.Notification notification, Growl.Connector.CallbackContext callbackContext, Growl.Connector.RequestInfo requestInfo, bool isIdle, ForwardedNotificationCallbackHandler callbackFunction)
        {
            bool send = true;

            if (requestInfo == null) requestInfo = new Growl.Connector.RequestInfo();

            // if this notification originated from Toasty in the first place, dont re-forward it
            if (((notification.ApplicationName == "Toasty") && notification.CustomTextAttributes.ContainsKey("ToastyDeviceID")) && notification.CustomTextAttributes["ToastyDeviceID"].Equals(this.DeviceID, StringComparison.InvariantCultureIgnoreCase))
            {
                requestInfo.HandlingInfo.Add(String.Format("Aborted forwarding due to circular notification (deviceID:{0})", this.DeviceID));
                send = false;
            }

            // if a minimum priority is set, check that
            if (send && this.MinimumPriority != null && this.MinimumPriority.HasValue && notification.Priority < this.MinimumPriority.Value)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Toasty ({0}) cancelled - Notification priority must be at least '{1}' (was actually '{2}').", this.Description, this.MinimumPriority.Value.ToString(), notification.Priority.ToString()));
                send = false;
            }

            // if only sending when idle, check that
            if (send && this.OnlyWhenIdle && !isIdle)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Toasty ({0}) cancelled - Currently only configured to forward when idle", this.Description));
                send = false;
            }

            // if quiet hours enabled, check that
            if (send && DuringQuietHours())
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarding to Toasty ({0}) cancelled - Quiet hours enabled on {1} from {2} to {3}. Current time: {4} {5}", this.Description, this.QuietHoursDaysChoice, this.QuietHoursStart.ToShortTimeString(), this.QuietHoursEnd.ToShortTimeString(), DateTime.Now.DayOfWeek, DateTime.Now.ToShortTimeString()));
                send = false;
            }

            if (send)
            {
                requestInfo.SaveHandlingInfo(String.Format("Forwarded to Toasty '{0}' - Minimum Priority:'{1}', Actual Priority:'{2}'", this.Description, (this.MinimumPriority != null && this.MinimumPriority.HasValue ? this.MinimumPriority.Value.ToString() : "<any>"), notification.Priority.ToString()));

                string url = BuildUrl(NOTIFY_URL_FORMAT, this.DeviceID);

                System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
                data.Add("target", this.DeviceID);
                data.Add("sender", notification.ApplicationName);
                data.Add("title", notification.Title);
                data.Add("text", notification.Text);

                byte[] bytes = null;
                if (notification.Icon != null && notification.Icon.IsSet)
                {
                    System.Drawing.Image image = (System.Drawing.Image)notification.Icon;
                    using (image)
                    {
                        bytes = GenerateThumbnail(image, 128, 128);
                    }
                }

                APIRequestInfo info = new APIRequestInfo();
                info.RequestInfo = requestInfo;
                info.Url = url;
                info.Data = data;
                info.ImageBytes = bytes;

                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SendAsync), info);
            }
        }

        private void SendAsync(object state)
        {
            System.IO.MemoryStream ms = null;

            try
            {
                APIRequestInfo info = (APIRequestInfo)state;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(info.Url);
                request.UserAgent = "Growl for Windows/2.0";
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ServicePoint.Expect100Continue = false;

                byte[] file = info.ImageBytes;
                byte[] postBody = GetPostBody(info.Data, file, ref request);    // this also handles updating the content type with the correct boundary information
                request.ContentLength = postBody.Length;

                Utility.WriteDebugInfo(System.Text.Encoding.UTF8.GetString(postBody));

                System.IO.Stream stream = request.GetRequestStream();
                using (stream)
                {
                    stream.Write(postBody, 0, postBody.Length);
                }

                string responseText = null;
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                using (response)
                {
                    System.IO.Stream responseStream = response.GetResponseStream();
                    using (responseStream)
                    {
                        System.IO.StreamReader reader = new System.IO.StreamReader(responseStream);
                        using (reader)
                        {
                            responseText = reader.ReadToEnd();
                        }
                    }
                }
                Utility.WriteDebugInfo(responseText);
            }
            catch (Exception ex)
            {
                Utility.WriteDebugInfo(String.Format("Toasty forwarding failed: {0}", ex.Message));
            }
            finally
            {
                if (ms != null)
                {
                    ms.Dispose();
                    ms = null;
                }
            }
        }

        private static string BuildUrl(string format, params object[] parts)
        {
            if (parts != null)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    object o = parts[i];
                    if (o != null) parts[i] = System.Web.HttpUtility.UrlEncode(o.ToString());
                }
            }
            return String.Format(format, parts);
        }

        private static byte[] GenerateThumbnail(System.Drawing.Image originalImage, int newWidth, int newHeight)
        {
            byte[] bytes = null;

            if (originalImage != null)
            {
                // dont change the size if it is already smaller than we need
                if (originalImage.Width < newWidth)
                {
                    newWidth = originalImage.Width;
                    newHeight = originalImage.Height;
                }

                System.Drawing.Bitmap bmpResized = new System.Drawing.Bitmap(newWidth, newHeight);
                using (bmpResized)
                {
                    lock (bmpResized)
                    {
                        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmpResized);
                        using (g)
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                            g.DrawImage(
                                originalImage,
                                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmpResized.Size),
                                new System.Drawing.Rectangle(System.Drawing.Point.Empty, originalImage.Size),
                                System.Drawing.GraphicsUnit.Pixel);
                        }
                    }

                    System.IO.MemoryStream stream = new System.IO.MemoryStream();
                    using (stream)
                    {
                        bmpResized.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        bytes = stream.GetBuffer();
                    }
                }
            }

            return bytes;
        }

        private byte[] GetPostBody(System.Collections.Specialized.NameValueCollection data, byte[] file, ref HttpWebRequest request)
        {
            if (file == null)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Count; i++)
                {
                    sb.AppendFormat("{0}={1}&", data.Keys[i], System.Web.HttpUtility.UrlEncode(data[i]));
                }
                string s =  sb.ToString();
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
                return bytes;
            }
            else
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

                request.ContentType = "multipart/form-data; boundary=" + boundary;
                request.Method = "POST";

                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("--" + boundary + "\r\n");

                System.IO.MemoryStream memStream = new System.IO.MemoryStream();
                memStream.Write(boundarybytes, 0, boundarybytes.Length);

                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n";
                foreach (string key in data.Keys)
                {
                    string formitem = string.Format(formdataTemplate, key, data[key]);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    memStream.Write(formitembytes, 0, formitembytes.Length);
                    memStream.Write(boundarybytes, 0, boundarybytes.Length);
                }

                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: image/x-png\r\n\r\n";
                string header = string.Format(headerTemplate, "image", "image.png");
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                memStream.Write(headerbytes, 0, headerbytes.Length);
                memStream.Write(file, 0, file.Length);

                byte[] endboundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                memStream.Write(endboundarybytes, 0, endboundarybytes.Length);

                byte[] bytes = memStream.GetBuffer();
                return bytes;
            }
        }

        private class APIRequestInfo
        {
            public Growl.Connector.RequestInfo RequestInfo;
            public string Url;
            public System.Collections.Specialized.NameValueCollection Data;
            public byte[] ImageBytes;
        }

        public enum QuietHoursDayChoice
        {
            Everyday = 0,
            Weekdays = 1,
            Weekends = 2
        }

        public class QuietHoursDayList
        {
            private QuietHoursDayChoice choice;
            private DayOfWeek[] days;

            public QuietHoursDayList(QuietHoursDayChoice choice, DayOfWeek[] days)
            {
                this.choice = choice;
                this.days = days;
            }

            public QuietHoursDayChoice Choice
            {
                get
                {
                    return this.choice;
                }
            }

            public DayOfWeek[] Days
            {
                get
                {
                    return this.days;
                }
            }
        }
    }
}
