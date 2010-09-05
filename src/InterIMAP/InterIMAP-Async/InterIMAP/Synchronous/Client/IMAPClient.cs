/********************************************************************************************
 * IMAPConfig.cs
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Collections;
using System.Threading;

namespace InterIMAP.Synchronous
{
    /// <summary>
    /// Main class for the client wrapper
    /// </summary>
    [Serializable]
    [Obsolete("The Synchronous code base is no longer supported.")]
    public class IMAPClient
    {
        #region Private Fields
        [NonSerialized] private IMAPConfig _config;        
        [XmlIgnore, NonSerialized] internal IMAP _imap;
        private IMAPFolderCollection _folders;
        internal int _messageCount;
        internal int _folderCount;
        private bool _usingCache;
        private bool _offlineMode;
        internal bool _updatingCache;
        private IMAPLogger _logger;                        
        #endregion

        #region Public Properties                
        /// <summary>
        /// IMAPLogger instance to use for this client instance
        /// </summary>
        [XmlIgnore]
        public IMAPLogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        /// <summary>
        /// Internal flag to tell the system that the object model is currently being serialized.
        /// </summary>
        [XmlIgnore]
        internal bool UpdatingCache
        {
            get { return _updatingCache; }
            set { _updatingCache = value; }
        }

        /// <summary>
        /// Flag to indicate whether the client is currently running in offline mode
        /// </summary>
        public bool OfflineMode
        {
            get { return _offlineMode; }
            set { _offlineMode = value; }
        }

        /// <summary>
        /// Flag to indicate whether the client is using a local cache
        /// </summary>
        /// 
        [XmlIgnore]
        public bool UsingCache
        {
            get { return _usingCache; }
            set { _usingCache = value; }
        }
        /// <summary>
        /// Total number of messages found in all folders
        /// </summary>
        /// 
        [XmlIgnore]
        public int TotalMessages
        {
            get 
            {
                _messageCount = 0;

                foreach (IMAPFolder f in _folders)
                    CountMessages(f, ref _messageCount);
                
                return _messageCount; 
            }
            set { _messageCount = value; }
        }

        /// <summary>
        /// Total number of folders
        /// </summary>
        [XmlIgnore]
        public int FolderCount
        {
            get
            {
                _folderCount = 0;

                foreach (IMAPFolder f in _folders)
                    CountFolders(f, ref _folderCount);

                return _folderCount;
            }

            set { _folderCount = value; }
        }

        

        /// <summary>
        /// The configuration object for this client instance
        /// </summary>
        [XmlIgnore]
        public IMAPConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// Returns true if the client is currently logged into the server
        /// </summary>
        [XmlIgnore]
        public bool LoggedOn
        {
            get
            {
                if (_imap != null)
                {
                    return _imap.IsLoggedIn;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Returns list of all folders underneath the default folder in the configuration class
        /// </summary>
        public IMAPFolderCollection Folders
        {
            get { return _folders; }
            set { _folders = value; }

        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default constructor. Only used for deserialization purposes.
        /// </summary>
        public IMAPClient()
        {
            _config = null;
            _imap = new IMAP();
            _folders = new IMAPFolderCollection();
            
            
        }

        /// <summary>
        /// Main constructor. 
        /// </summary>
        /// <param name="config">The configuration instance to use for this client</param>
        /// <param name="logger">Custom logger to use with this client. Use null for default logger.</param>
        public IMAPClient(IMAPConfig config, IMAPLogger logger, int maxWorkers)
        {
            
            
            _config = config;
            _imap = new IMAP();
            
            _logger = logger ?? new IMAPLogger(config);
            //_imap.InfoLogged += Log;
            _imap.Logger = _logger;
            _folders = new IMAPFolderCollection();
            
            Log(IMAPBase.LogTypeEnum.IMAP, "------------------------------------------------------");
            Log(IMAPBase.LogTypeEnum.INFO, "InterIMAP Client Initialized");

            if (config.CacheFile != String.Empty)
            {
                this.UsingCache = true;
                Log(IMAPBase.LogTypeEnum.INFO, String.Format("Using Local Cache File: {0}", config.CacheFile));
            }
            
            if (config.AutoLogon)
                Logon();

            if (UsingCache)
            {
                FileInfo finfo = new FileInfo(config.CacheFile);
                if (finfo.Exists)
                {
                    // this config has a cache file specified. Load the cache into the object model
                    
                    LoadCache();
                    if (!OfflineMode && config.AutoSyncCache)
                        SyncCache();
                }
                else
                {
                    
                    _folders.Clear();
                    _folders = _imap.ProcessFolders(_config.DefaultFolderName);
                    //IMAPFolderCollection tempFolders = _imap.ProcessFolders(_config.DefaultFolderName);
                    foreach (IMAPFolder f in _folders)
                    {
                        f.SetClient(this);
                        if (_config.AutoGetMsgID)
                            f.GetMessageIDs(false);
                    }

                    

                    
                    BuildNewCache();
                }
                
            }

            
        }        
        #endregion

        

        #region Logon/Logout Methods
        /// <summary>
        /// Login using currently specified connection settings. Called automatically if AutoLogon is true.
        /// </summary>
        public void Logon()
        {
            if (this.LoggedOn || this._config == null)
                return;

            

            if (!_imap.Login(_config.Host, _config.UserName, _config.Password, _config.UseSSL))
            {
                if (!UsingCache)
                {
                    Log(IMAPBase.LogTypeEnum.ERROR, "Cannot connect to server and no cache is available. System cannot proceed. Quiting...");
                    throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_CONNECT);
                }
                OfflineMode = true;
                Log(IMAPBase.LogTypeEnum.WARN, "Login Failed. Switching to Offline Mode");
                return;
            }

            Log(IMAPBase.LogTypeEnum.INFO, "Login Successful");

            if (!UsingCache)
            {
                XmlDocument doc = new XmlDocument();
                _folders.Clear();
                _folders = _imap.ProcessFolders(_config.DefaultFolderName);
                foreach (IMAPFolder f in _folders)
                {
                    f.SetClient(this);
                    if (_config.AutoGetMsgID)
                        f.GetMessageIDs(false);
                }
            }

        }

        /// <summary>
        /// Logoff from the server, if currently logged in
        /// </summary>
        public void Logoff()
        {
            if (this.LoggedOn)
                _imap.LogOut();
        }
        #endregion

        #region Folder Methods
        /// <summary>
        /// Returns a single folder that exactly maches the specified name
        /// </summary>
        /// <param name="name">The case-sensitive name of the folder to get</param>
        /// <returns></returns>
        public IMAPFolder GetSingleFolder(string name)
        {
            foreach (IMAPFolder f in _folders)
            {
                if (f.FolderName.Equals(name))
                    return f;
            }

            return null;
        }

        /// <summary>
        /// Iterates through the folder list and returns a list of matching folders. Can specify whether the name should match exactly or not.
        /// </summary>
        /// <param name="name">Name of folder to get</param>
        /// <param name="exact">Specify whether the name should match exactly</param>
        /// <returns></returns>
        public List<IMAPFolder> GetFoldersByName(string name, bool exact)
        {
            List<IMAPFolder> foundFolders = new List<IMAPFolder>();
            foreach (IMAPFolder f in _folders)
            {
                if (exact)
                {
                    if (f.FolderName.Equals(name))
                        foundFolders.Add(f);
                }
                else
                {
                    if (f.FolderName.Contains(name))
                        foundFolders.Add(f);
                }
                    
            }

            return foundFolders;
        }
        #endregion

        #region Cache Methods
        /// <summary>
        /// Serializes the current state of the object model to the cache file. 
        /// Should be called after modifying any messages or folders
        /// </summary>
        public void UpdateCache(bool blockDownloading)
        {
            if (!this.UsingCache)
                return;

            if (blockDownloading)
                UpdatingCache = true;
            else
                UpdatingCache = false;
            
            Stream s = File.Open(_config.CacheFile, FileMode.Create);
            switch (_config.Format)
            {
                case CacheFormat.XML:
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(IMAPClient));
                        xml.Serialize(s, this);
                        break;
                    }
                case CacheFormat.Binary:
                    {
                        BinaryFormatter b = new BinaryFormatter();
                        b.Serialize(s, this);
                        break;
                    }
            }

            s.Close();

            if (blockDownloading)
                UpdatingCache = false;
            else
                UpdatingCache = true;

        }

        /// <summary>
        /// Synchronizes the local cache with the server
        /// </summary>
        public void SyncCache()
        {
            if (this.OfflineMode)
                return;

            if (!this.UsingCache)
                return;

            // to synchronize the cache without having to download everything all over again,
            // we first get the folder list from the server. We then look at each folder in the server list
            // and see if it already exists in the client list. if it does not, we add it and pull
            // the message UIDs for it.

            // then we check the client list and see if all of those folders are still on the server
            // if not, the folder on the client side is removed, all with all of its messages

            // keep track of newly added folders so their contents can be downloaded.

            // next we iterate through all the existing folders and check for any new messages.
            // this is accomplished by simply calling the GetMessageIDs method on the folder. this will
            // update the folder with any UIDs that dont already exist. 
            // the messages content will be loaded automatically when it is serialized.             
            Log(IMAPBase.LogTypeEnum.INFO, "Synching Cache...");
            IMAPFolderCollection serverFolderList = _imap.RawFolderList;
            IMAPFolderCollection clientFolderList = FlattenFolderList(_folders);

            IMAPFolderCollection newFolderList = new IMAPFolderCollection();
            IMAPFolderCollection oldFolderList = new IMAPFolderCollection();


            foreach (IMAPFolder f in serverFolderList)
            {
                bool found = false;
                
                foreach (IMAPFolder cf in clientFolderList)
                {
                    if (cf.FolderPath.Equals(f.FolderPath))
                        found = true;
                }

                if (!found)
                {
                    newFolderList.Add(f);
                }
            }

            foreach (IMAPFolder f in clientFolderList)
            {
                bool found = false;

                foreach (IMAPFolder sf in serverFolderList)
                {
                    if (sf.FolderPath.Equals(f.FolderPath))
                        found = true;
                }

                if (!found)
                {
                    oldFolderList.Add(f);
                }

            }

            if (oldFolderList.Count > 0)
            {
                Log(IMAPBase.LogTypeEnum.INFO, String.Format("{0} old folders found", newFolderList.Count));
                foreach (IMAPFolder f in oldFolderList)
                {
                    IMAPFolder temp = null;
                    FindFolder(f.FolderPath, ref _folders, ref temp);

                    if (temp != null)
                    {
                        if (temp.ParentFolder == null)
                        {
                            _folders.Remove(temp);
                        }
                        else
                        {
                            temp.ParentFolder.SubFolders.Remove(temp);
                        }
                    }
                }
            }

            if (newFolderList.Count > 0)
            {
                Log(IMAPBase.LogTypeEnum.INFO, String.Format("{0} new folders found", newFolderList.Count));
                
                // now we need to put these new folders into the proper locations in the tree. 
                foreach (IMAPFolder f in newFolderList)
                {
                    f.GetMessageIDs(false);
                    foreach (IMAPFolder sf in serverFolderList)
                    {
                        if (sf.FolderName.Equals(f.ParentFolderName))
                        {
                            f.ParentFolder = sf;
                            break;
                        }
                    }
                    // if the new folder has no parent assigned to it then we just add it to the root folders
                    if (f.ParentFolderName == String.Empty)
                    {
                        _folders.Add(f);
                    }
                    else
                    {
                        
                        // otherwise we just loop through the flat list we created
                        // and find the folder that is the parent of the current new folder
                        // we then add the new folder to the sub folders of its parent
                        foreach (IMAPFolder cf in clientFolderList)
                        {
                            if (cf.FolderPath.Equals(f.ParentFolder.FolderPath))
                            {
                                cf.SubFolders.Add(f);
                                f.ParentFolder = cf;
                                break;
                            }
                        }
                    }
                }
            }

            foreach (IMAPFolder f in clientFolderList)
            {
                // this will get the UIDs of any new messages that have been added to the folder on the server
                f.GetMessageIDs(false);
            }

            UpdateCache(false);
            Log(IMAPBase.LogTypeEnum.INFO, "Cache Synchronization Complete");
        }

        /// <summary>
        /// Takes the multi-level folder structure and flattens it into a single list of all folders
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private IMAPFolderCollection FlattenFolderList(IMAPFolderCollection list)
        {
            IMAPFolderCollection newList = new IMAPFolderCollection();

            foreach (IMAPFolder f in list)
            {
                FolderRecurser(f, ref newList);
                
                newList.Add(f);
            }

            return newList;
        }

        /// <summary>
        /// Helper method for recursively adding all folders from the source folder structure into the flat list
        /// </summary>
        /// <param name="currentFolder"></param>
        /// <param name="master"></param>
        private void FolderRecurser(IMAPFolder currentFolder, ref IMAPFolderCollection master)
        {
            if (currentFolder.SubFolders.Count > 0)
            {
                foreach (IMAPFolder f in currentFolder.SubFolders)
                {
                    master.Add(f);
                    FolderRecurser(f, ref master);
                }
            }
        }

        /// <summary>
        /// Loads the specified local cache into the object model
        /// </summary>
        public void LoadCache() 
        {
            Log(IMAPBase.LogTypeEnum.INFO, "Loading Local Cache...");
            
            //try
            //{
                Stream s = File.Open(_config.CacheFile, FileMode.Open);
                IMAPClient c = new IMAPClient();
                switch (_config.Format)
                {
                    case CacheFormat.XML:
                        {
                            XmlSerializer xml = new XmlSerializer(typeof(IMAPClient));
                            c = (IMAPClient)xml.Deserialize(s);
                            break;
                        }
                    case CacheFormat.Binary:
                        {
                            BinaryFormatter b = new BinaryFormatter();
                            c = (IMAPClient)b.Deserialize(s);
                            break;
                        }
                }

                this._folders = c.Folders;
                foreach (IMAPFolder f in _folders)
                {
                    f.SetClient(this);
                    f.SetParent(null);
                }

                s.Close();
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

            Log(IMAPBase.LogTypeEnum.INFO, String.Format("Cache loaded ({0} messages found, in {1} folders)", this.TotalMessages, this.FolderCount));
        
        }

        /// <summary>
        /// Retrieves all message data from the server and serializes it to the specified cache file. Used when a new cache file is specified.
        /// </summary>        
        public void BuildNewCache()
        {
            if (_config.CacheFile == String.Empty)
            {
                throw new ArgumentNullException("CacheFile is not defined in current configuration.");
            }

            foreach (IMAPFolder f in _folders)
            {
                f.GetAllMessageData();
            }

            

            UpdatingCache = true;

            Stream s = File.Open(_config.CacheFile, FileMode.Create);
            switch (_config.Format)
            {
                case CacheFormat.Binary:
                    {
                        BinaryFormatter b = new BinaryFormatter();
                        b.Serialize(s, this);
                        break;
                    }
                case CacheFormat.XML:
                    {
                        XmlSerializer x = new XmlSerializer(typeof(IMAPClient));
                        x.Serialize(s, this);
                        break;
                    }
            }
            s.Close();

            UpdatingCache = false;
        }
        #endregion

        #region Folder Management Methods
        /// <summary>
        /// This method will delete all the messages in the specified folder. Use with caution.
        /// </summary>
        /// <param name="folderName">The name of the folder to empty</param>
        public void EmptyFolder(string folderName)
        {
            if (OfflineMode)
            {
                Log(IMAPBase.LogTypeEnum.WARN, "Cannot delete messages in offline mode.");
                return;
            }

            IMAPFolder foundFolder = null;

            foreach (IMAPFolder f in _folders)
            {
                if (f.FolderName.Equals(folderName))
                    foundFolder = f;
            }

            if (foundFolder == null)
            {
                Log(IMAPBase.LogTypeEnum.ERROR, String.Format("Folder \"{0}\" not found.", folderName));
                return;
            }

            string cmd = "STORE {0}:{1} +FLAGS (\\Deleted)\r\n";
            ArrayList result = new ArrayList();
            if (foundFolder._messages.Count == 0)
                return;
            int firstUID = foundFolder._messages[0].Uid;
            int lastUID = foundFolder._messages[foundFolder._messages.Count - 1].Uid;
            foundFolder.Select();
            _imap.SendAndReceive(String.Format(cmd, firstUID, lastUID), ref result);


            cmd = "EXPUNGE\r\n";
            _imap.SendAndReceive(cmd, ref result);
            foundFolder.Examine();
            foundFolder._messages.Clear();

            Log(IMAPBase.LogTypeEnum.INFO, String.Format("Folder {0} emptied successfully.", folderName));
        }

        /// <summary>
        /// Creates a new sub-folder under this folder
        /// </summary>
        /// <param name="name"></param>
        public void CreateFolder(string name)
        {
            if (OfflineMode)
            {
                Log(IMAPBase.LogTypeEnum.WARN, "Cannot create folders in offline mode.");
                return;
            }
            
            string cmd = "CREATE \"{0}\"\r\n";
            ArrayList result = new ArrayList();
            string newFolder = String.Format("{0}", name);
            _imap.SendAndReceive(String.Format(cmd, newFolder), ref result);
            if (result[0].ToString().Contains("OK"))
            {
                IMAPFolder oNewFolder = new IMAPFolder();
                oNewFolder.FolderName = name;
                oNewFolder.FolderPath = newFolder;
                oNewFolder.ParentFolder = null;
                oNewFolder.ParentFolderName = String.Empty;
                _folders.Add(oNewFolder);
                UpdateCache(true);
                Log(IMAPBase.LogTypeEnum.INFO, String.Format("Folder {0} created successfully.", name));
            }
            else
            {
                _imap.Log(IMAPBase.LogTypeEnum.ERROR, "Folder could not be created (" + result[0].ToString()+")");
            }

        }

        /// <summary>
        /// Deletes a sub-folder under this folder. All messages contained in this folder will be deleted.
        /// </summary>
        /// <param name="name">Name of folder to delete</param>
        public void DeleteFolder(string name)
        {
            if (OfflineMode)
            {
                Log(IMAPBase.LogTypeEnum.WARN, "Cannot delete folders in offline mode.");
                return;
            }
            
            string cmd = "DELETE \"{0}\"\r\n";

            IMAPFolder folderToDelete = null;

            foreach (IMAPFolder f in _folders)
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
                _imap.SendAndReceive(String.Format(cmd, folderToDelete.FolderPath), ref result);
                if (result[0].ToString().Contains("OK"))
                {
                    _folders.Remove(folderToDelete);
                    UpdateCache(true);
                    Log(IMAPBase.LogTypeEnum.INFO, String.Format("Folder {0} deleted successfully.", name));
                }
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Recursive helper method to count the total number of folders
        /// </summary>
        /// <param name="currentFolder"></param>
        /// <param name="count"></param>
        private void CountFolders(IMAPFolder currentFolder, ref int count)
        {
            count++;

            
            foreach (IMAPFolder f in currentFolder.SubFolders)
                CountFolders(f, ref count);

            
        }

        /// <summary>
        /// Recursive helper method to count the total number of messages in all folders
        /// </summary>
        /// <param name="currentFolder"></param>
        /// <param name="count"></param>
        private void CountMessages(IMAPFolder currentFolder, ref int count)
        {
            count += currentFolder._messages.Count;

            foreach (IMAPFolder f in currentFolder.SubFolders)
            {
                CountMessages(f, ref count);
            }

        }
        /// <summary>
        /// Shortcut method for logging information to the Console and optionally, a log file.        
        /// </summary>
        /// <param name="type">Type of message</param>
        /// <param name="msg">the message to display</param>
        public void Log(IMAPBase.LogTypeEnum type, string msg)
        {

            IMAPLogger.LogType t = IMAPLogger.LogType.General;

            switch (type)
            {
                case IMAPBase.LogTypeEnum.ERROR:
                    {
                        t = IMAPLogger.LogType.Error;
                        break;
                    }
                case IMAPBase.LogTypeEnum.IMAP:
                    {
                        t = IMAPLogger.LogType.General;
                        break;
                    }
                case IMAPBase.LogTypeEnum.INFO:
                    {
                        t = IMAPLogger.LogType.Info;
                        break;
                    }
                case IMAPBase.LogTypeEnum.WARN:
                    {
                        t = IMAPLogger.LogType.Warning;
                        break;
                    }
            }

            if (_logger != null)
                _logger.Log(IMAPLogger.LoggingSource.InterIMAP, t, msg);
                                    
        }

        /// <summary>
        /// Searches the entire tree structure of the provided folder collection looking for a folder with
        /// the specified path. if it finds the folder, it is stored in the foundFolder parameter
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sourceList"></param>
        /// <param name="foundFolder"></param>
        public void FindFolder(string path, ref IMAPFolderCollection sourceList, ref IMAPFolder foundFolder)
        {
            if (sourceList.Count == 0)
                return;
            
            foreach (IMAPFolder f in sourceList)
            {
                if (f.FolderPath.Equals(path))
                {
                    foundFolder = f;
                    return;
                }
                IMAPFolderCollection fc = f.SubFolders;
                FindFolder(path, ref fc, ref foundFolder);
            }
        }
        #endregion
    }

    /// <summary>
    /// Available formats for data serialization
    /// </summary>
    public enum CacheFormat
    {
        /// <summary>
        /// Serializes message data as human-readable XML
        /// </summary>
        XML,
        /// <summary>
        /// Serializes message data as machine-readable binary data
        /// </summary>
        Binary
    }
}
