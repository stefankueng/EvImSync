/********************************************************************************************
 * InterIMAP
 * Copyright (C) 2008-2009 Jason Miesionczek
 * Original Author: Rohit Joshi
 * Based on this article on codeproject.com:
 * IMAP Client library using C#
 * http://www.codeproject.com/KB/IP/imaplibrary.aspx?msg=2498332
 * Posted: August 16th 2004
 * 
 * ZipStorer code written by Jaime Olivares
 * http://www.codeproject.com/KB/recipes/ZipStorer.aspx
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
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Attributes;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Asynchronous.Objects
{
    /// <summary>
    /// An individual section of content for a message
    /// </summary>
    [LinkToTable("Content")]
    public class MessageContent : IMessageContent
    {
        #region PrivateFields

        private readonly IMAPAsyncClient _client;
        private readonly int _id;
        #endregion

        #region CTOR
        /// <summary>
        /// Create a new content object. The ID must already exist in the content table
        /// </summary>
        /// <param name="client"></param>
        /// <param name="contentID"></param>
        public MessageContent(IMAPAsyncClient client, int contentID)
        {
            _client = client;
            _id = contentID;
        }
        #endregion

        #region Public Properties
        public int ID
        {
            get { return _id; }
        }
        
        public string ContentDescription
        {
            get { return _client.DataManager.GetValue<MessageContent, String>(this, "ContentDescription"); }
            set { _client.DataManager.SetValue(this, "ContentDescription", value); }
        }

        public string MIMEVersion
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "MIMEVersion"); }
            set { _client.DataManager.SetValue(this, "MIMEVersion", value); }
        }

        public string ContentFilename
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "ContentFilename"); }
            set { _client.DataManager.SetValue(this, "ContentFilename", value); }
        }

        public string ContentDisposition
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "ContentDisposition"); }
            set { _client.DataManager.SetValue(this, "ContentDisposition", value); }
        }

        public string ContentId
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "ContentId"); }
            set { _client.DataManager.SetValue(this, "ContentId", value); }
        }

        public string PartID
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "PartID"); }
            set { _client.DataManager.SetValue(this, "PartID", value); }
        }

        public string TextData
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "TextData"); }
            set { _client.DataManager.SetValue(this, "TextData", value); }
        }

        public byte[] BinaryData
        {
            get { return _client.DataManager.GetValue<MessageContent, byte[]>(this, "BinaryData"); }
            set { _client.DataManager.SetValue(this, "BinaryData", value); }
        }

        public string ContentType
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "ContentType"); }
            set { _client.DataManager.SetValue(this, "ContentType", value); }
        }

        public string ContentTransferEncoding
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "ContentTransferEncoding"); }
            set { _client.DataManager.SetValue(this, "ContentTransferEncoding", value); }
        }

        public Int64 ContentSize
        {
            get { return _client.DataManager.GetValue<MessageContent, Int64>(this, "ContentSize"); }
            set { _client.DataManager.SetValue(this, "ContentSize", value); }
        }

        public int MessageID
        {
            get { return _client.DataManager.GetValue<MessageContent, int>(this, "MessageID"); }
            set { _client.DataManager.SetValue(this, "MessageID", value); }
        }

        public Int64 Lines
        {
            get { return _client.DataManager.GetValue<MessageContent, Int64>(this, "Lines"); }
            set { _client.DataManager.SetValue(this, "Lines", value); }
        }

        public string MD5
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "MD5"); }
            set { _client.DataManager.SetValue(this, "MD5", value); }
        }

        public string Language
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "Language"); }
            set { _client.DataManager.SetValue(this, "Language", value); }
        }

        public string Charset
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "Charset"); }
            set { _client.DataManager.SetValue(this, "Charset", value); }
        }

        public IMessage Message
        {
            get { return _client.MailboxManager.GetMessageByID(MessageID); }
        }

        public string HTMLData
        {
            get { return _client.DataManager.GetValue<MessageContent, string>(this, "HTMLData"); }
            set { _client.DataManager.SetValue(this, "HTMLData", value); }
        }

        public bool IsAttachment
        {
            get { return BinaryData != null && BinaryData.Length > 0; }
        }
        #endregion
    }
}
