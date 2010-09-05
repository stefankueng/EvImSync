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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace InterIMAP
{
    /// <summary>
    /// A simple container to manage all the configuration options
    /// </summary>
    [Serializable]
    public class IMAPConfig
    {
        #region Debug Level Enum
        /// <summary>
        /// The different levels of debug output
        /// </summary>
        public enum DebugDetailLevel
        {
            /// <summary>
            /// Log all data
            /// </summary>
            All,
            /// <summary>
            /// Log only information from InterIMAP. No IMAP commands and responses.
            /// </summary>
            InterIMAPOnly,
            /// <summary>
            /// Logs only error messages
            /// </summary>
            ErrorsOnly
        }
        #endregion

        #region Private Fields
        private string _userName;        
        private string _password;        
        private string _host;        
        private bool _useSSL;        
        private string _defaultFolderName;
        private bool _autoLogon;        
        private bool _debugMode;
        private string _cacheFile;
        private bool _autoGetMsgID;
        private CacheFormat _cacheFormat;
        private bool _autoSyncCache;
        private string _logFile;
        private DebugDetailLevel _debugDetail;
        #endregion

        #region Public Properties
        /// <summary>
        /// Sets the amount of detail logged when in debug mode
        /// </summary>
        public DebugDetailLevel DebugDetail
        {
            get { return _debugDetail; }
            set { _debugDetail = value; }
        }

        /// <summary>
        /// Specifies the file that this configuration will use to log output when using debug mode
        /// </summary>
        public string LogFile
        {
            get { return _logFile; }
            set { _logFile = value; }
        }
        /// <summary>
        /// Flag to indicate whether the local cache should be synchronized with the server upon successful logon
        /// </summary>
        public bool AutoSyncCache
        {
            get { return _autoSyncCache; }
            set { _autoSyncCache = value; }
        }

        /// <summary>
        /// Format that the local cache should be stored in
        /// </summary>
        public CacheFormat Format
        {
            get { return _cacheFormat; }
            set { _cacheFormat = value; }
        }
        /// <summary>
        /// Decide whether to automatically retreive all the message UIDs for all folders
        /// </summary>
        public bool AutoGetMsgID
        {
            get { return _autoGetMsgID; }
            set { _autoGetMsgID = value; }
        }
        /// <summary>
        /// File where InterIMAP will store the local cache
        /// </summary>
        public string CacheFile
        {
            get { return _cacheFile; }
            set { _cacheFile = value; }
        }

        /// <summary>
        /// Set whether the library will output debug information to the console
        /// </summary>
        public bool DebugMode
        {
            get { return _debugMode; }
            set { _debugMode = value; }
        }
        
        /// <summary>
        /// Username to the mail account
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        /// <summary>
        /// Password to the mail account
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Host server to connect to
        /// </summary>
        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

        /// <summary>
        /// Set whether an SSL connection should be used
        /// </summary>
        public bool UseSSL
        {
            get { return _useSSL; }
            set { _useSSL = value; }
        }

        /// <summary>
        /// Default folder to select once connected
        /// </summary>
        public string DefaultFolderName
        {
            get { return _defaultFolderName; }
            set { _defaultFolderName = value; }
        }

        /// <summary>
        /// Sets whether the client should attempt to connect during the initialization of the client object
        /// </summary>
        public bool AutoLogon
        {
            get { return _autoLogon; }
            set { _autoLogon = value; }
        }
        #endregion

        #region CTORs
        /// <summary>
        /// Default Constructor
        /// </summary>
        public IMAPConfig()
        {
            this._host = String.Empty;
            this._userName = String.Empty;
            this._password = String.Empty;
            this._useSSL = false;
            this._autoLogon = false;
            this._defaultFolderName = String.Empty;            
            this._debugMode = false;
            this._cacheFile = String.Empty;
            this._autoGetMsgID = false;
            this._autoSyncCache = false;
            this._logFile = String.Empty;
            this._debugDetail = DebugDetailLevel.All;
        }

        /// <summary>
        /// Alternate Constructor. Not all options are represented here.
        /// </summary>
        /// <param name="host">The name of the server to connect to</param>
        /// <param name="username">The username of the account</param>
        /// <param name="passowrd">The password to the account</param>
        /// <param name="ssl">Set to True to connect using SSL</param>
        /// <param name="autoLogon">Set to True to have the system automatically logged in</param>
        /// <param name="folder">The Root folder that should be used. Leave blank to include all folders.</param>
        public IMAPConfig(string host, string username, string passowrd, bool ssl, bool autoLogon, string folder)
        {
            this._host = host;
            this._userName = username;
            this._password = passowrd;
            this._useSSL = ssl;
            this._defaultFolderName = folder;
            this._autoLogon = autoLogon;            
            this._debugMode = true;
            this._cacheFile = String.Empty;
            this._autoGetMsgID = true;
            this._autoSyncCache = false;
            this._logFile = String.Empty;
            this._debugDetail = DebugDetailLevel.All;
        }

        /// <summary>
        /// Creates a config object based on previously saved settings
        /// </summary>
        /// <param name="configFile"></param>
        public IMAPConfig(string configFile)
        {
            IMAPConfig c = new IMAPConfig();

            Stream s = File.Open(configFile, FileMode.Open);
            BinaryFormatter b = new BinaryFormatter();
            c = (IMAPConfig)b.Deserialize(s);
            this._host = c.Host;
            this._userName = c.UserName;
            this._password = c.Password;
            this._useSSL = c.UseSSL;
            this._defaultFolderName = c.DefaultFolderName;
            this._autoLogon = c.AutoLogon;
            this._debugMode = c.DebugMode;
            this._cacheFile = c.CacheFile;
            this._autoGetMsgID = c.AutoGetMsgID;
            this._autoSyncCache = c.AutoSyncCache;
            this._logFile = c.LogFile;
            this._debugDetail = c.DebugDetail;
            s.Close();
        }
        #endregion

        #region Save Settings Method
        /// <summary>
        /// Serializes current config settings to the specified config file in binary format.
        /// </summary>
        /// <param name="configFile"></param>
        public void SaveConfig(string configFile)
        {
            Stream s = File.Open(configFile, FileMode.OpenOrCreate);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(s, this);
            s.Close();
        }
        #endregion
    }
}
