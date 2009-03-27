using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents an application that may send notifications
    /// </summary>
    public class Application : ExtensibleObject
    {
        /// <summary>
        /// The application name
        /// </summary>
        private string name;

        /// <summary>
        /// The application's icon
        /// </summary>
        private Resource icon;


        /// <summary>
        /// Creates a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="name">The name of the application</param>
        public Application(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// The name of the application
        /// </summary>
        /// <value>
        /// string - Ex: SurfWriter
        /// </value>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        /// <summary>
        /// The application's icon
        /// </summary>
        /// <value>
        /// <see cref="Resource"/>
        /// </value>
        public Resource Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = value;
            }
        }

        /// <summary>
        /// Converts the object to a list of headers
        /// </summary>
        /// <returns><see cref="HeaderCollection"/></returns>
        public HeaderCollection ToHeaders()
        {
            Header hName = new Header(Header.APPLICATION_NAME, this.name);
            Header hIcon = new Header(Header.APPLICATION_ICON, this.Icon);

            HeaderCollection headers = new HeaderCollection();
            headers.AddHeader(hName);

            if (this.Icon != null && this.Icon.IsSet)
            {
                headers.AddHeader(hIcon);
                headers.AssociateBinaryData(this.Icon);
            }

            this.AddInheritedAttributesToHeaders(headers);
            return headers;
        }

        /// <summary>
        /// Creates a new <see cref="Application"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="Application"/></returns>
        public static Application FromHeaders(HeaderCollection headers)
        {
            string name = headers.GetHeaderStringValue(Header.APPLICATION_NAME, true);
            Resource icon = headers.GetHeaderResourceValue(Header.APPLICATION_ICON, false);

            Application app = new Application(name);
            app.Icon = icon;

            SetInhertiedAttributesFromHeaders(app, headers);

            return app;
        }
    }
}
