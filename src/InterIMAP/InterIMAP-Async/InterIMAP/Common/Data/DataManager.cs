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
using InterIMAP.Asynchronous.Client;
using InterIMAP.Asynchronous.Objects;
using InterIMAP.Common.Attributes;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Data
{
    /// <summary>
    /// Responsible for the reading and writing of data to the mailbox
    /// </summary>
    public class DataManager
    {
        #region Private Fields
        private Mailbox _mbx;        
        private readonly object _lockObj;
        private readonly IMAPAsyncClient _client;
        #endregion

        #region CTOR
        /// <summary>
        /// Create a new data manager for the specified client
        /// </summary>
        /// <param name="client"></param>
        public DataManager(IMAPAsyncClient client)
        {            
            _client = client;
            _lockObj = new object();
        }
        #endregion
        
        #region Public Properties
        /// <summary>
        /// The Database object being managed
        /// </summary>
        public Mailbox Db
        {
            get { return _mbx; }
        }

        /// <summary>
        /// Returns fully populated FolderTable
        /// </summary>
        public Mailbox.FolderDataTable FolderTable
        {
            get
            {                
                return _mbx.Folder;
            }
        }

        /// <summary>
        /// Returns fully populated ContactTable
        /// </summary>
        public Mailbox.ContactDataTable ContactTable
        {
            get { return _mbx.Contact; }
        }

        /// <summary>
        /// Returns fully populated MessageTable
        /// </summary>
        public Mailbox.MessageDataTable MessageTable
        {
            get { return _mbx.Message; }
        }

        /// <summary>
        /// Returns fully populated ContentTable
        /// </summary>
        public Mailbox.ContentDataTable ContentTable
        {
            get { return _mbx.Content; }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Initializes a new Database based on the embedded schema
        /// </summary>
        public void New()
        {                   
            _mbx = new Mailbox();                            
        }

        internal bool RenameFolder(IFolder sourceFolder, string newName)
        {
            lock (_lockObj)
            {
                IFolder parentFolder = sourceFolder.Parent;
                if (parentFolder == null)
                {
                    sourceFolder.Name = newName;
                    sourceFolder.FullPath = newName;
                }
                else
                {
                    sourceFolder.Name = newName;
                    sourceFolder.FullPath = String.Format("{0}/{1}", parentFolder.FullPath, newName);
                }
            }

            return true;
        }

        internal bool MoveFolder(IFolder folder, IFolder newParent)
        {
            lock (_lockObj)
            {
                folder.ParentID = newParent == null ? -1 : newParent.ID;
                return true;
            }
        }

        internal bool DeleteFolder(IFolder deadFolder)
        {
            lock (_lockObj)
            {
                Mailbox.FolderRow[] row = (Mailbox.FolderRow[])FolderTable.Select(String.Format("ID = {0}", deadFolder.ID));
                if (row.Length == 1)
                {
                    FolderTable.RemoveFolderRow(row[0]);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal IMessageContent NewContent(int messageID)
        {            
            lock (_lockObj)
            {
                Mailbox.ContentRow row = ContentTable.NewContentRow();

                row.MessageID = messageID;
                ContentTable.Rows.Add(row);
                ContentTable.AcceptChanges();

                return new MessageContent(_client, row.ID);
            }            
        }
               
        internal IFolder NewFolder(string name, IFolder parent)
        {            
            lock (_lockObj)
            {
                string fullPath = parent != null ? String.Format("{0}/{1}", parent.FullPath, name) : name;
                IFolder existingFolder = FolderExists(fullPath);
                if (existingFolder != null)
                    return existingFolder;
                
                Mailbox.FolderDataTable folderTable = FolderTable;
                Mailbox.FolderRow row = folderTable.NewFolderRow();

                row.Name = name;
                row.ParentID = parent != null ? parent.ID : -1;
                row.FullPath = fullPath;
                row.Exists = 0;
                row.Recent = 0;
                
                folderTable.Rows.Add(row);
                folderTable.AcceptChanges();

                return new Folder(_client, row.ID);
            }            
        }

        internal IMessage NewMessage(int UID, int FolderID)
        {            
            lock (_lockObj)
            {
                IMessage msg;
                if (MessageExists(UID, FolderID, out msg)) return msg;

                Mailbox.MessageDataTable messageTable = MessageTable;
                Mailbox.MessageRow row = messageTable.NewMessageRow();

                row.UID = UID;
                row.FolderID = FolderID;
                row.DateSent = new DateTime();
                row.DateReceived = new DateTime();
                
                messageTable.AddMessageRow(row);
                messageTable.AcceptChanges();

                return new Message(_client, row.ID);
            }            
        }

        internal bool DeleteMessage(int uid, int folderID)
        {
            lock (_lockObj)
            {
                IMessage msg;
                if (!MessageExists(uid, folderID, out msg)) return false;

                Mailbox.MessageRow[] row = (Mailbox.MessageRow[])MessageTable.Select(String.Format("UID = {0}", uid));
                MessageTable.Rows.Remove(row[0]);
                MessageTable.AcceptChanges();
                return true;

            }
        }

        internal IContact NewContact(string firstName, string lastName, string email)
        {                        
            lock (_lockObj)
            {
                Mailbox.ContactDataTable contactTable = ContactTable;
                Mailbox.ContactRow row = contactTable.NewContactRow();

                row.FirstName = firstName;
                row.LastName = lastName;
                row.EMail = email;
                
                contactTable.AddContactRow(row);
                contactTable.AcceptChanges();

                return new Contact(_client, row.ID);
            }                        
        }

        internal IContact NewContact(string fullName, string email)
        {            
            lock (_lockObj)
            {
                Mailbox.ContactDataTable contactTable = ContactTable;
                Mailbox.ContactRow row = contactTable.NewContactRow();

                //row.FirstName = firstName;
                //row.LastName = lastName;
                row.FullName = fullName;
                row.EMail = email;
                
                contactTable.AddContactRow(row);
                contactTable.AcceptChanges();

                return new Contact(_client, row.ID);
            }            
        }

        internal void AssociateContact(string tableName, int messageID, int contactID)
        {                        
            lock (_lockObj)
            {
                DataTable dt = Db.Tables[tableName];
                DataRow row = dt.NewRow();

                row["MessageID"] = messageID;
                row["ContactID"] = contactID;
                
                dt.Rows.Add(row);
                dt.AcceptChanges();
            }            
        }

        internal IContact GetContactByEMail(string email)
        {
            lock (_lockObj)
            {
                Mailbox.ContactDataTable cdt = ContactTable;
                Mailbox.ContactRow[] rows = (Mailbox.ContactRow[]) cdt.Select(String.Format("EMail like '{0}'", email));

                if (rows.Length > 0)
                    return new Contact(_client, rows[0].ID);
            }

            return null;
        }

        internal IMessage[] GetMessagesByFolder(IFolder folder)
        {
            lock (_lockObj)
            {
                List<IMessage> msgs = new List<IMessage>();                
                Mailbox.MessageRow[] rows = (Mailbox.MessageRow[]) MessageTable.Select("FolderID = " + folder.ID);
                foreach (Mailbox.MessageRow row in rows)
                    msgs.Add(new Message(_client, row.ID));

                return msgs.ToArray();
            }
        }

        internal Mailbox.ContentRow[] GetContentRowsByMessageID(int messageId)
        {
            lock (_lockObj)
            {
                return (Mailbox.ContentRow[])ContentTable.Select(String.Format("MessageID = {0}", messageId));
            }
        }

        internal IMessage[] GetMessagesByFolder(IFolder folder, MessageListDirection direction)
        {
            lock (_lockObj)
            {
                List<IMessage> msgs = new List<IMessage>();                
                Mailbox.MessageRow[] rows = (Mailbox.MessageRow[])MessageTable.Select("FolderID = " + folder.ID);
                foreach (Mailbox.MessageRow row in rows)
                    msgs.Add(new Message(_client, row.ID));

                if (direction == MessageListDirection.Descending)
                    msgs.Reverse();

                return msgs.ToArray();
            }
        }

        internal void SetValue<T>(T source, string col, object value) where T : IBaseObject
        {                        
            lock (_lockObj)
            {
                string table = GetObjectTableName(source.GetType());
                DataTable dt = _mbx.Tables[table];
                DataRow[] rows = dt.Select("ID = " + source.ID);
                if (rows.Length == 0) return;
                
                rows[0][col] = value;
                dt.AcceptChanges();
            }
        }

        internal K GetValue<T, K>(T source, string col) where T : IBaseObject
        {
            lock (_lockObj)
            {
                string table = GetObjectTableName(source.GetType());
                DataTable dt = _mbx.Tables[table];
                DataRow[] rows = dt.Select("ID = " + source.ID);
                if (rows.Length == 0) return default(K);
                if (rows[0][col] == DBNull.Value)
                    return default(K);

                return (K)rows[0][col];
            }            
        }

        /// <summary>
        /// Get the name of the table that the specified object stores its data in
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetObjectTableName(Type type)
        {
            object[] attribs = type.GetCustomAttributes(true);

            foreach (object attr in attribs)
            {
                if (attr is LinkToTable)
                {
                    return ((LinkToTable)attr).TableName;
                }
            }

            throw new Exception(String.Format("Type: {0} does not contain a LinkToTable attribute", type.Name));
        }

        internal void UpdateFullPath(IFolder folder)
        {
            if (folder.ParentID == -1)
                folder.FullPath = folder.Name;
            else
                folder.FullPath = String.Format("{0}/{1}", folder.Parent.FullPath, folder.Name);
        }
        #endregion

        #region Private Methods
        private bool MessageExists(int uid, int folderid, out IMessage msg)
        {
            lock (_lockObj)
            {
                Mailbox.MessageDataTable messageTable = MessageTable;

                Mailbox.MessageRow[] rows =
                    (Mailbox.MessageRow[])
                    messageTable.Select(String.Format("UID = {0} AND FolderID = {1}", uid, folderid));

                msg = rows.Length > 0 ? new Message(_client, rows[0].ID) : null;

                return (rows.Length > 0);
            }
        }

        private IFolder FolderExists(string path)
        {
            lock (_lockObj)
            {                                
                foreach (Mailbox.FolderRow row in FolderTable.Rows)
                {
                    IFolder f = new Folder(_client, row.ID);                                
                    if (f.FullPath.Equals(path))
                        return f;
                }
                                                
                return null;
            }
        }
        #endregion

        #region Internal Methods
        internal bool CanAssociate(int ID, int contactID, string sourceTable)
        {
            
            lock (_lockObj)
            {
                DataTable dt = _client.DataManager.Db.Tables[sourceTable];
                DataRow[] rows = dt.Select(String.Format("MessageID = {0} AND ContactID = {1}", ID, contactID));
                if (rows.Length == 0) return true;
            }
            

            return false;
        }
        #endregion
    }
}
