/********************************************************************************************
 * IMAPFolder.cs
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
using System.Collections;
using System.Xml.Serialization;

namespace InterIMAP.Synchronous
{
    /// <summary>
    /// Contains all the information for a folder and its sub-folders and messages as stored on the server
    /// </summary>
    [Serializable]
    public class IMAPFolder
    {
        #region Private Fields
        [XmlIgnore]
        private IMAPFolder _parentFolder;        
        private string _folderName;
        private string _parentFolderName;
        [XmlIgnore]
        private IMAPFolderCollection _subFolders;
        internal IMAPMessageCollection _messages;
        [XmlIgnore]
        internal IMAPClient _client;        
        private string _folderPath;
        private IMAPFolderQuota _quota;
        private bool _selectable;
        #endregion

        #region Public Properties
        /// <summary>
        /// Determines whether this folder can be selected
        /// </summary>
        public bool Selectable
        {
            get { return _selectable; }
            set { _selectable = value; }
        }

        /// <summary>
        /// For quota information for this folder
        /// </summary>
        [XmlIgnore]
        public IMAPFolderQuota Quota
        {
            get 
            {
                if (_client == null)
                    return _quota;

                bool unlimited = false;
                int current = 0;
                int total = 0;
                _client._imap.GetQuota(this.FolderPath, ref unlimited, ref current, ref total);
                if (unlimited)
                {
                    _quota.CurrentSize = -1;
                    _quota.MaxSize = -1;
                }
                else
                {
                    _quota.CurrentSize = current;
                    _quota.MaxSize = total;
                }
                    
                
                return _quota; 
            }
            set { _quota = value; }
        }

        /// <summary>
        /// Flag to indicate whether this folder is currently selected. Setting this to True will force the server to select it
        /// </summary>
        [XmlIgnore]
        public bool IsCurrentlySelected
        {
            get
            {
                if (_client != null)
                {
                    if (_client._imap.SelectedFolder == null)
                        return false;
                    
                    if (_client._imap.SelectedFolder.Equals(this))
                        return true;
                    else
                        return false;
                }

                return false;
            }

            set 
            {
                if (value)
                {
                    if (_client != null)
                    {
                        if (!_client.OfflineMode)
                        {
                            _client._imap.SelectFolder(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Flag to indicate whether this folder is currently examined. Setting this to True will force the server to examine it
        /// </summary>
        [XmlIgnore]
        public bool IsCurrentlyExamined
        {
            get
            {
                if (_client != null)
                {
                    if (_client._imap.ExaminedFolder == null)
                        return false;
                    
                    if (_client._imap.ExaminedFolder.Equals(this))
                        return true;
                    else
                        return false;
                }

                return false;
            }

            set
            {
                if (value)
                {
                    if (_client != null)
                    {
                        if (!_client.OfflineMode)
                        {
                            _client._imap.ExamineFolder(this);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// The parent folder of this folder
        /// </summary>
        [XmlIgnore]
        public IMAPFolder ParentFolder
        {
            get { return _parentFolder; }
            set { _parentFolder = value; }
        }

        /// <summary>
        /// The name of the parent folder
        /// </summary>
        public string ParentFolderName
        {
            get { return _parentFolderName; }
            set { _parentFolderName = value; }
        }

        /// <summary>
        /// The name of this folder
        /// </summary>
        public string FolderName
        {
            get { return _folderName; }
            set { _folderName = value; }
        }

        /// <summary>
        /// The collection of folders under this folder
        /// </summary>        
        public IMAPFolderCollection SubFolders
        {
            get { return _subFolders; }
            set { _subFolders = value; }
        }

        /// <summary>
        /// The complete path to this folder from the root.
        /// </summary>
        public string FolderPath
        {
            get { return _folderPath; }
            set { _folderPath = value; }
        }

        /// <summary>
        /// List of messages in this folder
        /// </summary>        
        public IMAPMessageCollection Messages
        {
            get 
            {
                if (this._client == null)
                    return _messages;

                IMAPClient c = this._client;
                if (!c.UpdatingCache && Selectable)
                    GetMessageIDs(false);
                return _messages; 
            }
            set { _messages = value; }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Sets the ParentFolder field for this folder
        /// </summary>
        /// <param name="parent"></param>
        internal void SetParent(IMAPFolder parent)
        {
            this._parentFolder = parent;

            foreach (IMAPFolder f in _subFolders)
            {
                f.SetParent(this);
            }
        }
        /// <summary>
        /// Sets the client field for this folder and all the sub folders
        /// </summary>
        /// <param name="c"></param>
        internal void SetClient(IMAPClient c)
        {
            this._client = c;
            foreach (IMAPFolder f in _subFolders)
            {
                f.SetClient(c);
            }

            foreach (IMAPMessage m in _messages)
            {
                if (m != null)
                {
                    m._client = c;
                    m.Folder = this;
                }
            }
        }

        /// <summary>
        /// Performs a search for new messages on this folder, adding them to the message collection
        /// </summary>
        /// <returns>Number of new messages found</returns>
        public int[] CheckForNewMessages()
        {
            return GetMessageIDs(true);
        }

        /// <summary>
        /// Gets the UIDs for each message in this folder, and populates the Messages collection with IMAPMessage objects
        /// </summary>
        internal int[] GetMessageIDs(bool newOnly)
        {
            List<int> newMsgIDs = new List<int>();
            
            if (this._client == null)
                return null;

            if (this._client.OfflineMode)
                return null;

            IMAPClient c = this._client;

            if (!String.IsNullOrEmpty(_folderPath) || !_folderPath.Equals("\"\""))
            {
                string path = "";
                if (_folderPath.Contains(" "))
                    path = "\"" + _folderPath + "\"";
                else
                    path = _folderPath;

                //if (!this.IsCurrentlyExamined)
                c._imap.ExamineFolder(this);
                List<int> ids = c._imap.GetSelectedFolderMessageIDs(newOnly);
                //_messages.Clear();
                foreach (int id in ids)
                {
                    bool found = false;
                    foreach (IMAPMessage m in _messages)
                    {
                        if (m.Uid == id)
                            found = true;
                    }

                    if (!found)
                    {
                        IMAPMessage msg = new IMAPMessage();
                        msg.Uid = id;
                        msg.Folder = this;
                        msg._client = c;
                        _messages.Add(msg);
                        newMsgIDs.Add(id);
                        c.Log(IMAPBase.LogTypeEnum.INFO, String.Format("Added message UID {0} to folder {1}", id, this.FolderPath));

                    }
                }
            }

            if (_client.Config.AutoGetMsgID)
            {
                
                
                foreach (IMAPFolder f in _subFolders)
                {
                    f.GetMessageIDs(newOnly);
                }
            }

            //_client._messageCount += _messages.Count;
            //foreach (IMAPMessage msg in _messages)
            //{
            //    //ArrayList headerResults = new ArrayList();
            //    //c._imap.FetchPartHeader(msg.Uid.ToString(), "0", headerResults);
            //    c._imap.FetchMessageObject(msg, false);
            //}

            return newMsgIDs.ToArray();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Appends new message to end of this folder
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="content">The content of the message</param>
        public void AppendMessage(IMAPMessage msg, string content)
        {
            this.Select();
            
            // first lets determine what the UID of the new message should be
            int uid = _messages.Count > 0 ? _messages[_messages.Count - 1].Uid : 0;
            uid++;

            msg.Uid = uid;

            //string cmd = "APPEND \"{0}\" (\\Seen) {0}\r\n";
            ArrayList result = new ArrayList();
            _client._imap.SendRaw("APPEND \""+FolderPath+"\" (\\Seen) {"+uid+"}\r\n", true);
            //if (!result[0].ToString().StartsWith("+"))
            //{
            //    _client.Log(IMAPBase.LogTypeEnum.ERROR, "Invalid response from server");
            //    return;
            //}
            //_client._imap.ReadLine();

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Date: {0}{1}", "Mon, 7 Feb 1994 21:52:25 -0800 (PST)", Environment.NewLine);
            sb.AppendFormat("From: {0}{1}", msg.From[0], Environment.NewLine);
            sb.AppendFormat("Subject: {0}{1}", msg.Subject, Environment.NewLine);
            sb.Append("To: ");
            foreach (IMAPMailAddress addr in msg.To)
                sb.AppendFormat("{0}, ", addr);

            sb.Remove(sb.Length - 2, 1);
            sb.Append(Environment.NewLine);
            sb.AppendFormat("Message-Id: <{0}@{1}>{2}", msg.Date.Ticks, msg.From[0].ToString().Substring(msg.From[0].ToString().IndexOf("@")+1).Replace(">",""),Environment.NewLine);
            sb.AppendLine("MIME-Version: 1.0");
            sb.AppendLine("Content-Type: TEXT/PLAIN; CHARSET=US-ASCII");
            sb.AppendLine();
            sb.AppendLine(content);
            sb.AppendLine("\r\n");

            result.Clear();
            _client.Log(IMAPBase.LogTypeEnum.INFO, sb.ToString());
            _client._imap.SendRaw(sb.ToString(), false);


        }

        /// <summary>
        /// Sets the IsCurrentlySelected property to True which forces the server to SELECT this folder
        /// </summary>
        public void Select()
        {
            this.IsCurrentlySelected = true;
        }

        /// <summary>
        /// Sets the IsCurrentlyExamined property to True which forces the server to EXAMINE this folder
        /// </summary>
        public void Examine()
        {
            this.IsCurrentlyExamined = true;
        }

        
        /// <summary>
        /// Finds a message in this folder with the specified UID. Returns null if UID not found.
        /// </summary>
        /// <param name="UID">Message UID to search for</param>
        /// <returns>IMAPMessage object of found message. Null if UID not found.</returns>
        public IMAPMessage GetMessageByID(int UID)
        {
            foreach (IMAPMessage msg in _messages)
            {
                if (msg.Uid == UID)
                    return msg;
            }

            return null;
        }

        /// <summary>
        /// Forces the system to download the full content for each message in this folder and the sub folders.
        /// </summary>
        internal void GetAllMessageData()
        {
            if (Selectable)
            {
                foreach (IMAPMessage msg in Messages)
                {
                    msg.RefreshData(true, true);
                    
                }
            }

            foreach (IMAPFolder f in _subFolders)
            {
                f.GetAllMessageData();
            }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default constructor
        /// </summary>
        public IMAPFolder()
        {
            _subFolders = new IMAPFolderCollection();
            _parentFolder = null;
            _folderName = String.Empty;
            _folderPath = String.Empty;
            _parentFolderName = String.Empty;
            _messages = new IMAPMessageCollection();
            _quota = new IMAPFolderQuota();
        }

        /// <summary>
        /// Alternate Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public IMAPFolder(string name, string parent)
        {
            _subFolders = new IMAPFolderCollection();
            _parentFolder = null;
            _folderName = name;
            _parentFolderName = parent;
            _folderPath = String.Empty;
            _messages = new IMAPMessageCollection();
            _quota = new IMAPFolderQuota();
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Simple override to show the folder name. Only really useful for debugging purposes. 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._folderName;
        }
        #endregion

        #region Message Management Methods
        /// <summary>
        /// This method will delete all the messages in this folder. Use with caution.
        /// </summary>
        public void EmptyFolder()
        {
            if (_client.OfflineMode)
            {
                _client.Log(IMAPBase.LogTypeEnum.WARN, "Cannot delete messages in offline mode.");
                return;
            }
            
            string cmd = "STORE {0}:{1} +FLAGS (\\Deleted)\r\n";
            ArrayList result = new ArrayList();
            if (Messages.Count == 0)
                return;
            int firstUID = Messages[0].Uid;
            int lastUID = Messages[Messages.Count-1].Uid;
            this.Select();
            _client._imap.SendAndReceive(String.Format(cmd, firstUID, lastUID), ref result);
            

            cmd = "EXPUNGE\r\n";
            _client._imap.SendAndReceive(cmd, ref result);
            this.Examine();
            _messages.Clear();
            _client.Log(IMAPBase.LogTypeEnum.INFO, String.Format("Folder {0} emptied successfully.", this.FolderName));
        }

        /// <summary>
        /// Copies all messages in this folder to the specified folder
        /// </summary>
        /// <param name="destFolder">The destination folder all the messages should be copied to</param>
        public void CopyAllMessagesToFolder(IMAPFolder destFolder)
        {
            if (_client.OfflineMode)
            {
                _client.Log(IMAPBase.LogTypeEnum.WARN, "Cannot copy messages in offline mode.");
                return;
            }
            
            if (!this.IsCurrentlySelected)
                this.Select();
            
            string cmd = "UID COPY {0}:{1} \"{2}\"\r\n";
            ArrayList result = new ArrayList();
            int firstMsg = _messages[0].Uid;
            int lastMsg = _messages[_messages.Count-1].Uid;
            cmd = String.Format(cmd, firstMsg, lastMsg, destFolder.FolderPath);
            _client._imap.SendAndReceive(cmd, ref result);

            // TODO: Need to find a way to determine what the UIDs of the copies are and instead of having
            // to pull the same messages from the server again, just copy the IMAPMessage objects and update
            // the UID to the new value. This is possible if copying one message at a time, but is not as
            // easy when copying messages in bulk.
            
            
            foreach (string s in result)
            {
                if (s.Contains("OK"))
                {
                    destFolder.GetMessageIDs(false);
                    _client.UpdateCache(true);
                    _client.Log(IMAPBase.LogTypeEnum.INFO, String.Format("All Messages from {0} successfully copied to {1}.", this.FolderName, destFolder.FolderName));
                    break;
                }
            }
        }

        /// <summary>
        /// Copies the specified message to the specified folder
        /// </summary>
        /// <param name="msg">The message to copy</param>
        /// <param name="destFolder">The folder to copy the message to</param>
        public void CopyMessageToFolder(IMAPMessage msg, IMAPFolder destFolder)
        {
            if (_client.OfflineMode)
            {
                _client.Log(IMAPBase.LogTypeEnum.WARN, "Cannot copy messages in offline mode.");
                return;
            }
            
            string cmd = "UID COPY {0} \"{1}\"\r\n";
            ArrayList result = new ArrayList();
            _client._imap.SendAndReceive(String.Format(cmd, msg.Uid, destFolder.FolderPath), ref result);
            foreach (string s in result)
            {
                if (s.Contains("OK"))
                {
                    // if the copy was successful, tell the destination folder to refresh its message UID list.
                    destFolder.GetMessageIDs(false);
                    int msgCount = destFolder.Messages.Count;
                    // the copy function puts the new message at the end of the folder so lets automatically
                    // load the data for the copy. If for some reason during the folder refresh another new message
                    // was found and added making the copied message not the last one in the folder, thats ok
                    // because as soon as the content is accessed the data will be loaded automatically
                    if (msgCount > 0)
                        destFolder.Messages[msgCount - 1].RefreshData(msg.ContentLoaded, true);
                    _client.UpdateCache(true);
                    _client.Log(IMAPBase.LogTypeEnum.INFO, String.Format("Message with UID {0} successfully copied to folder \"{1}\"", msg.Uid, destFolder.FolderName));
                    break;
                }
            }
        }

        /// <summary>
        /// Moves the specified message to the specified folder
        /// </summary>
        /// <param name="msg">The message to move</param>
        /// <param name="destFolder">The folder to move the message to</param>
        public void MoveMessageToFolder(IMAPMessage msg, IMAPFolder destFolder)
        {
            if (_client.OfflineMode)
            {
                _client.Log(IMAPBase.LogTypeEnum.WARN, "Cannot move messages in offline mode.");
                return;
            }
            
            CopyMessageToFolder(msg, destFolder);
            DeleteMessage(msg);
        }

        /// <summary>
        /// Deletes the specified message
        /// </summary>
        /// <param name="msg">The message to delete</param>
        public void DeleteMessage(IMAPMessage msg)
        {
            if (_client.OfflineMode)
            {
                _client.Log(IMAPBase.LogTypeEnum.WARN, "Cannot delete messages in offline mode.");
                return;
            }
            
            string cmd = "UID STORE {0} +FLAGS (\\Deleted)\r\n";
            ArrayList result = new ArrayList();
            // first we need to put the folder in read/write mode by SELECTing it
            this.Select();
            // mark the specified message as deleted
            _client._imap.SendAndReceive(String.Format(cmd, msg.Uid), ref result);
            // EXPUNGE the \Deleted messages
            _client._imap.SendAndReceive("EXPUNGE\r\n", ref result);
            // remove the message object from the collection
            _messages.Remove(msg);
            // set the folder back to read-only mode
            this.Examine();
            _client.UpdateCache(true);
            _client.Log(IMAPBase.LogTypeEnum.INFO, String.Format("Message wih UID {0} in folder \"{1}\" successfully deleted.", msg.Uid, this.FolderName));
        }
        #endregion

        #region Folder Management Methods
        /// <summary>
        /// Creates a new sub-folder under this folder
        /// </summary>
        /// <param name="name">Name of folder to create</param>
        /// <param name="autoSelect">Automatically select this folder upon successful creation</param>
        public void CreateFolder(string name, bool autoSelect)
        {
            if (_client.OfflineMode)
            {
                _client.Log(IMAPBase.LogTypeEnum.WARN, "Cannot create folders in offline mode.");
                return;
            }
            
            string cmd = "CREATE \"{0}\"\r\n";
            ArrayList result = new ArrayList();
            string newFolder = String.Format("{0}/{1}", this.FolderPath, name);
            _client._imap.SendAndReceive(String.Format(cmd, newFolder), ref result);
            if (result[0].ToString().Contains("OK"))
            {
                IMAPFolder oNewFolder = new IMAPFolder();
                oNewFolder.FolderName = name;
                oNewFolder.FolderPath = newFolder;
                oNewFolder.ParentFolder = this;
                oNewFolder.ParentFolderName = this.FolderName;
                _subFolders.Add(oNewFolder);
                if (autoSelect)
                    oNewFolder.Select();
                _client.UpdateCache(true);
            }
            else
            {
                _client.Log(IMAPBase.LogTypeEnum.ERROR, "Folder could not be created." + result[0].ToString());
            }

        }

        /// <summary>
        /// Deletes a sub-folder under this folder. All messages contained in this folder will be deleted.
        /// </summary>
        /// <param name="name">Name of folder to delete</param>
        public void DeleteFolder(string name)
        {
            if (_client.OfflineMode)
            {
                _client.Log(IMAPBase.LogTypeEnum.WARN, "Cannot delete folders in offline mode.");
                return;
            }
            
            string cmd = "DELETE \"{0}\"\r\n";

            IMAPFolder folderToDelete = null;

            foreach (IMAPFolder f in _subFolders)
            {
                if (f.FolderName.Equals(name))
                {
                    folderToDelete = f;
                    break;
                }
            }

            if (folderToDelete != null)
            {
                ArrayList result = new ArrayList();
                _client._imap.SendAndReceive(String.Format(cmd, folderToDelete.FolderPath), ref result);
                if (result[0].ToString().Contains("OK"))
                {
                    // TODO: check if this folder is selected. if so, select its parent instead
                    
                    _subFolders.Remove(folderToDelete);
                    _client.UpdateCache(true);
                }
            }
        }
        #endregion

        #region Search Methods

        /// <summary>
        /// Searches this folder, and optionally the sub-folders for the specified query
        /// </summary>
        /// <param name="query">IMAPSearchQuery object containing the search options</param>
        /// <returns>A new IMAPSearchResult object containing the results of the query</returns>
        public IMAPSearchResult Search(IMAPSearchQuery query)
        {
            if (_client.OfflineMode)
                return OfflineSearch(query);
            
            IMAPSearchResult result = new IMAPSearchResult();
            result.Query = query;
            result.Folder = this;
            string cmd = "UID SEARCH {0}\r\n";
            StringBuilder q = new StringBuilder();
            ArrayList searchTerms = new ArrayList();
            // first we need to analyze the query and build our search command
            if (query.To.Count > 0)
            {
                foreach (IMAPMailAddress a in query.To)
                {
                    if (a.DisplayName != String.Empty)
                        searchTerms.Add(String.Format("TO \"{0}\"", a.DisplayName));

                    if (a.Address != String.Empty)
                        searchTerms.Add(String.Format("TO \"{0}\"", a.Address));

                }
            }

            if (query.From.Count > 0)
            {
                foreach (IMAPMailAddress a in query.From)
                {
                    if (a.DisplayName != String.Empty)
                        searchTerms.Add(String.Format("FROM \"{0}\"", a.DisplayName));

                    if (a.Address != String.Empty)
                        searchTerms.Add(String.Format("FROM \"{0}\"", a.Address));
                }
            }

            if (query.CC.Count > 0)
            {
                foreach (IMAPMailAddress a in query.CC)
                {
                    if (a.DisplayName != String.Empty)
                        searchTerms.Add(String.Format("CC \"{0}\"", a.DisplayName));

                    if (a.Address != String.Empty)
                        searchTerms.Add(String.Format("CC \"{0}\"", a.Address));
                }
            }

            if (query.BCC.Count > 0)
            {
                foreach (IMAPMailAddress a in query.BCC)
                {
                    if (a.DisplayName != String.Empty)
                        searchTerms.Add(String.Format("BCC \"{0}\"", a.DisplayName));

                    if (a.Address != String.Empty)
                        searchTerms.Add(String.Format("BCC \"{0}\"", a.Address));
                }
            }

            if (query.Subject != String.Empty)
            {
                searchTerms.Add(String.Format("SUBJECT \"{0}\"", query.Subject));
            }

            if (query.Content != String.Empty)
            {
                searchTerms.Add(String.Format("BODY {0}", query.Content));
            }

            if (query.Date != String.Empty)
            {
                searchTerms.Add(String.Format("ON {0}", FormatDateToServer(query.Date)));
            }            

            if (query.BeforeDate != String.Empty)
            {
                searchTerms.Add(String.Format("SENTBEFORE {0}", FormatDateToServer(query.BeforeDate)));
            }

            if (query.AfterDate != String.Empty)
            {
                searchTerms.Add(String.Format("SENTSINCE {0}", FormatDateToServer(query.AfterDate)));
            }

            if (query.Range != null)
            {                
                searchTerms.Add(String.Format("SENTBEFORE {0}", FormatDateToServer(query.Range.EndDate)));
                searchTerms.Add(String.Format("SENTSINCE {0}", FormatDateToServer(query.Range.StartDate)));
                searchTerms.Add(String.Format("SENTON {0}", FormatDateToServer(query.Range.StartDate)));
                searchTerms.Add(String.Format("SENTON {0}", FormatDateToServer(query.Range.EndDate)));
            }

            if (query.Answered)
                searchTerms.Add("ANSWERED");

            if (query.Deleted)
                searchTerms.Add("DELETED");

            if (query.Draft)
                searchTerms.Add("DRAFT");

            if (query.New)
                searchTerms.Add("NEW");

            if (query.Recent)
                searchTerms.Add("RECENT");

            if (query.LargerThan > -1)
                searchTerms.Add(String.Format("LARGER {0}", query.LargerThan));

            if (query.SmallerThan > -1)
                searchTerms.Add(String.Format("SMALLER {0}", query.SmallerThan));

            foreach (string s in searchTerms)
            {
                q.Append(s);
                q.Append(" ");
            }

            if (!this.IsCurrentlyExamined || !this.IsCurrentlySelected)
                this.Examine();

            ArrayList cmdResult = new ArrayList();
            cmd = String.Format(cmd, q.ToString().Trim());
            if (searchTerms.Count == 0)
                throw new ArgumentNullException("No search terms were found");

            _client._imap.SearchMessage(new string[] { q.ToString().Trim() }, true, cmdResult);

            foreach (string id in cmdResult)
            {
                if (id == String.Empty)
                    continue;

                foreach (IMAPMessage msg in _messages)
                {
                    if (msg.Uid == Convert.ToInt32(id))
                    {
                        result.Messages.Add(msg);
                    }
                }
            }

            

            return result;
        }

        /// <summary>
        /// Private method for manually searching the cache when in offline mode
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private IMAPSearchResult OfflineSearch(IMAPSearchQuery query)
        {
            IMAPSearchResult result = new IMAPSearchResult();
            result.Query = query;

            foreach (IMAPMessage msg in _messages)
            {
                if (msg == null)
                    continue;

                #region To
                if (query.To.Count > 0)
                {
                    bool containsAll = true;
                    foreach (IMAPMailAddress a in query.To)
                    {
                        if (!AddressListContains(msg.To, a))
                        {
                            containsAll = false;
                            break;
                        }


                    }

                    if (containsAll)
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region From
                if (query.From.Count > 0)
                {
                    bool containsAll = true;
                    foreach (IMAPMailAddress a in query.From)
                    {
                        if (!AddressListContains(msg.From, a))
                        {
                            containsAll = false;
                            break;
                        }


                    }

                    if (containsAll)
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region CC
                if (query.CC.Count > 0)
                {
                    bool containsAll = true;
                    foreach (IMAPMailAddress a in query.CC)
                    {
                        if (!AddressListContains(msg.Cc, a))
                        {
                            containsAll = false;
                            break;
                        }


                    }

                    if (containsAll)
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region BCC
                if (query.BCC.Count > 0)
                {
                    bool containsAll = true;
                    foreach (IMAPMailAddress a in query.BCC)
                    {
                        if (!AddressListContains(msg.Bcc, a))
                        {
                            containsAll = false;
                            break;
                        }


                    }

                    if (containsAll)
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region Subject
                if (query.Subject != String.Empty)
                {
                    if (msg.Subject.Contains(query.Subject))
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region Content
                if (query.Content != String.Empty)
                {
                    if (msg.TextData.TextData.Contains(query.Content))
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region Answered
                if (query.Answered)
                {
                    if (msg.Flags.Answered)
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region Deleted
                if (query.Deleted)
                {
                    if (msg.Flags.Deleted)
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region Draft
                if (query.Draft)
                {
                    if (msg.Flags.Draft)
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region New
                if (query.New)
                {
                    if (msg.Flags.New)
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region Recent
                if (query.Recent)
                {
                    if (msg.Flags.Recent)
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion

                #region AfterDate
                if (query.AfterDate != String.Empty)
                {
                    DateTime afterDate = new DateTime();
                    if (DateTime.TryParse(query.AfterDate, out afterDate))
                    {
                        if (msg.Date >= afterDate)
                        {
                            if (!result.Messages.Contains(msg))
                                result.Messages.Add(msg);
                        }
                        else
                        {
                            if (result.Messages.Contains(msg))
                                result.Messages.Remove(msg);
                        }
                    }


                }
                #endregion

                #region BeforeDate
                if (query.BeforeDate != String.Empty)
                {
                    DateTime beforeDate = new DateTime();
                    if (DateTime.TryParse(query.BeforeDate, out beforeDate))
                    {
                        if (msg.Date < beforeDate)
                        {
                            if (!result.Messages.Contains(msg))
                                result.Messages.Add(msg);
                        }
                        else
                        {
                            if (result.Messages.Contains(msg))
                                result.Messages.Remove(msg);
                        }
                    }
                }
                #endregion

                #region Date
                if (query.Date != String.Empty)
                {
                    DateTime date = new DateTime();
                    if (DateTime.TryParse(query.Date, out date))
                    {
                        if (msg.Date == date)
                        {
                            if (!result.Messages.Contains(msg))
                                result.Messages.Add(msg);
                        }
                        else
                        {
                            if (result.Messages.Contains(msg))
                                result.Messages.Remove(msg);
                        }
                    }
                }
                #endregion

                #region Date Range
                if (query.Range != null)
                {
                    if (query.Range.DateWithinRange(msg.Date))
                    {
                        if (!result.Messages.Contains(msg))
                            result.Messages.Add(msg);
                    }
                    else
                    {
                        if (result.Messages.Contains(msg))
                            result.Messages.Remove(msg);
                    }
                }
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Helper method to quickly check whether a specific address exists within the specified list
        /// </summary>
        /// <param name="list">The source list to check</param>
        /// <param name="addr">The address to look for</param>
        /// <returns>True if the address was found, False if it wasnt</returns>
        private bool AddressListContains(List<IMAPMailAddress> list, IMAPMailAddress addr)
        {
            foreach (IMAPMailAddress a in list)
            {
                if (a.Address.Equals(addr.Address) || a.DisplayName.Equals(addr.DisplayName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Converts a standard DateTime to a format usable by the IMAP server
        /// </summary>
        /// <param name="date">The date to convert</param>
        /// <returns>The date formatted for use on the IMAP server</returns>
        private string FormatDateToServer(DateTime date)
        {
            string formattedDate = "{0}-{1}-{2}";
            string MonthName = MonthNames[date.Month];
            return String.Format(formattedDate, date.Day, MonthName, date.Year);
                
        }

        /// <summary>
        /// Overload for FormatDateToServer to take a string as a parameter. If the string can be parsed into a date, it is converted.
        /// </summary>
        /// <param name="date">The string containing the date</param>
        /// <returns>The re-formatted date string or String.Empty if the date could not be parsed.</returns>
        private string FormatDateToServer(string date)
        {
            DateTime d = new DateTime();
            if (DateTime.TryParse(date, out d))
                return FormatDateToServer(d);
            else
                return String.Empty;
        }

        internal string[] MonthNames = new string[] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        #endregion
    }

    
}
