using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace Vortex.Growl.AppBridge
{
    /// <summary>
    /// Provides utilities for writing to a file-based log
    /// </summary>
    public class FileLogger
    {
        #region member variables

        /// <summary>
        /// Path to the log file
        /// </summary>
        string filePath = "";

        /// <summary>
        /// Format of the log file
        /// </summary>
        FileLogFormat format = FileLogFormat.Line;

        /// <summary>
        /// Indicates if the log file should be appended to or overwritten
        /// </summary>
        bool overwrite = false;

        /// <summary>
        /// Flag to keep track if the log file has been overwritten already or not
        /// </summary>
        bool hasOverwrittenFile = false;

        #endregion member variables

        #region constructors & destructors

        /// <summary>
        /// Standard constructor
        /// </summary>
        public FileLogger()
        {
            // there is no parameter checking because even if the parameter is bad,
            // we dont want the logging mechanism to throw exceptions and break stuff
            this.filePath = Utility.UserSettingFolder + "growl.log";
        }

        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="filePath">Full path to the log file</param>
        public FileLogger(string filePath)
        {
            // there is no parameter checking because even if the parameter is bad,
            // we dont want the logging mechanism to throw exceptions and break stuff
            this.filePath = filePath;
        }

        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="filePath">Full path to the log file</param>
        /// <param name="format">Log file format</param>
        public FileLogger(string filePath, FileLogFormat format)
        {
            // there is no parameter checking because even if the parameter is bad,
            // we dont want the logging mechanism to throw exceptions and break stuff
            this.filePath = filePath;
            this.format = format;
        }

        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="filePath">Full path to the log file</param>
        /// <param name="format">Log file format</param>
        /// <param name="overwrite">True if you want to overwrite the log file; False if you want to append to the file</param>
        public FileLogger(string filePath, FileLogFormat format, bool overwrite)
        {
            // there is no parameter checking because even if the parameter is bad,
            // we dont want the logging mechanism to throw exceptions and break stuff
            this.filePath = filePath;
            this.format = format;
            this.overwrite = overwrite;
        }

        #endregion constructors & destructors

        #region ILog Members

        /// <summary>
        /// Writes the text to the log source
        /// </summary>
        /// <param name="message">The text to be logged</param>
        public void Log(string message)
        {
            FileStream fileStream = null;
            StreamWriter writer = null;

            try
            {
                // there is no parameter checking because even if the parameter is bad,
                // we dont want the logging mechanism to throw exceptions and break stuff

                // Determine if we want to append to the file or overwrite it
                FileMode mode;
                if (this.overwrite && !this.hasOverwrittenFile)
                    mode = FileMode.Create;
                else
                    mode = FileMode.Append;

                // Open the file and prepare the StreamWriter
                fileStream = File.Open(this.filePath, mode, FileAccess.Write, FileShare.None);
                this.hasOverwrittenFile = true;
                writer = new StreamWriter(fileStream);

                // Write the text
                writer.Write(FormatEntry(message));
            }
            catch
            {
                // just swallow the error so nothing breaks
                // (we dont want the logging to be the cause of exceptions)
            }
            finally
            {
                // Finish up by making sure all resources are released
                if (writer != null) writer.Close();
                if (fileStream != null) fileStream.Close();
            }
        }

        /// <summary>
        /// Writes the text to the log source
        /// </summary>
        /// <param name="messages">string[] - The text to be logged</param>
        public void Log(string[] messages)
        {
            try
            {
                // there is no parameter checking because even if the parameter is bad,
                // we dont want the logging mechanism to throw exceptions and break stuff

                // Determine the delimiter based on the format
                string delimiter = "\t";
                if (this.format == FileLogFormat.Block)
                    delimiter = "\r\n";

                // Convert the string array into a single string
                string message = string.Join(delimiter, messages);

                // Log the message
                Log(message);
            }
            catch
            {
                // just swallow the error so nothing breaks
                // (we dont want the logging to be the cause of exceptions)
            }
        }

        #endregion ILog Members

        #region Private methods

        /// <summary>
        /// Formats the text to pre-defined styles
        /// </summary>
        /// <param name="message">The text to be logged</param>
        private string FormatEntry(string message)
        {
            try
            {
                // there is no parameter checking because even if the parameter is bad,
                // we dont want the logging mechanism to throw exceptions and break stuff

                // Determine what format we should use
                string format = FetchTemplate(this.format);

                // Build the list of params
                object[] args = new object[] { DateTime.Now.ToString(), Environment.MachineName, message };

                // Construct the string
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(format, args);

                // Return the string
                return sb.ToString();
            }
            catch
            {
                // if something goes awry, just return the text that was 
                // passed to us, just to be safe
                return message;
            }
        }

        /// <summary>
        /// The FetchTemplate method retrieves the LogFormatTemplateAttribute from the metadata
        /// of the enumField passed in.
        /// </summary>
        /// <param name="enumField">A specific field of an enumeration (MyEnum.Field)</param>
        /// <returns cref="string">
        /// String containing LogFormatTemplateAttribute.
        /// </returns>
        public static string FetchTemplate(FileLogFormat enumField)
        {
            string template = "";

            try
            {
                // determine what type of object we are dealing with
                Type myType = enumField.GetType().UnderlyingSystemType;

                // get the specific field we are interested in
                FieldInfo field = myType.GetField(enumField.ToString());

                // load the EnumStringAttributes for the object (there should be 1 and only 1)
                object[] attributes;
                attributes = field.GetCustomAttributes(typeof(LogFormatTemplateAttribute), false);
                LogFormatTemplateAttribute attribute = (LogFormatTemplateAttribute)attributes[0];

                // get the DisplayName property from the attribute
                template = attribute.Template;
            }
            catch
            {
                // if we couldn't get the EnumStringAttribute (or it wasn't set),
                // then return a message as the template
                template = "TEMPLATE NOT SET";
            }
            return template;
        }

        #endregion Private methods
    }

    # region FileLogFormat enum

    /// <summary>
    /// The formatting style of log entries
    /// </summary>
    public enum FileLogFormat
    {
        /// <summary>
        /// Formats log entry to a single line
        /// </summary>
        [LogFormatTemplate("{0}\t{1}\t{2}\r\n")]
        Line,

        /// <summary>
        /// Formats log entry as a block of text
        /// </summary>
        [LogFormatTemplate("-----BEGIN ENTRY----------------------\r\nLOG TIME: {0}\r\nSERVER:   {1}\r\n\r\n{2}\r\n-----END ENTRY------------------------\r\n\r\n")]
        Block,

        /// <summary>
        /// Formats the log entry as well-formed xml
        /// </summary>
        [LogFormatTemplate("<!-- {0}\t{1} -->\r\n{2}\r\n")]
        Xml
    }

    # endregion FileLogFormat enum

    # region LogFormatTemplateAttribute

    /// <summary>
    /// Provides the log format template used when logging entries to a file log
    /// </summary>
    /// <remarks>
    /// The LogFormatTemplateAttribute is only allowed on Fields, and only one LogFormatTemplateAttribute is allowed per Field.<br/><br/>
    /// The three placeholder values are:
    /// <list type="table">
    ///	<listheader>
    ///	<term>Placeholder</term>
    ///	<description>Data</description>
    ///	</listheader>
    ///	<item>
    ///	<term>{0}</term>
    ///	<description>Date and time</description>
    ///	</item>
    ///	<item>
    ///	<term>{1}</term>
    ///	<description>Machine name</description>
    ///	</item>
    ///	<item>
    ///	<term>{2}</term>
    ///	<description>Log data/message</description>
    ///	</item>
    /// </list>
    /// <code>
    /// Usage:
    ///
    ///	enum MyEnum : int
    ///	{
    ///		[LogFormatTemplateAttribute("{0} - {1} - {2}")]
    ///		SingleLine = 1,
    ///		[LogFormatTemplateAttribute("{0}\r\n{1}\r\n{2}\r\n")]
    ///		MultiLine = 2,
    ///		All = 3
    ///	}
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class LogFormatTemplateAttribute : System.Attribute
    {
        # region private & protected member variables

        /// <summary>
        /// Friendly name of the enum value
        /// </summary>
        private readonly string template;

        # endregion private & protected member variables

        # region constructors

        /// <summary>
        /// Creates a new instance of the LogFormatTemplateAttribute class
        /// </summary>
        /// <param name="template">The formatting string</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="template" /> is null</exception>
        public LogFormatTemplateAttribute(string template)
        {
            // parameter checking
            if (template == null)
                throw new ArgumentNullException("template", "LogFormatTemplateAttribute: 'template' parameter cannot be null.");
            // NOTE: It is OK for template to be an empty string

            // set the the template
            this.template = template;
        }

        # endregion constructors

        # region Public methods

        /// <summary>
        /// Provides a public method to access the template format string
        /// </summary>
        /// <value>
        /// The formatting string
        /// </value>
        /// <remarks>
        /// This property is Read-Only.
        /// </remarks>
        public string Template
        {
            get
            {
                return this.template;
            }
        }

        # endregion Public methods
    }

    # endregion LogFormatTemplateAttribute
}
