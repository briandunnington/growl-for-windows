using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace GrowlExtras.Subscriptions.FeedMonitor
{
    /// <summary>
    /// Provides methods for converting <see cref="DateTime"/> structures 
    /// to and from the equivalent <a href="http://www.w3.org/Protocols/rfc822/#z28">RFC 822</a> 
    /// string representation.
    /// </summary>
    public class Rfc822DateTime
    {
        /// <summary>
        /// Private member to hold array of formats that RFC 822 date-time representations conform to.
        /// </summary>
        private static string[] formats = new string[0];

        /// <summary>
        /// Private member to hold the DateTime format string for representing a DateTime in the RFC 822 format.
        /// </summary>
        private const string format = "ddd, dd MMM yyyy HH:mm:ss K";

        private static Dictionary<string, string> timezoneOffsets = null;

        static Rfc822DateTime()
        {
            string folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string file = System.IO.Path.Combine(folder, "formats.xml");

            XmlDocument xml = new XmlDocument();
            xml.Load(file);

            List<string> formatList = new List<string>();
            XmlNodeList formatNodes = xml.SelectNodes("/datetimeparser/formats/format");
            foreach (XmlNode formatNode in formatNodes)
            {
                formatList.Add(formatNode.InnerText);
            }
            // Fall back patterns
            formatList.Add(DateTimeFormatInfo.InvariantInfo.UniversalSortableDateTimePattern);
            formatList.Add(DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern);
            formats = formatList.ToArray();

            timezoneOffsets = new Dictionary<string, string>();
            XmlNodeList timezoneNodes = xml.SelectNodes("/datetimeparser/timezones/timezone");
            foreach (XmlNode timezoneNode in timezoneNodes)
            {
                string abbr = timezoneNode.Attributes["abbr"].Value;
                string val = timezoneNode.InnerText;
                timezoneOffsets.Add(abbr, val);
            }
        }


        /// <summary>
        /// Gets the custom format specifier that may be used to represent a <see cref="DateTime"/> in the RFC 822 format.
        /// </summary>
        /// <value>A <i>DateTime format string</i> that may be used to represent a <see cref="DateTime"/> in the RFC 822 format.</value>
        /// <remarks>
        /// <para>
        /// This method returns a string representation of a <see cref="DateTime"/> that utilizes the time zone 
        /// offset (local differential) to represent the offset from Greenwich mean time in hours and minutes. 
        /// The <see cref="Rfc822DateTimeFormat"/> is a valid date-time format string for use 
        /// in the <see cref="DateTime.ToString(String, IFormatProvider)"/> method.
        /// </para>
        /// <para>
        /// The <a href="http://www.w3.org/Protocols/rfc822/#z28">RFC 822</a> Date and Time specification 
        /// specifies that the year will be represented as a two-digit value, but the 
        /// <a href="http://www.rssboard.org/rss-profile#data-types-datetime">RSS Profile</a> recommends that 
        /// all date-time values should use a four-digit year. The <see cref="Rfc822DateTime"/> class 
        /// follows the RSS Profile recommendation when converting a <see cref="DateTime"/> to the equivalent 
        /// RFC 822 string representation.
        /// </para>
        /// </remarks>
        public static string Rfc822DateTimeFormat
        {
            get
            {
                return format;
            }
        }

        /// <summary>
        /// Gets an array of the expected formats for RFC 822 date-time string representations.
        /// </summary>
        /// <value>
        /// An array of the expected formats for RFC 822 date-time string representations 
        /// that may used in the <see cref="DateTime.TryParseExact(String, string[], IFormatProvider, DateTimeStyles, out DateTime)"/> method.
        /// </value>
        /// <remarks>
        /// The array of the expected formats that is returned assumes that the RFC 822 time zone 
        /// is represented as or converted to a local differential representation.
        /// </remarks>
        /// <seealso cref="ConvertZoneToLocalDifferential(String)"/>
        public static string[] Rfc822DateTimePatterns
        {
            get
            {
                return formats;
            }
        }

        /// <summary>
        /// Converts the specified string representation of a date and time to its <see cref="DateTime"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a date and time to convert.</param>
        /// <returns>
        /// A <see cref="DateTime"/> equivalent to the date and time contained in <paramref name="s"/>, 
        /// expressed as <i>Coordinated Universal Time (UTC)</i>.
        /// </returns>
        /// <remarks>
        /// The string <paramref name="s"/> is parsed using formatting information in the <see cref="DateTimeFormatInfo.InvariantInfo"/> object.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is an empty string.</exception>
        /// <exception cref="FormatException"><paramref name="s"/> does not contain a valid RFC 822 string representation of a date and time.</exception>
        public static DateTime Parse(string s)
        {
            DateTime result;
            if (Rfc822DateTime.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                throw new FormatException(String.Format(null, "{0} is not a valid RFC 822 string representation of a date and time.", s));
            }
        }

        /// <summary>
        /// Converts the time zone component of an RFC 822 date and time string representation to its local differential (time zone offset).
        /// </summary>
        /// <param name="s">A string containing an RFC 822 date and time to convert.</param>
        /// <returns>A date and time string that uses local differential to describe the time zone equivalent to the date and time contained in <paramref name="s"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is an empty string.</exception>
        public static string ConvertZoneToLocalDifferential(string s)
        {
            string zoneRepresentedAsLocalDifferential = s;

            foreach (KeyValuePair<string, string> item in timezoneOffsets)
            {
                bool found = false;

                List<string> endings = new List<string>();
                endings.Add(String.Format(" {0}", item.Key));
                endings.Add(item.Key);

                foreach (string ending in endings)
                {
                    if (s.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
                    {
                        zoneRepresentedAsLocalDifferential = String.Concat(s.Substring(0, (s.LastIndexOf(ending))), " ", item.Value);
                        found = true;
                        break;
                    }
                }

                if (found) break;
            }

            return zoneRepresentedAsLocalDifferential;
        }

        /// <summary>
        /// Converts the value of the specified <see cref="DateTime"/> object to its equivalent string representation.
        /// </summary>
        /// <param name="utcDateTime">The Coordinated Universal Time (UTC) <see cref="DateTime"/> to convert.</param>
        /// <returns>A RFC 822 string representation of the value of the <paramref name="utcDateTime"/>.</returns>
        /// <exception cref="ArgumentException">The specified <paramref name="utcDateTime"/> object does not represent a <see cref="DateTimeKind.Utc">Coordinated Universal Time (UTC)</see> value.</exception>
        public static string ToString(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("utcDateTime");
            }

            return utcDateTime.ToString(Rfc822DateTime.Rfc822DateTimeFormat, DateTimeFormatInfo.InvariantInfo);
        }

        public static bool TryParse(string s, out DateTime result)
        {
            return TryParse(s, null, out result);
        }

        /// <summary>
        /// Converts the specified string representation of a date and time to its <see cref="DateTime"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a date and time to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="DateTime"/> value equivalent to the date and time 
        /// contained in <paramref name="s"/>, expressed as <i>Coordinated Universal Time (UTC)</i>, 
        /// if the conversion succeeded, or <see cref="DateTime.MinValue">MinValue</see> if the conversion failed. 
        /// The conversion fails if the s parameter is a <b>null</b> reference (Nothing in Visual Basic), 
        /// or does not contain a valid string representation of a date and time. 
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><b>true</b> if the <paramref name="s"/> parameter was converted successfully; otherwise, <b>false</b>.</returns>
        /// <remarks>
        /// The string <paramref name="s"/> is parsed using formatting information in the <see cref="DateTimeFormatInfo.InvariantInfo"/> object. 
        /// </remarks>
        public static bool TryParse(string s, string cultureCode, out DateTime result)
        {
            //------------------------------------------------------------
            //  Attempt to convert string representation
            //------------------------------------------------------------
            bool wasConverted = false;
            result = DateTime.MinValue;

            if (!String.IsNullOrEmpty(s))
            {
                List<DateTimeFormatInfo> dtfis = new List<DateTimeFormatInfo>();
                dtfis.Add(DateTimeFormatInfo.InvariantInfo);

                try
                {
                    if (!String.IsNullOrEmpty(cultureCode))
                    {
                        CultureInfo ci = CultureInfo.CreateSpecificCulture(cultureCode);
                        dtfis.Add(ci.DateTimeFormat);
                    }
                }
                catch { }

                DateTime parseResult;
                foreach (DateTimeFormatInfo dtfi in dtfis)
                {
                    if (DateTime.TryParseExact(Rfc822DateTime.ConvertZoneToLocalDifferential(s), Rfc822DateTime.Rfc822DateTimePatterns, dtfi, DateTimeStyles.AdjustToUniversal, out parseResult))
                    {
                        result = DateTime.SpecifyKind(parseResult, DateTimeKind.Utc);
                        wasConverted = true;
                        break;
                    }
                }
            }

            return wasConverted;
        }
    }
}