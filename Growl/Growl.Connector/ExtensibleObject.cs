using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents the base class for types that can be represented as a set of headers (including
    /// pre-defined and custom headers)
    /// </summary>
    public class ExtensibleObject
    {
        private static string defaultMachineName = Environment.MachineName;
        private static string defaultSoftwareName = "GrowlConnector";
        private static string defaultSoftwareVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static string defaultPlatformName = Environment.OSVersion.ToString();
        private static string defaultPlatformVersion = Environment.OSVersion.Version.ToString();

        /*
        Origin-Machine-Name: Gazebo
        Origin-Software-Name: GrowlAIRConnector
        Origin-Software-Version: 2.0.3230.22943
        Origin-Platform-Name: Microsoft Windows XP
        Origin-Platform-Version: 5.2.3790.131072
         * */

        //private string requestID;
        private string machineName;
        private string softwareName;
        private string softwareVersion;
        private string platformName;
        private string platformVersion;

        Dictionary<string, string> customTextAttributes;
        Dictionary<string, Resource> customBinaryAttributes;

        /// <summary>
        /// Creates a new instance of the <see cref="ExtensibleObject"/> class.
        /// </summary>
        public ExtensibleObject()
        {
            this.machineName = defaultMachineName;
            this.softwareName = defaultSoftwareName;
            this.softwareVersion = defaultSoftwareVersion;
            this.platformName = defaultPlatformName;
            this.platformVersion = defaultPlatformVersion;

            this.customTextAttributes = new Dictionary<string, string>();
            this.customBinaryAttributes = new Dictionary<string, Resource>();
        }

        /// <summary>
        /// The name of the machine sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: Gazebo
        /// </value>
        public string MachineName
        {
            get
            {
                return this.machineName;
            }
        }

        /// <summary>
        /// The name of the software (framework) sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: GrowlConnector
        /// </value>
        public string SoftwareName
        {
            get
            {
                return this.softwareName;
            }
        }

        /// <summary>
        /// The version of the software (framework) sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: 2.0
        /// </value>
        public string SoftwareVersion
        {
            get
            {
                return this.softwareVersion;
            }
        }

        /// <summary>
        /// The name of the platform (OS) sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: Windows XP
        /// </value>
        public string PlatformName
        {
            get
            {
                return this.platformName;
            }
        }

        /// <summary>
        /// The version of the platform (OS) sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: 5.0.12
        /// </value>
        public string PlatformVersion
        {
            get
            {
                return this.platformVersion;
            }
        }

        /// <summary>
        /// Gets a collection of custom text attributes associated with this object
        /// </summary>
        /// <remarks>
        /// Each custom text attribute is equivalent to a custom "X-" header
        /// </remarks>
        /// <value>
        /// <see cref="Dictionary{TKey, TVal}"/>
        /// </value>
        public Dictionary<string, string> CustomTextAttributes
        {
            get
            {
                return this.customTextAttributes;
            }
        }

        /// <summary>
        /// Gets a collection of custom binary attributes associated with this object
        /// </summary>
        /// <remarks>
        /// Each custom binary attribute is equivalent to a custom "X-" header with a 
        /// "x-growl-resource://" value, as well as the necessary resource headers
        /// (Identifier, Length, and binary bytes)
        /// </remarks>
        /// <value>
        /// <see cref="Dictionary{TKey, TVal}"/>
        /// </value>
        public Dictionary<string, Resource> CustomBinaryAttributes
        {
            get
            {
                return this.customBinaryAttributes;
            }
        }

        /// <summary>
        /// Adds any inherited headers to the end of the header collection
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> to append the headers to</param>
        /// <remarks>
        /// This method should only be called from a derived class' .ToHeaders() method.
        /// It takes care of adding the Origin-* headers as well as any X-* custom headers.
        /// 
        /// This method is the same as calling both AddCommonAttributesToHeaders and 
        /// AddCustomAttributesToHeaders.
        /// </remarks>
        protected void AddInheritedAttributesToHeaders(HeaderCollection headers)
        {
            AddCommonAttributesToHeaders(headers);
            AddCustomAttributesToHeaders(headers);
        }

        /// <summary>
        /// Sets the object's base class properties from the supplied header list
        /// </summary>
        /// <param name="obj">The <see cref="ExtensibleObject"/> being rehydrated</param>
        /// <param name="headers">The <see cref="HeaderCollection"/> containing the parsed header values</param>
        /// <remarks>
        /// This method should only be called from a derived class' .FromHeaders() method.
        /// It takes care of setting the Origin-* related properties, as well as any custom attributes.
        /// 
        /// This method is the same as calling both SetCommonAttributesFromHeaders and
        /// SetCustomAttributesFromHeaders.
        /// </remarks>
        protected static void SetInhertiedAttributesFromHeaders(ExtensibleObject obj, HeaderCollection headers)
        {
            SetCommonAttributesFromHeaders(obj, headers);
            SetCustomAttributesFromHeaders(obj, headers);
        }

        /// <summary>
        /// When converting an <see cref="ExtensibleObject"/> to a list of headers,
        /// this method adds the common attributes to the list of headers.
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> to add the custom headers to</param>
        protected void AddCommonAttributesToHeaders(HeaderCollection headers)
        {
            if (headers != null)
            {
                //Header hRequestID = new Header("RequestID", requestID);
                Header hMachineName = new Header(Header.ORIGIN_MACHINE_NAME, machineName);
                Header hSoftwareName = new Header(Header.ORIGIN_SOFTWARE_NAME, softwareName);
                Header hSoftwareVersion = new Header(Header.ORIGIN_SOFTWARE_VERSION, softwareVersion);
                Header hPlatformName = new Header(Header.ORIGIN_PLATFORM_NAME, platformName);
                Header hPlatformVersion = new Header(Header.ORIGIN_PLATFORM_VERSION, platformVersion);

                //headers.Add(hRequestID);
                headers.AddHeader(hMachineName);
                headers.AddHeader(hSoftwareName);
                headers.AddHeader(hSoftwareVersion);
                headers.AddHeader(hPlatformName);
                headers.AddHeader(hPlatformVersion);
            }
        }

        /// <summary>
        /// When converting a list of headers to an <see cref="ExtensibleObject"/>, this
        /// method sets the common attributes on the object.
        /// </summary>
        /// <param name="obj">The <see cref="ExtensibleObject"/> to be populated</param>
        /// <param name="headers">The <see cref="HeaderCollection"/> containing the list of headers</param>
        protected static void SetCommonAttributesFromHeaders(ExtensibleObject obj, HeaderCollection headers)
        {
            if (obj != null && headers != null)
            {
                Header hMachineName = headers.Get(Header.ORIGIN_MACHINE_NAME);
                if (hMachineName != null && !String.IsNullOrEmpty(hMachineName.Value)) obj.machineName = hMachineName.Value;

                Header hSoftwareName = headers.Get(Header.ORIGIN_SOFTWARE_NAME);
                if (hSoftwareName != null && !String.IsNullOrEmpty(hSoftwareName.Value)) obj.softwareName = hSoftwareName.Value;

                Header hSoftwareVersion = headers.Get(Header.ORIGIN_SOFTWARE_VERSION);
                if (hSoftwareVersion != null && !String.IsNullOrEmpty(hSoftwareVersion.Value)) obj.softwareVersion = hSoftwareVersion.Value;

                Header hPlatformName = headers.Get(Header.ORIGIN_PLATFORM_NAME);
                if (hPlatformName != null && !String.IsNullOrEmpty(hPlatformName.Value)) obj.platformName = hPlatformName.Value;

                Header hPlatoformVersion = headers.Get(Header.ORIGIN_PLATFORM_VERSION);
                if (hPlatoformVersion != null && !String.IsNullOrEmpty(hPlatoformVersion.Value)) obj.platformVersion = hPlatoformVersion.Value;
            }
        }

        /// <summary>
        /// When converting an <see cref="ExtensibleObject"/> to a list of headers,
        /// this method adds the custom attributes (both text and binary) to the
        /// list of headers.
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> to add the custom attributes to</param>
        protected void AddCustomAttributesToHeaders(HeaderCollection headers)
        {
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> item in this.CustomTextAttributes)
                {
                    Header customHeader = new CustomHeader(item.Key, item.Value);
                    headers.AddHeader(customHeader);
                }
                foreach (KeyValuePair<string, Resource> item in this.CustomBinaryAttributes)
                {
                    Header customHeader = new CustomHeader(item.Key, item.Value.ToString());
                    headers.AddHeader(customHeader);
                    headers.AssociateBinaryData(item.Value);
                }
            }
        }

        /// <summary>
        /// When converting a list of headers to an <see cref="ExtensibleObject"/>, this
        /// method sets the custom attributes (both text and binary) on the object.
        /// </summary>
        /// <param name="obj">The <see cref="ExtensibleObject"/> to be populated</param>
        /// <param name="headers">The <see cref="HeaderCollection"/> containing the list of headers</param>
        protected static void SetCustomAttributesFromHeaders(ExtensibleObject obj, HeaderCollection headers)
        {
            if (obj != null && headers != null)
            {
                foreach (Header header in headers.CustomHeaders)
                {
                    if (header != null)
                    {
                        if (header.IsGrowlResourcePointer)
                        {
                            obj.CustomBinaryAttributes.Add(header.ActualName, header.GrowlResource);
                        }
                        else
                        {
                            obj.CustomTextAttributes.Add(header.ActualName, header.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the software information (name/version) for the current application
        /// </summary>
        /// <param name="name">The name of the software</param>
        /// <param name="version">The version of the software</param>
        /// <remarks>
        /// This method is typically called by a server implementation that wants to identify itself
        /// properly in the 'Origin-Software-*' headers.
        /// </remarks>
        public static void SetSoftwareInformation(string name, string version)
        {
            defaultSoftwareName = name;
            defaultSoftwareVersion = version;
        }

        /// <summary>
        /// Sets the platform information (name/version) for the current application
        /// </summary>
        /// <param name="name">The name of the platform</param>
        /// <param name="version">The version of the platform</param>
        /// <remarks>
        /// This method is typically called by a server implementation that wants to identify itself
        /// properly in the 'Origin-Platform-*' headers.
        /// Normally it is not necessary to call this method as the platform information is automatically
        /// detected.
        /// </remarks>
        public static void SetPlatformInformation(string name, string version)
        {
            defaultPlatformName = name;
            defaultPlatformVersion = version;
        }
    }
}
