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
        private Synchronous.CacheFormat _cacheFormat;
        private bool _autoSyncCache;
        private string _logFile;
        private DebugDetailLevel _debugDetail;
        private string _configFile;
        #endregion

        #region Public Properties

        /// <summary>
        /// The file that the configuration is stored in
        /// </summary>
        public string ConfigFile
        {
            get { return _configFile; }
            set { _configFile = value; }
        }
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
        public Synchronous.CacheFormat Format
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
            _host = String.Empty;
            _userName = String.Empty;
            _password = String.Empty;
            _useSSL = false;
            _autoLogon = false;
            _defaultFolderName = String.Empty;            
            _debugMode = false;
            _cacheFile = String.Empty;
            _autoGetMsgID = false;
            _autoSyncCache = false;
            _logFile = String.Empty;
            _debugDetail = DebugDetailLevel.All;
            _configFile = null;
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
        public IMAPConfig(string host, string username, string password, bool ssl, bool autoLogon, string folder)
        {
            _host = host;
            _userName = username;
            _password = password;
            _useSSL = ssl;
            _defaultFolderName = folder;
            _autoLogon = autoLogon;            
            _debugMode = true;
            _cacheFile = String.Empty;
            _autoGetMsgID = true;
            _autoSyncCache = false;
            _logFile = String.Empty;
            _debugDetail = DebugDetailLevel.All;
        }

        /// <summary>
        /// Creates a config object based on previously saved settings
        /// </summary>
        /// <param name="configFile"></param>
        public IMAPConfig(string configFile)
        {
            _configFile = configFile;

            Stream s = File.Open(configFile, FileMode.Open);
            BinaryFormatter b = new BinaryFormatter();
            IMAPConfig c = (IMAPConfig)b.Deserialize(s);
            _host = c.Host;
            _userName = c.UserName;
            _password = c.Password;
            _useSSL = c.UseSSL;
            _defaultFolderName = c.DefaultFolderName;
            _autoLogon = c.AutoLogon;
            _debugMode = c.DebugMode;
            _cacheFile = c.CacheFile;
            _autoGetMsgID = c.AutoGetMsgID;
            _autoSyncCache = c.AutoSyncCache;
            _logFile = c.LogFile;
            _debugDetail = c.DebugDetail;
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

        /// <summary>
        /// Save the configuration to the already specified config file
        /// </summary>
        public void SaveConfig()
        {
            SaveConfig(_configFile);
        }
        #endregion
    }
}
