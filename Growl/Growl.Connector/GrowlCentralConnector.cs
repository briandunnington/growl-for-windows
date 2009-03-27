using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Connector
{
    /*
    public class GrowlCentralConnector
    {
        GrowlConnector growl = null;
        string developerID;

        public GrowlCentralConnector(string developerID, string developerKey, string hostname, int port)
        {
            this.growl = new GrowlConnector(developerKey, hostname, port);
            this.developerID = developerID;

            // TODO: handle response events
        }

        public Cryptography.HashAlgorithmType KeyHashAlgorithm
        {
            get
            {
                return this.growl.KeyHashAlgorithm;
            }
            set
            {
                this.growl.KeyHashAlgorithm = value;
            }
        }

        public Cryptography.SymmetricAlgorithmType EncryptionAlgorithm
        {
            get
            {
                return this.growl.EncryptionAlgorithm;
            }
            set
            {
                if (value == Cryptography.SymmetricAlgorithmType.PlainText)
                    throw new System.Security.Cryptography.CryptographicException("PlainText mode is not supported");

                this.growl.EncryptionAlgorithm = value;
            }
        }

        public string Register(string userID, Application application, NotificationType[] notificationTypes)
        {
            AddIdentificationInformation(application, userID);
            return growl.Register(application, notificationTypes);
        }

        public string Notify(string userID, Application application, Notification notification)
        {
            AddIdentificationInformation(notification, userID);
            return growl.Notify(application, notification);
        }

        public string Notify(string userID, Application application, Notification notification, CallbackContext callbackContext)
        {
            AddIdentificationInformation(notification, userID);
            return growl.Notify(application, notification, callbackContext);
        }

        public string Status(string userID, Application application)
        {
            AddIdentificationInformation(application, userID);
            return growl.Status(application);
        }

        protected void AddIdentificationInformation(ExtensibleObject exObj, string userID)
        {
            exObj.CustomTextAttributes.Add("GrowlCentral-Developer-ID", this.developerID);
            exObj.CustomTextAttributes.Add("GrowlCentral-Intended-Recipient", Cryptography.ComputeHash(userID));
        }
    }
     * */
}
