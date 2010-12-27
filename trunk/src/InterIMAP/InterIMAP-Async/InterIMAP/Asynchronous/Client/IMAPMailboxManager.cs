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
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using InterIMAP.Common;
using InterIMAP.Asynchronous.Objects;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Attributes;
using InterIMAP.Common.Data;
using InterIMAP.Common.Requests;
using System.Text.RegularExpressions;


namespace InterIMAP.Asynchronous.Client
{
    /// <summary>
    /// Class used to manage objects in a mailbox
    /// </summary>
    public class IMAPMailboxManager
    {
        #region Private Fields
        //private static IMAPMailboxManager _instance;
        private MailboxType _boxType;
        //private bool _physMailboxLoaded;
        private readonly IMAPAsyncClient _client;
        private string _mailboxFile;
        #endregion

        #region CTOR
        /// <summary>
        /// Create a new mailbox for the specified client
        /// </summary>
        /// <param name="client"></param>
        public IMAPMailboxManager(IMAPAsyncClient client)
        {
            _boxType = MailboxType.Virtual;
            //_physMailboxLoaded = false;
            _client = client;
            _client.DataManager.New();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Flag indicating which storage mechanism currently being used
        /// </summary>
        public MailboxType MailboxStorageType
        {
            get { return _boxType; }
        }

        /// <summary>
        /// Returns a list of top-level folders
        /// </summary>
        public IFolder[] Folders
        {
            get
            {
                return GetAllFolders();
            }

        }

        public delegate void AccountDownloadCompleteCallback(int totalFolders, int totalMessages, long totalTime);

        public delegate void AccountDownloadProgressCallback(int messagesCompleted, int totalMessages, IFolder currentFolder);

        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the specified mbx file
        /// </summary>
        /// <param name="mbxFile"></param>
        public void LoadMailbox(string mbxFile)
        {
            _boxType = MailboxType.Physical;
            _client.DataManager.Db.ReadXml(mbxFile);
            //_physMailboxLoaded = true;
        }

        /// <summary>
        /// Creates a new mailbox based on what is currently loaded.
        /// Can be used to create a backup of an existing physical mailbox, or
        /// create a new physical mailbox based on a virtual one.
        /// </summary>
        /// <param name="mbxFile"></param>
        public void SaveNewMailbox(string mbxFile)
        {
            _boxType = MailboxType.Physical;
            _client.DataManager.Db.WriteXml(mbxFile);
        }

        public void CreateNewMailbox(string mbxFile)
        {
            _boxType = MailboxType.Physical;
            _mailboxFile = mbxFile;
            ZipStorer.Create(mbxFile, "InterIMAP Mail Storage File");
        }

        /// <summary>
        /// Creates an empty mailbox
        /// </summary>
        public void InitializeMailbox()
        {
            _boxType = MailboxType.Virtual;
        }


        /// <summary>
        /// Updates physical mailbox to reflect current state of server
        /// </summary>
        public void Synchronize()
        {

        }

        /// <summary>
        /// Adds a new folder to the mailbox, specifying an optional parent
        /// </summary>
        /// <param name="name">The name of the folder to add</param>
        /// <param name="parent">The parent folder, or null for no parent</param>
        /// <returns></returns>
        public IFolder AddFolder(string name, IFolder parent)
        {
            return _client.DataManager.NewFolder(name, parent);
        }

        /// <summary>
        /// Adds a new message stub to the mailbox
        /// </summary>
        /// <param name="uid">The UID from the server</param>
        /// <param name="folderid">The ID of the folder this message is stored in</param>
        /// <returns></returns>
        public IMessage AddMessage(int uid, int folderid)
        {
            return _client.DataManager.NewMessage(uid, folderid);
        }

        /// <summary>
        /// Removes the specified message from the mailbox. Assumes that the message has been successfully
        /// expunged from the server.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="folderid"></param>
        /// <returns></returns>
        public bool RemoveMessage(int uid, int folderid)
        {
            return _client.DataManager.DeleteMessage(uid, folderid);
        }

        /// <summary>
        /// Renames the specified folder
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool RenameFolder(IFolder sourceFolder, string newName)
        {
            return _client.DataManager.RenameFolder(sourceFolder, newName);
        }

        /// <summary>
        /// Moves the specified folder
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="parentFolder"></param>
        /// <returns></returns>
        public bool MoveFolder(IFolder folder, IFolder parentFolder)
        {
            return _client.DataManager.MoveFolder(folder, parentFolder);
        }

        /// <summary>
        /// Deletes the specified folder and all the messages it contains. Sub folders are moved up 
        /// to the parent folder.
        /// </summary>
        /// <param name="deadFolder"></param>
        /// <returns></returns>
        public bool DeleteFolder(IFolder deadFolder)
        {
            if (deadFolder.SubFolders.Length == 0)
            {
                _client.RequestManager.SubmitAndWait(new DeleteFolderRequest(deadFolder,
                    delegate(IRequest req)
                    {
                        if (req.Result.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE)
                            _client.DataManager.DeleteFolder(deadFolder);

                    }), false);

                return true;
            }

            string deadFolderPath = deadFolder.FullPath;
            int newParentID = deadFolder.ParentID;


            List<IFolder> foldersToChange = new List<IFolder>(deadFolder.SubFolders);
            foreach (IFolder f in foldersToChange)
            {
                // first we move the sub folders to the dead folders parent folder
                IFolder f1 = f;
                _client.RequestManager.SubmitAndWait(new MoveFolderRequest(f, deadFolder.Parent,
                                                                           delegate(IRequest req)
                                                                           {
                                                                               _client.DataManager.MoveFolder(f1, deadFolder.Parent);
                                                                           }), false);

                string newFolderName = String.Format("{0}_{1}", deadFolder.Name, f.Name);
                // then we rename each of those folders to include the dead folders name

                _client.RequestManager.SubmitAndWait(new RenameFolderRequest(f, newFolderName,
                                                                             delegate(IRequest req)
                                                                             {
                                                                                 _client.DataManager.RenameFolder(f1, newFolderName);
                                                                             }), false);
            }

            _client.RequestManager.SubmitAndWait(new DeleteFolderRequest(deadFolder,
                                                                         delegate(IRequest req)
                                                                         {
                                                                             if (req.Result.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE)
                                                                                 _client.DataManager.DeleteFolder(deadFolder);

                                                                         }), false);

            return true;
        }
        /// <summary>
        /// Adds a new contact to the mailbox
        /// </summary>
        /// <param name="fname">The contacts First Name</param>
        /// <param name="lname">The contacts Last Name</param>
        /// <param name="email">The contacts E-Mail address</param>
        /// <returns></returns>
        public IContact AddContact(string fname, string lname, string email)
        {
            return _client.DataManager.NewContact(fname, lname, email);
        }

        /// <summary>
        /// Add a new contact to the mailbox
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public IContact AddContact(string fullName, string email)
        {
            return _client.DataManager.NewContact(fullName, email);
        }

        /// <summary>
        /// Add a new message content section to the mailbox
        /// </summary>
        /// <param name="messageID"></param>
        /// <returns></returns>
        public IMessageContent AddMessageContent(int messageID)
        {
            return _client.DataManager.NewContent(messageID);
        }

        /// <summary>
        /// Returns the list of folders that have their parent sent to the ID of rootFolder
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <returns></returns>
        public IFolder[] GetChildFolders(IFolder rootFolder)
        {
            int rootID = rootFolder == null ? -1 : rootFolder.ID;

            Mailbox.FolderDataTable folderTable = _client.DataManager.FolderTable;
            List<IFolder> folders = new List<IFolder>();
            foreach (Mailbox.FolderRow row in folderTable.Select(String.Format("ParentID = {0}", rootID)))
            {
                folders.Add(new Folder(_client, row.ID));
            }

            return folders.ToArray();
        }

        /// <summary>
        /// Get a list of all of the specified folders child folders
        /// </summary>
        /// <param name="parentFolder">The folder of which to get the children</param>
        /// <returns>Array of child folders</returns>
        public IFolder[] GetSubFolders(IFolder parentFolder)
        {
            if (parentFolder == null)
                return GetAllFolders();

            Mailbox.FolderDataTable folderTable = _client.DataManager.FolderTable;
            List<IFolder> folders = new List<IFolder>();
            foreach (Mailbox.FolderRow row in folderTable.Select(String.Format("ParentID = {0}", parentFolder.ID)))
            {
                folders.Add(new Folder(_client, row.ID));
            }

            return folders.ToArray();
        }

        public int GetMessageCount()
        {
            int count = 0;
            foreach (IFolder folder in GetAllFolders())
                count += folder.Messages.Length;

            return count;
        }

        /// <summary>
        /// Returns an array of all the folders in the mailbox. 
        /// </summary>
        /// <returns></returns>
        public IFolder[] GetAllFolders()
        {
            Mailbox.FolderDataTable folderTable =
                _client.DataManager.FolderTable;
            List<IFolder> _folderList = new List<IFolder>();

            foreach (Mailbox.FolderRow row in folderTable.Rows)
            {
                IFolder f = new Folder(_client, row.ID);
                _folderList.Add(f);
            }

            return _folderList.ToArray();
        }

        /// <summary>
        /// Retreives a folder from the mailbox with the specified ID
        /// </summary>
        /// <param name="id">ID of folder to find</param>
        /// <returns>IFolder object, or null if ID not found</returns>
        public IFolder GetFolderByID(int id)
        {
            Mailbox.FolderDataTable folderTable =
                _client.DataManager.FolderTable;

            Mailbox.FolderRow[] rows = (Mailbox.FolderRow[])folderTable.Select(String.Format("ID = {0}", id));
            if (rows.Length == 1)
                return new Folder(_client, rows[0].ID);

            throw new ArgumentOutOfRangeException(String.Format("No folder with ID {0} found.", id));
        }

        /// <summary>
        /// Finds a folder in the mailbox based on its full path
        /// </summary>
        /// <param name="fullpath">The full path of the folder to search for</param>
        /// <returns>IFolder object, or null if the folder was not found</returns>
        public IFolder GetFolderByPath(string fullpath)
        {
            foreach (IFolder folder in GetAllFolders())
            {
                if (folder.FullPath.Equals(fullpath))
                    return folder;
            }

            return null;
        }

        /// <summary>
        /// Finds a folder in the mailbox based on its name. If there are more than one folder
        /// with the same name, the function returns the first one found.
        /// </summary>
        /// <param name="name">The name of the folder to search for</param>
        /// <returns>IFolder object, or null if the folder was not found</returns>
        public IFolder GetFolderByName(string name)
        {
            foreach (IFolder folder in GetAllFolders())
            {
                if (folder.FullPath.EndsWith("/" + name))
                    return folder;
            }

            return null;
        }

        /// <summary>
        /// Submits an Examine request for each folder to store its message count data
        /// </summary>
        public void PopulateFolderData(AsyncBatchProgressCallback progCallback, AsyncBatchCompletedCallback completedCallback)
        {
            List<IRequest> requests = new List<IRequest>();
            foreach (IFolder folder in GetAllFolders())
            {
                requests.Add(new FolderDataRequest(folder, null));
                //requests.Add(new MessageListRequest(folder, null));
            }

            AsyncBatchRequest abr = new AsyncBatchRequest(requests.ToArray(), progCallback, completedCallback);
            _client.RequestManager.SubmitAsyncBatchRequest(abr, false);
        }

        /// <summary>
        /// Downloads every message in every folder in the system. Used for building the local cache
        /// </summary>
        /// <param name="progCallback"></param>
        /// <param name="completedCallback"></param>
        public void DownloadEntireAccount(AccountDownloadProgressCallback progCallback, AccountDownloadCompleteCallback completedCallback)
        {
            int messagesCompleted = 0;
            IFolder[] folderList = GetAllFolders();
            TimeSpan ts = new TimeSpan();
            int totalMessages = GetMessageCount();
            ManualResetEvent mre = new ManualResetEvent(false);
            foreach (IFolder folder in folderList)
            {
                foreach (IMessage msg in folder.Messages)
                {
                    FullMessageRequest fmr = new FullMessageRequest(_client, msg);
                    fmr.MessageComplete += delegate(IMessage m, TimeSpan totalTime)
                                               {
                                                   progCallback(++messagesCompleted, totalMessages, m.Folder);
                                                   StoreMessage(m);
                                                   ts.Add(totalTime);
                                                   if (messagesCompleted >= totalMessages)
                                                       mre.Set();
                                               };
                    fmr.MessageProgress += delegate(IMessage m, long receieved, long total)
                                               {
                                                   Console.CursorLeft = 0;
                                                   Console.Write("{0}/{1}k downloaded", receieved / 1024, total / 1024);
                                               };
                    fmr.MessageProgress += delegate(IMessage m, long bytesReceived, long totalBytes) { };
                    fmr.Start();
                }
            }

            mre.WaitOne();

            completedCallback(folderList.Length, messagesCompleted, ts.Ticks);


        }

        private void StoreMessage(IMessage m)
        {
            if (_boxType != MailboxType.Physical) return;

            MailMessage mmsg = new MailMessage();
            mmsg.Body = m.HTMLData ?? m.TextData;
            mmsg.BodyEncoding = String.IsNullOrEmpty(m.ContentTransferEncoding) ? System.Text.Encoding.Unicode : System.Text.Encoding.GetEncoding(m.ContentTransferEncoding);
            mmsg.From = new MailAddress(m.FromContacts[0].EMail, m.FromContacts[0].FullName);
            mmsg.IsBodyHtml = m.HTMLData != null;
            mmsg.ReplyToList.Add(String.IsNullOrEmpty(m.InReplyTo) ? new MailAddress(m.FromContacts[0].EMail) : new MailAddress(m.InReplyTo));
            mmsg.Subject = m.Subject;
            foreach (IContact c in m.ToContacts)
                mmsg.To.Add(new MailAddress(c.EMail, c.FullName));


            foreach (IMessageContent content in m.MessageContent)
            {
                if (!content.IsAttachment) continue;
                ContentType ctype = new ContentType(content.ContentType);
                Attachment attach = new Attachment(new MemoryStream(content.BinaryData), ctype);
                mmsg.Attachments.Add(attach);
            }

            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            string saveDir = @"i:\";
            client.PickupDirectoryLocation = saveDir;
            client.Send(mmsg);
            DirectoryInfo dinfo = new DirectoryInfo(saveDir);
            FileInfo[] files = dinfo.GetFiles("*.eml");
            if (files.Length == 0) throw new FileNotFoundException("No .eml files were found in specified location");
            string pathToEml = files[0].FullName;
            string messagePath = String.Format(@"{0}\{1}\{2}", m.Folder.FullPath, m.UID, Path.GetFileName(pathToEml));
            ZipStorer zip = ZipStorer.Open(_mailboxFile, FileAccess.ReadWrite);
            zip.AddFile(pathToEml, messagePath, messagePath);
            zip.Close();

            // TODO: add folder and filename info to some kind of xml file stored inside the zip
        }

        /// <summary>
        /// Retreives the name of the table an objects data is stored in
        /// </summary>
        /// <param name="type">The Type of the object</param>
        /// <returns>The name of the data table</returns>
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

        /// <summary>
        /// Search for a message within the specified folder using its UID
        /// </summary>
        /// <param name="uid">The UID of the message to find</param>
        /// <param name="folderid">The ID of the folder that the message is in</param>
        /// <returns></returns>
        public IMessage GetMessageByUID(int uid, int folderid)
        {
            Mailbox.MessageDataTable mdt = _client.DataManager.MessageTable;
            Mailbox.MessageRow[] rows = (Mailbox.MessageRow[])mdt.Select(String.Format("UID = {0} AND FolderID = {1}", uid, folderid));

            if (rows.Length == 1)
                return new Message(_client, rows[0].ID);

            return null;
        }

        /// <summary>
        /// Retrieve a message from the mailbox with the specified internal ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMessage GetMessageByID(int id)
        {
            Mailbox.MessageDataTable mdt = _client.DataManager.MessageTable;
            Mailbox.MessageRow[] rows = (Mailbox.MessageRow[])mdt.Select(String.Format("ID = {0}", id));

            if (rows.Length == 1)
                return new Message(_client, rows[0].ID);

            return null;
        }

        /// <summary>
        /// Get a list of messages based on the specified folder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public IMessage[] GetMessagesByFolder(IFolder folder)
        {
            return _client.DataManager.GetMessagesByFolder(folder);
        }

        /// <summary>
        /// Get a list of messages based on the specified folder and in the specified direction
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public IMessage[] GetMessagesByFolder(IFolder folder, MessageListDirection direction)
        {
            return _client.DataManager.GetMessagesByFolder(folder, direction);
        }


        /// <summary>
        /// Find a contact based on the e-mail address
        /// </summary>
        /// <param name="email">The e-mail address to search for</param>
        /// <returns></returns>
        public IContact GetContactByEMail(string email)
        {
            return _client.DataManager.GetContactByEMail(email);
        }

        /// <summary>
        /// Returns an array of all the message content parts for the specified message
        /// </summary>
        /// <param name="messageID"></param>
        /// <returns></returns>
        public IMessageContent[] GetMessageContent(int messageID)
        {
            List<IMessageContent> content = new List<IMessageContent>();

            foreach (Mailbox.ContentRow row in _client.DataManager.GetContentRowsByMessageID(messageID))
            {
                content.Add(new MessageContent(_client, row.ID));
            }

            return content.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        /// <param name="localOnly">Indicates if the flag should be set only on the local object, and not on the server</param>
        public bool SetMessageFlag(IMessage msg, MessageFlag flag, bool value, bool localOnly)
        {
            bool currentValue = _client.DataManager.GetValue<Message, bool>((Message)msg, flag.ToString());
            if (currentValue == value) return true;
            _client.DataManager.SetValue(msg, flag.ToString(), value);
            if (localOnly) return true;
            ChangeFlagRequest cfr = new ChangeFlagRequest(msg, flag, value, null);
            _client.RequestManager.SubmitRequest(cfr, true);

            return cfr.Result.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        public bool SetMessageFlag(IMessage msg, string flag, bool value)
        {
            bool currentValue = msg.GetCustomFlag(flag);
            if (currentValue == value) return true;
            ChangeFlagRequest cfr = new ChangeFlagRequest(msg, flag, value, null);
            _client.RequestManager.SubmitAndWait(cfr, true);
            if (cfr.Result.Response != IMAPResponse.IMAP_SUCCESS_RESPONSE)
                _client.RequestManager.SubmitAndWait(cfr, true);    // retry once

            if (cfr.Result.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE)
                msg.SetCustomFlag(flag, value);

            return cfr.Result.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE;
        }

        /// <summary>
        /// Marks the specified message as \Seen on the server
        /// </summary>
        /// <param name="msg"></param>
        public bool MarkMessageAsRead(IMessage msg)
        {
            return SetMessageFlag(msg, MessageFlag.Seen, true, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="path"></param>
        public void SaveAttachment(IMessageContent content, string path)
        {
            string fn = content.ContentFilename;

            fn = Regex.Replace(fn, "[\\/:*?\"<>|]+", "_");

            string p = String.Format("{0}\\{1}", path.TrimEnd('\\'), fn);
            FileStream fstream = new FileStream(p, FileMode.Create);
            fstream.Write(content.BinaryData, 0, content.BinaryData.Length);
            fstream.Close();
        }


        #endregion
    }
}
