using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Provides the ability to dynamically construct a GNTP message
    /// </summary>
    public class MessageBuilder : MessageSection
    {
        /// <summary>
        /// The protocol name
        /// </summary>
        private const string PROTOCOL_NAME = "GNTP";

        /// <summary>
        /// The protocol version supported by the message builder
        /// </summary>
        private const string PROTOCOL_VERSION = "1.0";

        /// <summary>
        /// Array of bytes containing the protocol header information
        /// </summary>
        private static byte[] protocolHeaderBytes = GetStringBytes(String.Format("{0}/{1} ", PROTOCOL_NAME, PROTOCOL_VERSION));

        /// <summary>
        /// The type of message
        /// </summary>
        private string messageType;

        /// <summary>
        /// The secret key
        /// </summary>
        private Key key;

        /// <summary>
        /// Indicates if the key hash should be included in the message
        /// </summary>
        private bool includeKeyHash = false;

        /// <summary>
        /// Collection of additional message sections that are part of this message
        /// </summary>
        private List<MessageSection> sections = new List<MessageSection>();

        /// <summary>
        /// Creates a new instance of the <see cref="MessageBuilder"/> class
        /// used to build a request message.
        /// </summary>
        /// <param name="messageType">The <see cref="RequestType"/> of the message</param>
        /// <param name="key">The <see cref="Key"/> used to authorize and encrypt the message</param>
        public MessageBuilder(RequestType messageType, Key key)
            : this(messageType.ToString(), key, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MessageBuilder"/> class
        /// used to build a response message.
        /// </summary>
        /// <param name="messageType">The <see cref="ResponseType"/> of the message</param>
        public MessageBuilder(ResponseType messageType) : this("-" + messageType.ToString(), null, false)
        {
        }

        /*
        // TODO: do we ever want to encrypt responses?
        public MessageBuilder(ResponseType messageType, Key key, Cryptography.HashAlgorithmType keyHashAlgorithm, Cryptography.SymmetricAlgorithmType encryptionAlgorithm)
            : this("-" + messageType.ToString(), key, false, Cryptography.HashAlgorithmType.MD5, Cryptography.SymmetricAlgorithmType.PlainText)
        {
        }
         * */

        /// <summary>
        /// Creates a new instance of the <see cref="MessageBuilder"/> class
        /// </summary>
        /// <param name="messageType">The type of message (directive)</param>
        /// <param name="key">The <see cref="Key"/> used to authorize and encrypt the message</param>
        /// <param name="includeKeyHash">Indicates if the key hash should be included in the message or not</param>
        protected MessageBuilder(string messageType, Key key, bool includeKeyHash) : base()
        {
            this.messageType = messageType;
            this.key = (key != null ? key : Key.None);
            this.includeKeyHash = (includeKeyHash && (key != null && key != Key.None));
        }

        /// <summary>
        /// Adds a <see cref="MessageSection"/> to the message
        /// </summary>
        /// <param name="section"><see cref="MessageSection"/></param>
        public void AddMessageSection(MessageSection section)
        {
            this.sections.Add(section);
        }

        /// <summary>
        /// Converts the contents of the message into an array of bytes
        /// </summary>
        /// <returns>Array of bytes</returns>
        public override byte[] GetBytes()
        {
            List<byte> allBytes = new List<byte>();
            List<byte> messageBytes = new List<byte>();
            List<BinaryData> allBinaryData = new List<BinaryData>();

            allBinaryData.AddRange(this.binaryData);

            // additional sections
            messageBytes.AddRange(this.bytes);
            foreach (MessageSection section in this.sections)
            {
                messageBytes.AddRange(blankLineBytes);
                messageBytes.AddRange(section.GetBytes());
                allBinaryData.AddRange(section.BinaryData);
            }

            // encrypt message
            byte[] bytesToEncrypt = messageBytes.ToArray();
            EncryptionResult result = key.Encrypt(bytesToEncrypt);
            //EncryptionResult result = Cryptography.Encrypt(encryptionKey, bytesToEncrypt, this.encryptionAlgorithm);

            string encryptionInfo = DisplayName.Fetch(key.EncryptionAlgorithm);
            if (key.EncryptionAlgorithm != Cryptography.SymmetricAlgorithmType.PlainText)
            {
                string iv = Cryptography.HexEncode(result.IV);
                encryptionInfo = String.Format("{0}:{1}", encryptionInfo, iv);
            }

            string hashInfo = "";
            if (includeKeyHash)
            {
                //string keyHash = Cryptography.HexEncode(Cryptography.ComputeHash(key, keyHashAlgorithm));
                //string keyHash = key.GetKeyHash(this.keyHashAlgorithm);
                string keyHash = key.KeyHash;
                string salt = key.Salt;
                hashInfo = String.Format("{0}:{1}.{2}", DisplayName.Fetch(key.HashAlgorithm), keyHash, salt);
            }

            // start building message
            string s = String.Format("{0} {1} {2}", messageType, encryptionInfo, hashInfo);
            allBytes.AddRange(protocolHeaderBytes);
            allBytes.AddRange(GetStringBytes(s.ToUpper()));
            allBytes.AddRange(blankLineBytes);
            allBytes.AddRange(result.EncryptedBytes);
            if (key.EncryptionAlgorithm != Cryptography.SymmetricAlgorithmType.PlainText) allBytes.AddRange(blankLineBytes);

            // handle binary resources
            foreach (BinaryData data in allBinaryData)
            {
                if (data != null && data.Data != null)
                {
                    // encrypt each resource, making sure to use the same IV
                    //EncryptionResult er = Cryptography.Encrypt(encryptionKey, data.Data, this.encryptionAlgorithm, ref result.IV);
                    EncryptionResult er = key.Encrypt(data.Data, ref result.IV);

                    MessageSection section = new MessageSection();
                    section.AddBlankLine();
                    section.AddHeader(new Header(Header.RESOURCE_IDENTIFIER, data.ID));
                    section.AddHeader(new Header(Header.RESOURCE_LENGTH, er.EncryptedBytes.Length.ToString()));
                    section.AddBlankLine();
                    allBytes.AddRange(section.GetBytes());
                    allBytes.AddRange(er.EncryptedBytes);
                    allBytes.AddRange(blankLineBytes);
                }
            }
            allBytes.AddRange(blankLineBytes);

            return allBytes.ToArray();
        }
    }

    /// <summary>
    /// Represents a section of GNTP message
    /// </summary>
    public class MessageSection
    {
        /// <summary>
        /// Format string for header lines
        /// </summary>
        protected const string HEADER_FORMAT = "{0}: {1}\r\n";

        /// <summary>
        /// Byte array containing the bytes the represent a blank line
        /// </summary>
        protected static readonly byte[] blankLineBytes = GetStringBytes(Environment.NewLine);

        /// <summary>
        /// The bytes of the section
        /// </summary>
        protected List<byte> bytes;

        /// <summary>
        /// List of any binary data refernced by this section
        /// </summary>
        protected List<BinaryData> binaryData;


        /// <summary>
        /// Creates a new instance of the <see cref="MessageSection"/> class.
        /// </summary>
        public MessageSection()
        {
            this.bytes = new List<byte>();
            this.binaryData = new List<BinaryData>();

            //this.AddBlankLine();
        }

        /// <summary>
        /// Contains a list of all binary data referenced in this section
        /// </summary>
        /// <value>
        /// <see cref="List{BinaryData}"/>
        /// </value>
        internal List<BinaryData> BinaryData
        {
            get
            {
                return this.binaryData;
            }
        }

        /// <summary>
        /// Adds a <see cref="Header"/> to the section.
        /// </summary>
        /// <remarks>
        /// Headers are added to the message output in the same order that they are added via this method.
        /// If the header is a pointer to a binary resource, the binary data is also handled.
        /// </remarks>
        /// <param name="header">The <see cref="Header"/> to add</param>
        public void AddHeader(Header header)
        {
            if (header != null)
            {
                if (header.IsGrowlResourcePointer)
                    this.binaryData.Add(header.GrowlResource);

                if (header.IsBlankLine) AddBlankLine();
                else AddHeader(header.Name, header.Value);
            }
        }

        /// <summary>
        /// Adds a header line to the section
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <param name="val">The value of the header</param>
        protected void AddHeader(string name, string val)
        {
            string s = String.Format(HEADER_FORMAT, name, val);
            this.bytes.AddRange(GetStringBytes(s));
        }

        /// <summary>
        /// Adds a blank line to the section
        /// </summary>
        public void AddBlankLine()
        {
            this.bytes.AddRange(blankLineBytes);
        }

        /// <summary>
        /// Converts a string into an array of bytes
        /// </summary>
        /// <remarks>
        /// The conversion uses UTF8 encoding.
        /// </remarks>
        /// <param name="val">The string to convert</param>
        /// <returns>Array of bytes</returns>
        protected static byte[] GetStringBytes(string val)
        {
            return Encoding.UTF8.GetBytes(val);
        }

        /// <summary>
        /// Converts the contents of the section into an array of bytes
        /// </summary>
        /// <returns>Array of bytes</returns>
        public virtual byte[] GetBytes()
        {
            return bytes.ToArray();
        }

        /// <summary>
        /// Outputs the contents of the section as a string
        /// </summary>
        /// <remarks>
        /// The conversion uses UTF8 encoding.
        /// </remarks>
        /// <returns>string</returns>
        public override string ToString()
        {
            byte[] bytes = GetBytes();
            string val = Encoding.UTF8.GetString(bytes);
            return val;
        }
    }
}
