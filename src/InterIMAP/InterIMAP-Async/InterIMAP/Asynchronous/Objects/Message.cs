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
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common;
using InterIMAP.Common.Attributes;
using InterIMAP.Common.Data;
using InterIMAP.Common.Interfaces;
using System.Text.RegularExpressions;
using System.Text;

namespace InterIMAP.Asynchronous.Objects
{
    /// <summary>
    /// A single Message object in the mailbox
    /// </summary>
    [LinkToTable("Message")]
    public class Message : IMessage
    {
        #region Private Fields
        private readonly int _id;
        private readonly IMAPAsyncClient _client;
        #endregion

        #region Public Properties
        public int ID
        {
            get { return _id; }
        }

        public int UID
        {
            get { return _client.DataManager.GetValue<Message, int>(this, "UID"); }
            set { _client.DataManager.SetValue(this, "UID", value); }
        }

        [HeaderName("subject")]
        public string Subject
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "Subject"); }
            set
            {
                string temp = value.Trim();
                if (temp.Contains("=?"))
                {
                    temp = DecodeSubject(temp);
                }
                _client.DataManager.SetValue(this, "Subject", temp);
            }
        }

        [HeaderName("date")]
        public DateTime DateSent
        {
            get { return _client.DataManager.GetValue<Message, DateTime>(this, "DateSent"); }
            set { _client.DataManager.SetValue(this, "DateSent", value); }
        }

        [HeaderName("date")]
        public DateTime DateReceived
        {
            get { return _client.DataManager.GetValue<Message, DateTime>(this, "DateReceived"); }
            set { _client.DataManager.SetValue(this, "DateReceived", value); }
        }

        [HeaderName("receivedspf")]
        public string ReceivedSPF
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "ReceivedSPF"); }
            set { _client.DataManager.SetValue(this, "ReceivedSPF", value); }
        }

        [HeaderName("contenttransferencoding")]
        public string ContentTransferEncoding
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "ContentTransferEncoding"); }
            set { _client.DataManager.SetValue(this, "ContentTransferEncoding", value); }
        }

        [HeaderName("deliveredto")]
        public string DeliveredTo
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "DeliveredTo"); }
            set { _client.DataManager.SetValue(this, "DeliveredTo", value); }
        }

        [HeaderName("xgmailreceived")]
        public string XGMailReceived
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "XGmailReceived"); }
            set { _client.DataManager.SetValue(this, "XGMailReceived", value); }
        }

        [HeaderName("organization")]
        public string Organization
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "Organization"); }
            set { _client.DataManager.SetValue(this, "Organization", value); }
        }

        [HeaderName("inreplyto")]
        public string InReplyTo
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "InReplyTo"); }
            set { _client.DataManager.SetValue(this, "InReplyTo", value); }
        }

        [HeaderName("xoriginatingip")]
        public string XOriginatingIP
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "XOriginatingIP"); }
            set { _client.DataManager.SetValue(this, "XOriginatingIP", value); }
        }

        [HeaderName("received")]
        public string Received
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "Received"); }
            set { _client.DataManager.SetValue(this, "Received", value); }
        }

        [HeaderName("mimeversion")]
        [HeaderName("mime-version")]
        public string MimeVersion
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "MimeVersion"); }
            set { _client.DataManager.SetValue(this, "MimeVersion", value); }
        }

        [HeaderName("contenttype")]
        [HeaderName("content-type")]
        public string ContentType
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "ContentType"); }
            set { _client.DataManager.SetValue(this, "ContentType", value); }
        }

        [HeaderName("contentclass")]
        [HeaderName("content-class")]
        public string ContentClass
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "ContentClass"); }
            set { _client.DataManager.SetValue(this, "ContentClass", value); }
        }

        [HeaderName("returnpath")]
        [HeaderName("return-path")]
        public string ReturnPath
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "ReturnPath"); }
            set { _client.DataManager.SetValue(this, "ReturnPath", value); }
        }

        [HeaderName("xmailer")]
        [HeaderName("x-mailer")]
        public string XMailer
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "XMailer"); }
            set { _client.DataManager.SetValue(this, "XMailer", value); }
        }

        [HeaderName("xmimeole")]
        [HeaderName("x-mimeole")]
        public string XMimeOLE
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "XMimeOLE"); }
            set { _client.DataManager.SetValue(this, "XMimeOLE", value); }
        }

        [HeaderName("xoriginalarrivaltime")]
        [HeaderName("x-originalarrivaltime")]
        public string XOriginalArrivalTime
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "XOriginalArrivalTime"); }
            set { _client.DataManager.SetValue(this, "XOriginalArrivalTime", value); }
        }

        [HeaderName("messageid")]
        [HeaderName("message-id")]
        public string MessageID
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "MessageID"); }
            set { _client.DataManager.SetValue(this, "MessageID", value); }
        }

        [HeaderName("xmstnefcorrelator")]
        [HeaderName("x-ms-tnef-correlator")]
        public string XMSTNEFCorrelator
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "XMSTNEFCorrelator"); }
            set { _client.DataManager.SetValue(this, "XMSTNEFCorrelator", value); }
        }

        [HeaderName("threadtopic")]
        [HeaderName("thread-topic")]
        public string ThreadTopic
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "ThreadTopic"); }
            set { _client.DataManager.SetValue(this, "ThreadTopic", value); }
        }

        [HeaderName("threadindex")]
        [HeaderName("thread-index")]
        public string ThreadIndex
        {
            get { return _client.DataManager.GetValue<Message, String>(this, "ThreadIndex"); }
            set { _client.DataManager.SetValue(this, "ThreadIndex", value); }
        }

        public int FolderID
        {
            get { return _client.DataManager.GetValue<Message, int>(this, "FolderID"); }
            set { _client.DataManager.SetValue(this, "FolderID", value); }
        }

        [ConnectingTable("MessageToContacts")]
        public IContact[] ToContacts
        {
            get { return GetContacts("ToContacts"); }
        }

        [ConnectingTable("MessageFromContacts")]
        public IContact[] FromContacts
        {
            get { return GetContacts("FromContacts"); }
        }

        [ConnectingTable("MessageCcContacts")]
        public IContact[] CcContacts
        {
            get { return GetContacts("CcContacts"); }
        }

        [ConnectingTable("MessageBccContacts")]
        public IContact[] BccContacts
        {
            get { return GetContacts("BccContacts"); }
        }

        public IMessageContent[] MessageContent
        {
            get { return _client.MailboxManager.GetMessageContent(_id); }
        }

        public IFolder Folder
        {
            get { return _client.MailboxManager.GetFolderByID(FolderID); }
        }

        public string TextData
        {
            get
            {
                foreach (IMessageContent content in MessageContent)
                {
                    if (!string.IsNullOrEmpty(content.TextData))
                        return content.TextData;
                }

                return null;
            }
        }

        public string HTMLData
        {
            get
            {
                foreach (IMessageContent content in MessageContent)
                    if (!string.IsNullOrEmpty(content.HTMLData))
                        return content.HTMLData;

                return null;
            }
        }

        public bool Seen
        {
            get { return _client.DataManager.GetValue<Message, bool>(this, "Seen"); }
            set { _client.MailboxManager.SetMessageFlag(this, MessageFlag.Seen, value, true); }
        }

        public bool Deleted
        {
            get { return _client.DataManager.GetValue<Message, bool>(this, "Deleted"); }
            set { _client.MailboxManager.SetMessageFlag(this, MessageFlag.Deleted, value, true); }
        }

        public bool Answered
        {
            get { return _client.DataManager.GetValue<Message, bool>(this, "Answered"); }
            set { _client.MailboxManager.SetMessageFlag(this, MessageFlag.Answered, value, true); }
        }

        public bool Draft
        {
            get { return _client.DataManager.GetValue<Message, bool>(this, "Draft"); }
            set { _client.MailboxManager.SetMessageFlag(this, MessageFlag.Draft, value, true); }
        }

        public bool Flagged
        {
            get { return _client.DataManager.GetValue<Message, bool>(this, "Flagged"); }
            set { _client.MailboxManager.SetMessageFlag(this, MessageFlag.Flagged, value, true); }
        }

        public bool Recent
        {
            get { return _client.DataManager.GetValue<Message, bool>(this, "Recent"); }
        }

        public bool New
        {
            get { return !Seen; }
            set { Seen = !value; }
        }

        public bool HeaderLoaded
        {
            get
            {
                return !String.IsNullOrEmpty(ContentType);
            }
        }

        public bool ContentLoaded
        {
            get
            {
                bool loaded = false;
                foreach (IMessageContent content in MessageContent)
                {
                    loaded = loaded || (MessageContent.Length > 0 && (!String.IsNullOrEmpty(content.TextData) || !String.IsNullOrEmpty(content.HTMLData) || content.BinaryData != null));
                    if (loaded)
                        break;
                }
                return loaded;
            }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Create a Message object with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="client"></param>
        public Message(IMAPAsyncClient client, int id)
        {
            _id = id;
            _client = client;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Associate a list of contacts with the specified contact property
        /// </summary>
        /// <param name="propName">"ToContacts", "FromContacts", "CcContacts", "BccContacts"</param>
        /// <param name="contacts">List of contacts</param>
        public void AssociateContacts(string propName, IContact[] contacts)
        {
            if (contacts == null) return;

            string sourceTable = GetSourceTable(propName);
            if (sourceTable == null) return;

            foreach (IContact contact in contacts)
            {
                if (!CanAssociate(sourceTable, contact)) continue;
                _client.DataManager.AssociateContact(sourceTable, ID, contact.ID);
            }
        }

        /// <summary>
        /// returns a list of custom flags this message has
        /// </summary>
        public List<string> GetCustomFlags()
        {
            string flaglines = _client.DataManager.GetValue<Message, string>(this, "CustomFlags");
            string[] f = flaglines.Split('\n');
            List<string> flags = new List<string>();
            foreach (string s in f)
            {
                flags.Add(s);
            }
            return flags;
        }

        /// <summary>
        /// returns the state of the specified custom flag
        /// </summary>
        public bool GetCustomFlag(string flag)
        {
            string flaglines = _client.DataManager.GetValue<Message, string>(this, "CustomFlags");
            string[] f = flaglines.Split('\n');
            foreach (string s in f)
            {
                if (s == flag)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets or removes a custom flag from this message
        /// </summary>
        /// <param name="flag">the name of the flag</param>
        /// <param name="value">true to set the flag, false to remove it</param>
        public void SetCustomFlag(string flag, bool value)
        {
            string newFlagLines = string.Empty;
            string flaglines = _client.DataManager.GetValue<Message, string>(this, "CustomFlags");
            string[] f = flaglines.Split('\n');
            foreach (string s in f)
            {
                if ((!value) || (s != flag))
                    newFlagLines = newFlagLines + "\n" + s;
            }
            if (value)
                newFlagLines += "\n" + flag;
            _client.DataManager.SetValue(this, "CustomFlags", newFlagLines);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Determines if the specified contact is already associated with the specified table for this message
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        // ReSharper disable SuggestBaseTypeForParameter
        private bool CanAssociate(string sourceTable, IContact contact)
        // ReSharper restore SuggestBaseTypeForParameter
        {
            return _client.DataManager.CanAssociate(ID, contact.ID, sourceTable);
        }

        /// <summary>
        /// Returns all the contacts associated with the specified property
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        private IContact[] GetContacts(string propName)
        {
            string sourceTable = GetSourceTable(propName);

            if (sourceTable == null) return null;

            List<IContact> _contacts = new List<IContact>();

            DataTable dt = _client.DataManager.Db.Tables[sourceTable];
            Mailbox.ContactDataTable cdt = _client.DataManager.ContactTable;
            foreach (DataRow row in dt.Select("MessageID = " + ID))
            {
                int id = (int)row["ContactID"];
                foreach (Mailbox.ContactRow crow in cdt.Select("ID = " + id))
                {
                    _contacts.Add(new Contact(_client, crow.ID));
                }
            }

            return _contacts.ToArray();

        }

        /// <summary>
        /// Gets the name of the table that contacts of the specified property are stored in
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        private string GetSourceTable(string propName)
        {
            PropertyInfo pi = GetType().GetProperty(propName);
            foreach (object obj in pi.GetCustomAttributes(true))
            {
                if (obj is ConnectingTable)
                {
                    return ((ConnectingTable)obj).ConnectingTableName;
                }
            }
            return null;
        }

        private string charSet = "";
        /// <summary>
        /// =?utf-8?B?U2F2ZWQgc2VhcmNoIHJlc3VsdHMgLSA0LzUvMjAxMA==?=
        /// =?UTF-8?B?VG9waWMgcmVwbHkgbm90aWZpY2F0aW9uIC0gIkhhWGUvQUlSIHByb2plY3Qg?= =?UTF-8?B?dGVtcGxhdGUi?=
        /// =?ISO-8859-1?Q?JASON=2C_An_Exclusive_Financing_Offer_at_Best_Buy=AE?=
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string DecodeSubject(string input)
        {
            StringBuilder sb = new StringBuilder();
            MatchCollection matches = Regex.Matches(input, @"(?<predata>[\w\s]+)?=\?(?<charset>[\S]+)\?(?<encoding>.)\?(?<data>[\S]+[=]*)\?=");
            foreach (Match m in matches)
            {
                charSet = m.Groups["charset"].Value;
                string encoding = m.Groups["encoding"].Value;
                string data = m.Groups["data"].Value;
                string predata = m.Groups["predata"].Value;
                sb.Append(predata);

                Encoding enc = Encoding.GetEncoding(charSet.ToLower());

                if (encoding.ToLower().Equals("b"))
                {
                    byte[] d = Convert.FromBase64String(data);
                    sb.Append(enc.GetString(d));
                }
                else
                {
                    Regex re = new Regex(
                                "(\\=([0-9A-F][0-9A-F]))",
                                RegexOptions.IgnoreCase
                        );

                    string decoded = re.Replace(data, new MatchEvaluator(HexDecoderEvaluator));
                    decoded = decoded.Replace("_", " ");

                    sb.Append(decoded);
                }
            }

            return sb.ToString();
        }

        private string HexDecoderEvaluator(Match m)
        {
            string hex = m.Groups[2].Value;
            int iHex = Convert.ToInt32(hex, 16);

            // Return the string in the charset defined
            byte[] bytes = new byte[1];
            bytes[0] = Convert.ToByte(iHex);
            return Encoding.GetEncoding(charSet).GetString(bytes);

            // This will not work properly on "=85" in example string
            //              char c = (char)iHex;
            //              return c.ToString();
        }
        #endregion
    }
}
