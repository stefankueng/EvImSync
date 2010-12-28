/********************************************************************************************
 * IMAPMessageContent.cs
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

namespace InterIMAP.Synchronous
{
    /// <summary>
    /// Simple enumeration to set the expected format of the messages content
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        /// a Plain/Text content section
        /// </summary>
        Text,
        /// <summary>
        /// a Plain/HTML content section
        /// </summary>
        HTML,
        /// <summary>
        /// An unknown content section
        /// </summary>
        Unknown
    }
    
    /// <summary>
    /// Container to store the differnt kinds of content that can be contained in a message
    /// </summary>
    [Serializable]
    public class IMAPMessageContent
    {
        #region Private Fields
        private string _partID;        
        private string _textData;
        private byte[] _binaryData;
        private string _contentType;
        private string _contentTransferEncoding;        
        private int _contentSize;
        private string _contentID;
        private string _contentDisposition;
        private string _contentFilename;
        private string _mimeVersion;
        private string _contentDescription;
        #endregion

        #region Public Properties
        /// <summary>
        /// Description of message content. Sometimes contains attachment filename.
        /// </summary>
        public string ContentDescription
        {
            get { return _contentDescription; }
            set { _contentDescription = value; }
        }
        /// <summary>
        /// The MIME-Version field
        /// </summary>
        public string MIMEVersion
        {
            get { return _mimeVersion; }
            set { _mimeVersion = value; }
        }

        /// <summary>
        /// The filename of the embedded image or attachment
        /// </summary>
        public string ContentFilename
        {
            get { return _contentFilename; }
            set { _contentFilename = value; }
        }

        /// <summary>
        /// Content-Disposition field. Used for embedded images and file attachments
        /// </summary>
        public string ContentDisposition
        {
            get { return _contentDisposition; }
            set { _contentDisposition = value; }
        }
        /// <summary>
        /// The Content-Id field
        /// </summary>
        public string ContentId
        {
            get { return _contentID; }
            set { _contentID = value; }
        }

        /// <summary>
        /// The section of the message this content came from
        /// </summary>
        public string PartID
        {
            get { return _partID; }
            set { _partID = value; }
        }

        /// <summary>
        /// Contains the textual data on this content section
        /// </summary>
        public string TextData
        {
            get { return _textData; }
            set { _textData = value; }
        }

        /// <summary>
        /// Contains the binary data of this content section. Only used for embedded images and attachments
        /// </summary>
        public byte[] BinaryData
        {
            get { return _binaryData; }
            set { _binaryData = value; }
        }

        /// <summary>
        /// Content type enumeration based on the string returned from the server
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        /// <summary>
        /// The Encoding method used for this content section
        /// </summary>
        public string ContentTransferEncoding
        {
            get { return _contentTransferEncoding; }
            set { _contentTransferEncoding = value; }
        }

        /// <summary>
        /// The size of the content in bytes
        /// </summary>
        public int ContentSize
        {
            get { return _contentSize; }
            set { _contentSize = value; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default constructor. need for serialization.
        /// </summary>
        public IMAPMessageContent() { }
        #endregion
    }
}
