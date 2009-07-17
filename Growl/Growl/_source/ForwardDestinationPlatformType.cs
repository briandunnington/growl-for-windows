using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace Growl
{
    [Serializable]
    public class ForwardDestinationPlatformType : ISerializable
    {
        public static ForwardDestinationPlatformType Windows = new ForwardDestinationPlatformType(PLATFORM_WINDOWS, global::Growl.Properties.Resources.windows);
        public static ForwardDestinationPlatformType Mac = new ForwardDestinationPlatformType(PLATFORM_MAC, global::Growl.Properties.Resources.mac);
        public static ForwardDestinationPlatformType Linux = new ForwardDestinationPlatformType(PLATFORM_LINUX, global::Growl.Properties.Resources.linux);
        public static ForwardDestinationPlatformType Internet = new ForwardDestinationPlatformType(PLATFORM_INTERNET, global::Growl.Properties.Resources.internet);
        public static ForwardDestinationPlatformType IPhone = new ForwardDestinationPlatformType(PLATFORM_IPHONE, global::Growl.Properties.Resources.iphone);
        public static ForwardDestinationPlatformType Mobile = new ForwardDestinationPlatformType(PLATFORM_MOBILE, global::Growl.Properties.Resources.mobile);
        public static ForwardDestinationPlatformType Email = new ForwardDestinationPlatformType(PLATFORM_EMAIL, global::Growl.Properties.Resources.Envelope);
        public static ForwardDestinationPlatformType Twitter = new ForwardDestinationPlatformType(PLATFORM_TWITTER, global::Growl.Properties.Resources.twitter);
        public static ForwardDestinationPlatformType Other = new ForwardDestinationPlatformType(PLATFORM_OTHER, global::Growl.Properties.Resources.other);

        private const string PLATFORM_WINDOWS = "windows";
        private const string PLATFORM_MAC = "mac";
        private const string PLATFORM_LINUX = "linux";
        private const string PLATFORM_INTERNET = "internet";
        private const string PLATFORM_IPHONE = "iphone";
        private const string PLATFORM_MOBILE = "mobile";
        private const string PLATFORM_EMAIL = "email";
        private const string PLATFORM_TWITTER = "twitter";
        private const string PLATFORM_OTHER = "other";

        private string platform;
        private Image icon;

        private ForwardDestinationPlatformType(string platform, Image icon)
        {
            this.platform = platform;
            this.icon = icon;
        }

        public string Name
        {
            get
            {
                return this.platform;
            }
        }

        public Image Icon
        {
            get
            {
                return this.icon;
            }
        }

        public static ForwardDestinationPlatformType FromString(string platform)
        {
            // the passed in value may be a one-word string (usually used with Bonjour discovery),
            // or it may be an Origin-Platform-Name header value (which is not standardized, but more
            // like a User-Agent type value

            if (!String.IsNullOrEmpty(platform))
            {
                // first, check for known values
                switch (platform)
                {
                    case PLATFORM_WINDOWS:
                        return ForwardDestinationPlatformType.Windows;
                    case PLATFORM_MAC:
                        return ForwardDestinationPlatformType.Mac;
                    case PLATFORM_LINUX:
                        return ForwardDestinationPlatformType.Linux;
                    case PLATFORM_INTERNET:
                        return ForwardDestinationPlatformType.Internet;
                    case PLATFORM_IPHONE:
                        return ForwardDestinationPlatformType.IPhone;
                    case PLATFORM_MOBILE:
                        return ForwardDestinationPlatformType.Mobile;
                    case PLATFORM_EMAIL:
                        return ForwardDestinationPlatformType.Email;
                    case PLATFORM_TWITTER:
                        return ForwardDestinationPlatformType.Twitter;
                    default:
                        // if we didnt get a known value, try parsing the string
                        platform = platform.ToLower();
                        if (platform.IndexOf(PLATFORM_WINDOWS) >= 0) return ForwardDestinationPlatformType.Windows;
                        else if (platform.IndexOf(PLATFORM_MAC) >= 0) return ForwardDestinationPlatformType.Mac;
                        else if (platform.IndexOf(PLATFORM_LINUX) >= 0) return ForwardDestinationPlatformType.Linux;
                        else if (platform.IndexOf(PLATFORM_INTERNET) >= 0) return ForwardDestinationPlatformType.Internet;
                        else if (platform.IndexOf(PLATFORM_IPHONE) >= 0) return ForwardDestinationPlatformType.IPhone;
                        else if (platform.IndexOf(PLATFORM_MOBILE) >= 0) return ForwardDestinationPlatformType.Mobile;
                        break;
                }
            }

            // we get here if all else fails
            return ForwardDestinationPlatformType.Other;
        }

        #region ISerializable Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(ForwardDestinationPlatformTypeSerializationHelper));
            info.AddValue("platform", this.platform, typeof(string));
        }

        #endregion

        [Serializable]
        private class ForwardDestinationPlatformTypeSerializationHelper : IObjectReference
        {
            private string platform = null;

            #region IObjectReference Members

            public object GetRealObject(StreamingContext context)
            {
                return ForwardDestinationPlatformType.FromString(this.platform);
            }

            #endregion
        }
    }
}
