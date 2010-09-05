/********************************************************************************************
 * IMAPMessage.cs
 * Part of the InterIMAP Library
 * Copyright (C) 2004-2007 Rohit Joshi
 * Copyright (C) 2008 Jason Miesionczek
 * Original Author: Rohit Joshi
 * Based on this article on codeproject.com:
 * IMAP Client library using C#
 * http://www.codeproject.com/KB/IP/imaplibrary.aspx?msg=2498332
 * Posted: August 16th 2004
 * 
 * InterIMAP is free software; you can redistribute it and/or modify it under the terms
 * of the GNU Lesser General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * 
 * InterIMAP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with
 * InterIMAP. If not, see http://www.gnu.org/licenses/.
 * 
 * *****************************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.IO;
using System.Reflection;

namespace InterIMAP.Synchronous
{
    /// <summary>
    /// Stores all data about a single e-mail message
    /// </summary>
    [Serializable]
    public class IMAPMessage
    {
        #region Private Fields
        private int _uid;        
        private string _received;        
        private string _mimeVersion;        
        private string _contentType;        
        private string _contentClass;        
        private string _returnPath;        
        private string _xMailer;        
        private string _xMimeOLE;        
        private string _xOriginalArrivalTime;        
        private string _subject;        
        private DateTime _date;        
        private string _messageID;        
        private string _xMSTNEFCorrelator;        
        private string _threadTopic;        
        private string _threadIndex;        
        private List<IMAPMailAddress> _from;        
        private List<IMAPMailAddress> _to;
        private List<IMAPMailAddress> _cc;        
        private List<IMAPMailAddress> _bcc;        
        internal IMAPMessageContent _textData;        
        internal IMAPMessageContent _htmlData;
        internal List<IMAPFileAttachment> _attachments;        
        internal List<IMAPFileAttachment> _embedded;
        private bool _headerLoaded;
        private bool _contentLoaded;
        private bool _attachmentsAvailable;
        private string _xOriginatingIP;
        private string _inReplyTo;
        private IMAPFolder _folder;        
        internal IMAPClient _client;
        private IMAPMessageFlags _flags;
        private PropertyInfo[] _properties;
        private string _organization;
        private string _xgmailreceived;
        private string _deliveredTo;
        private string _contentTransferEncoding;
        private string _receivedSPF;
        internal List<IMAPMessageContent> _bodyParts;
        #endregion

        #region Public Properties
        /// <summary>
        /// Flag to indicate whether the attachments have been downloaded for this message
        /// </summary>
        [XmlIgnore]
        public bool AttachmentsAvailable
        {
            get { return _attachmentsAvailable; }
            set { _attachmentsAvailable = value; }
        }
        /// <summary>
        /// Collection of content parts based on the body structure
        /// </summary>
        public List<IMAPMessageContent> BodyParts
        {
            get 
            {
                if (!ContentLoaded && _client != null)
                {
                    _client._imap.ProcessBodyContent(this);
                    _client._imap.ProcessBodyParts(this);
                }
                    
                return _bodyParts; 
            }
            set { _bodyParts = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string ReceivedSPF
        {
            get { return _receivedSPF; }
            set { _receivedSPF = value; }
        }
        /// <summary>
        /// The encoding method of the message
        /// </summary>
        public string ContentTransferEncoding
        {
            get { return _contentTransferEncoding; }
            set { _contentTransferEncoding = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string DeliveredTo
        {
            get { return _deliveredTo; }
            set { _deliveredTo = value; }
        }

        /// <summary>
        /// Special data used by Gmail
        /// </summary>
        public string XGmailReceived
        {
            get { return _xgmailreceived; }
            set { _xgmailreceived = value; }
        }

        /// <summary>
        /// The organization the message came from
        /// </summary>
        public string Organization
        {
            get { return _organization; }
            set { _organization = value; }
        }

        /// <summary>
        /// Collection of flags for this message
        /// </summary>
        public IMAPMessageFlags Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }
        /// <summary>
        /// Reference to the foler this message is located in.
        /// </summary>
        [XmlIgnore]
        public IMAPFolder Folder
        {
            get { return _folder; }
            set { _folder = value; }
        }

        /// <summary>
        /// The In-Reply-To field of the message
        /// </summary>
        public string InReplyTo
        {
            get { RefreshData(false, false); return _inReplyTo; }
            set { _inReplyTo = value; }
        }

        /// <summary>
        /// The IP of the source of the message
        /// </summary>
        public string XOriginatingIP
        {
            get { RefreshData(false, false); return _xOriginatingIP; }
            set { _xOriginatingIP = value; }
        }

        /// <summary>
        /// The main ID used by the IMAP server to indetify a message
        /// </summary>
        public int Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }

        /// <summary>
        /// List of servers that this message traveled through
        /// </summary>
        public string Received
        {
            get { RefreshData(false, false); return _received; }
            set { _received = value; }
        }

        /// <summary>
        /// Version of the MIME specification being used
        /// </summary>
        public string MimeVersion
        {
            get { RefreshData(false, false); return _mimeVersion; }
            set { _mimeVersion = value; }
        }

        /// <summary>
        /// The main content type for this message
        /// </summary>
        public string ContentType
        {
            get { RefreshData(false, false); return _contentType; }
            set { _contentType = value; }
        }

        /// <summary>
        /// The Content-class section of the message header
        /// </summary>
        public string ContentClass
        {
            get { RefreshData(false, false); return _contentClass; }
            set { _contentClass = value; }
        }

        /// <summary>
        /// Usually the same value as "From"
        /// </summary>
        public string ReturnPath
        {
            get { RefreshData(false, false); return _returnPath; }
            set { _returnPath = value; }
        }

        /// <summary>
        /// Server Application used to send this message
        /// </summary>
        public string XMailer
        {
            get { RefreshData(false, false); return _xMailer; }
            set { _xMailer = value; }
        }

        /// <summary>
        /// X-MimeOLE string from message header
        /// </summary>
        public string XMimeOLE
        {
            get { RefreshData(false, false); return _xMimeOLE; }
            set { _xMimeOLE = value; }
        }

        /// <summary>
        /// Time message was received on the server (UTC)
        /// </summary>
        public string XOriginalArrivalTime
        {
            get { RefreshData(false, false); return _xOriginalArrivalTime; }
            set { _xOriginalArrivalTime = value; }
        }

        /// <summary>
        /// The subject of the message
        /// </summary>
        public string Subject
        {
            get { RefreshData(false, false); return _subject; }
            set { _subject = value; }
        }

        /// <summary>
        /// The date the message was received
        /// </summary>
        public DateTime Date
        {
            get { RefreshData(false, false); return _date; }
            set { _date = value; }
        }

        /// <summary>
        /// An internal ID for the message. Not to be confused with UID.
        /// </summary>
        public string MessageID
        {
            get { RefreshData(false, false); return _messageID; }
            set { _messageID = value; }
        }

        /// <summary>
        /// I have no idea what this is for.
        /// </summary>
        public string XMSTNEFCorrelator
        {
            get { RefreshData(false, false); return _xMSTNEFCorrelator; }
            set { _xMSTNEFCorrelator = value; }
        }

        /// <summary>
        /// Same as the Subject
        /// </summary>
        public string ThreadTopic
        {
            get { RefreshData(false, false); return _threadTopic; }
            set { _threadTopic = value; }
        }

        /// <summary>
        /// Insert something meaningful here
        /// </summary>
        public string ThreadIndex
        {
            get { RefreshData(false, false); return _threadIndex; }
            set { _threadIndex = value; }
        }

        /// <summary>
        /// List of addresses this is message is from
        /// </summary>
        public List<IMAPMailAddress> From
        {
            get { RefreshData(false, false); return _from; }
            set { _from = value; }
        }

        /// <summary>
        /// List of people this message was sent to
        /// </summary>
        public List<IMAPMailAddress> To
        {
            get { RefreshData(false, false); return _to; }
            set { _to = value; }
        }

        /// <summary>
        /// List of people this message was copied to
        /// </summary>
        public List<IMAPMailAddress> Cc
        {
            get { RefreshData(false, false); return _cc; }
            set { _cc = value; }
        }

        /// <summary>
        /// List of people this message blind copied to
        /// </summary>
        public List<IMAPMailAddress> Bcc
        {
            get { RefreshData(false, false); return _bcc; }
            set { _bcc = value; }
        }

        /// <summary>
        /// The content of the message as plain text (if available)
        /// </summary>
        [XmlIgnore]
        public IMAPMessageContent TextData
        {
            get 
            { 
                //RefreshData(true, false); 
                foreach (IMAPMessageContent content in _bodyParts)
                {
                    if (content.ContentType.ToLower().Contains("plain"))
                        return content;
                }
                return null;
            }
            //set { _textData = value; }
        }

        /// <summary>
        /// The content of the message as HTML (if available)
        /// </summary>
        [XmlIgnore]
        public IMAPMessageContent HtmlData
        {
            get 
            { 
                //RefreshData(true, false); 
                foreach (IMAPMessageContent content in _bodyParts)
                {
                    if (content.ContentType.ToLower().Contains("html"))
                        return content;
                }
                return null;
            }
            //set { _htmlData = value; }
        }

        /// <summary>
        /// List of files that have been attached to this message
        /// </summary>
        public List<IMAPFileAttachment> Attachments
        {
            get 
            {
                RefreshData(true, false);
                return _attachments; 
            }
            set { _attachments = value; }
        }

        /// <summary>
        /// List of files that have been embedded inside this message
        /// </summary>
        public List<IMAPFileAttachment> Embedded
        {
            get { RefreshData(true, false); return _embedded; }
            set { _embedded = value; }
        }

        /// <summary>
        /// Flag to indicate if the header data for this message has been loaded
        /// </summary>
        public bool HeaderLoaded
        {
            get { return _headerLoaded; }
            set { _headerLoaded = value; }
        }

        /// <summary>
        /// Flag to indicate if the content data for this message has been loaded
        /// </summary>
        public bool ContentLoaded
        {
            get { return _contentLoaded; }
            set { _contentLoaded = value; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default Constructor
        /// </summary>
        public IMAPMessage()
        {
            _to = new List<IMAPMailAddress>();
            _from = new List<IMAPMailAddress>();
            _cc = new List<IMAPMailAddress>();
            _bcc = new List<IMAPMailAddress>();

            _textData = new IMAPMessageContent();
            _htmlData = new IMAPMessageContent();
            _bodyParts = new List<IMAPMessageContent>();

            _received = String.Empty;

            _attachments = new List<IMAPFileAttachment>();
            _embedded = new List<IMAPFileAttachment>();

            _headerLoaded = false;
            _contentLoaded = false;

            _flags = new IMAPMessageFlags();

            _properties = this.GetType().GetProperties();
        }
        #endregion

        #region Property Methods
        /// <summary>
        /// Uses reflection to search for the specified property and set it to the specified value
        /// </summary>
        /// <param name="propName">Exact name of property to find</param>
        /// <param name="value">Value to assign to the property</param>
        public bool SetPropValue(string propName, object value)
        {
            foreach (PropertyInfo pinfo in _properties)
            {
                if (pinfo.Name.ToLower().Equals(propName.ToLower()))
                {
                    pinfo.SetValue(this, value, null);
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Message Data Methods
        /// <summary>
        /// Marks this message as \Seen on the server
        /// </summary>
        public void MarkAsRead()
        {
            if (_client == null) return;
            _client._imap.MarkMessageAsRead(this);
            _flags.New = false;
        }

        /// <summary>
        /// Manually reload the data for this message from the server.
        /// </summary>
        /// <param name="includeBody">Set to true to refresh the content</param>        
        /// <param name="force">Must to set to true if calling this method explicitly</param>        
        public void RefreshData(bool includeBody, bool force)
        {
            if (_client == null)
                return;
            
            if (_client.OfflineMode)
                return;
            
            if (_headerLoaded && !force)
                return;

            if (_contentLoaded && includeBody && !force)
                return;
            
            //XmlTextWriter txtWriter = new XmlTextWriter();
            //this.Folder._client._imap.FetchMessageObject(this, includeBody);
            _client._imap.ProcessMessageFlags(this);
            _client._imap.ProcessMessageHeader(this, 0);
            //if (includeBody)
            //{
            //    _client._imap.ProcessBodyStructure(this);
            //    _client._imap.ProcessBodyParts(this);
                
            //}

            //if (includeAttachments)
            //    _client._imap.ProcessAttachments(this);
            if (includeBody)
            {
                _client._imap.ProcessBodyContent(this);
                _client._imap.ProcessBodyParts(this);
                _contentLoaded = true;
                _attachmentsAvailable = _attachments.Count > 0;
            }
            
        }
        #endregion

        #region XML Methods
        /// <summary>
        /// Generates an XML structure representing this message
        /// </summary>
        public string GetXML()
        {
            StringWriter s = new StringWriter();
            XmlSerializer xml = new XmlSerializer(typeof(IMAPMessage));
            
            xml.Serialize(s, this);



            return s.ToString();
            
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Simple override to display to the message UID
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _uid.ToString();
        }
        #endregion


    }
}
