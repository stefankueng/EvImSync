/********************************************************************************************
 * IMAP.cs
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
using System.Xml;
using System.IO;
using System.Data;
using System.Reflection;

namespace InterIMAP.Synchronous
{
    /// <summary>
    /// The low-level IMAP management class
    /// </summary>
    [Serializable]
    //[Obsolete("The Synchronous code base is no longer supported.", true)]
    public class IMAP : IMAPBase
    {
        #region Private Fields
        bool m_bIsLoggedIn = false;
        string m_sMailboxName = "INBOX";
        bool m_bIsFolderSelected = false;
        bool m_bIsFolderExamined = false;
        int m_nTotalMessages = 0;
        int m_nRecentMessages = 0;
        int m_nFirstUnSeenMsgUID = -1;
        //int m_currentConMsgCount = 0;
        internal string m_selectedFolder = String.Empty;
        internal string m_examinedFolder = String.Empty;
        private IMAPFolder _selectedFolder;
        private IMAPFolder _examinedFolder;
        private IMAPFolderCollection _rawFolderList = new IMAPFolderCollection();
        private IMAPConfig _config;
        #endregion

        #region Public Properties
        /// <summary>
        /// Write-only property for setting the config instance for use within the IMAP engine
        /// </summary>
        public IMAPConfig Config
        {            
            set { _config = value; }
        }

        /// <summary>
        /// Raw list of folders needed for cache synchronization
        /// </summary>
        public IMAPFolderCollection RawFolderList
        {
            get 
            {
                if (_config != null)
                {
                    ProcessFolders(_config.DefaultFolderName);
                }
                
                return _rawFolderList; 
            }
            set { _rawFolderList = value; }
        }

        /// <summary>
        /// Is the user currently logged in?
        /// </summary>
        public bool IsLoggedIn
        {
            get
            {
                return m_bIsLoggedIn;
            }
        }

        /// <summary>
        /// The currently selected folder
        /// </summary>
        public IMAPFolder SelectedFolder
        {
            get { return _selectedFolder; }
            set { SelectFolder(value); }
        }

        /// <summary>
        /// The currently examined folder
        /// </summary>
        public IMAPFolder ExaminedFolder
        {
            get { return _examinedFolder; }
            set { ExamineFolder(value); }
        }
        #endregion

        #region Logon/Logoff Methods
        /// <summary>
        /// Login to specified Imap host and default port (143)
        /// </summary>
        /// <param name="sHost">Imap Server name</param>
        /// <param name="sUserId">User's login id</param>
        /// <param name="sPassword">User's password</param>
        /// <param name="useSSL">Use a secure connection</param>
        public bool Login(string sHost, string sUserId, string sPassword, bool useSSL)
        {
            m_useSSL = useSSL;

            try
            {
                Login(sHost, useSSL ? IMAP_DEFAULT_SSL_PORT : IMAP_DEFAULT_PORT, sUserId, sPassword, useSSL);
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
            
        }

        /// <summary>
        /// Login to specified Imap host and port
        /// </summary>
        /// <param name="sHost">Imap server name</param>
        /// <param name="nPort">Imap server port</param>
        /// <param name="sUserId">User's login id</param>
        /// <param name="sPassword">User's password</param>        
        /// <param name="useSSL">Use secure connection</param>
        public void Login(string sHost, ushort nPort, string sUserId, string sPassword, bool useSSL)
        {
            m_useSSL = useSSL;

            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            IMAPException e_login = new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_LOGIN, m_sUserId);
            IMAPException e_invalidparam = new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_INVALIDPARAM);

            if (sHost.Length == 0)
            {
                Log(LogTypeEnum.ERROR, "Invalid m_sHost name");
                throw e_invalidparam;
            }

            if (sUserId.Length == 0)
            {
                Log(LogTypeEnum.ERROR, "Invalid m_sUserId");
                throw e_invalidparam;
            }

            if (sPassword.Length == 0)
            {
                Log(LogTypeEnum.ERROR, "Invalid Password");
                throw e_invalidparam;
            }            

            if (m_bIsConnected)
            {
                if (m_bIsLoggedIn)
                {
                    if (m_sHost == sHost && m_nPort == nPort)
                    {
                        if (m_sUserId == sUserId &&
                            m_sPassword == sPassword)
                        {
                            Log(LogTypeEnum.INFO, "Connected and Logged in already");
                            return;
                        }
                        else
                            LogOut();
                    }
                    else Disconnect();
                }
            }

            m_bIsConnected = false;
            m_bIsLoggedIn = false;

            try
            {
                eImapResponse = Connect(sHost, nPort, useSSL);
                if (eImapResponse == IMAPResponseEnum.IMAP_SUCCESS_RESPONSE)
                {
                    m_bIsConnected = true;
                }
                else return;
            }
            catch (Exception e)
            {
                throw e;
            }

            ArrayList asResultArray = new ArrayList();
            string sCommand = IMAP_LOGIN_COMMAND;

            

            sCommand += " " + sUserId + " " + sPassword;
            sCommand += IMAP_COMMAND_EOL;
            try
            {
                eImapResponse = SendAndReceive(sCommand, ref asResultArray);
                if (eImapResponse == IMAPResponseEnum.IMAP_SUCCESS_RESPONSE)
                {
                    m_bIsLoggedIn = true;
                    m_sUserId = sUserId;
                    m_sPassword = sPassword;
                }
                else throw e_login;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Logout the user: It logout the user and disconnect the connetion from IMAP server.
        /// </summary>
        public void LogOut()
        {
            if (!m_bIsLoggedIn) return;
            IMAPResponseEnum eImapResponse;
            ArrayList asResultArray = new ArrayList();
            string sCommand = IMAP_LOGOUT_COMMAND;
            sCommand += IMAP_COMMAND_EOL;
            try
            {
                eImapResponse = SendAndReceive(sCommand, ref asResultArray);
            }
            catch (Exception e)
            {
                Disconnect();
                m_bIsLoggedIn = false;
                throw e;
            }
            Disconnect();
            m_bIsLoggedIn = false;
            m_bIsFolderSelected = false;
            m_bIsFolderExamined = false;
        }
        #endregion

        #region Folder Methods
        /// <summary>
        /// Overload to take an IMAPFolder object instead of the folder name
        /// </summary>
        /// <param name="folder"></param>
        public void SelectFolder(IMAPFolder folder)
        {
            if (!folder.Selectable)
            {
                Log(LogTypeEnum.WARN, String.Format("Folder {0} is not selectable", folder.FolderPath));
                return;
            }
            
            SelectFolder(String.Format("\"{0}\"",folder.FolderPath));
            _selectedFolder = folder;
            _examinedFolder = null;
        }

        /// <summary>
        /// Overload to take an IMAPFolder object instead of the folder name
        /// </summary>
        /// <param name="folder"></param>
        public void ExamineFolder(IMAPFolder folder)
        {
            if (!folder.Selectable)
            {
                Log(LogTypeEnum.WARN, String.Format("Folder {0} is not selectable", folder.FolderPath));
                return;
            }
            
            ExamineFolder(String.Format("\"{0}\"",folder.FolderPath));
            _examinedFolder = folder;
            _selectedFolder = null;
        }

        /// <summary>
        /// Select the sFolder/mailbox after login
        /// </summary>
        /// <param name="sFolder">mailbox folder</param>
        private void SelectFolder(string sFolder)
        {
            if (!m_bIsLoggedIn)
            {
                try
                {
                    Restore(false, m_useSSL);
                }
                catch (IMAPException e)
                {
                    if (e.Type != IMAPException.IMAPErrorEnum.IMAP_ERR_INSUFFICIENT_DATA)
                        throw e;
                    throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_NOTCONNECTED, e.Message);
                }

            }
            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            IMAPException e_select = new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_SELECT, sFolder);
            IMAPException e_invalidparam = new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_INVALIDPARAM);
            if (sFolder.Length == 0)
            {
                throw e_invalidparam;
            }
            if (m_bIsFolderSelected)
            {
                if (m_sMailboxName == sFolder)
                {
                    Log(LogTypeEnum.INFO, "Folder is already selected");
                    return;
                }
                else m_bIsFolderSelected = false;
            }
            ArrayList asResultArray = new ArrayList();
            string sCommand = IMAP_SELECT_COMMAND;
            sCommand += " " + sFolder + IMAP_COMMAND_EOL;
            try
            {
                eImapResponse = SendAndReceive(sCommand, ref asResultArray);
                if (eImapResponse == IMAPResponseEnum.IMAP_SUCCESS_RESPONSE)
                {
                    m_sMailboxName = sFolder;
                    m_bIsFolderSelected = true;
                }
                else throw e_select;
            }
            catch (Exception e)
            {
                throw e;
            }

            //-------------------------
            // PARSE RESPONSE

            bool bResult = false;
            foreach (string sLine in asResultArray)
            {
                // If this is an unsolicited response starting with '*'
                if (sLine.IndexOf(IMAP_UNTAGGED_RESPONSE_PREFIX) != -1)
                {
                    // parse the line by space
                    string[] asTokens;
                    asTokens = sLine.Split(' ');
                    if (asTokens[2] == "EXISTS")
                    {
                        // The line will look like "* 2 EXISTS"
                        m_nTotalMessages = Convert.ToInt32(asTokens[1]);
                    }
                    else if (asTokens[2] == "RECENT")
                    {
                        // The line will look like "* 2 RECENT"
                        m_nRecentMessages = Convert.ToInt32(asTokens[1]);
                    }
                    else if (asTokens[2] == "[UNSEEN")
                    {
                        // The line will look like "* OK [UNSEEN 2]"
                        string sUIDPart = asTokens[3].Substring(0, asTokens[3].Length - 1);
                        m_nFirstUnSeenMsgUID = Convert.ToInt32(sUIDPart);
                    }
                }
                // If this line looks like "<command-tag> OK ..."
                else if (sLine.IndexOf(IMAP_SERVER_RESPONSE_OK) != -1)
                {
                    bResult = true;
                    break;
                }
            }

            if (!bResult)
                throw e_select;

            m_selectedFolder = sFolder;

            string sLogStr = "TotalMessages[" + m_nTotalMessages.ToString() + "] ,";
            sLogStr += "RecentMessages[" + m_nRecentMessages.ToString() + "] ,";
            if (m_nFirstUnSeenMsgUID > 0)
                sLogStr += "FirstUnSeenMsgUID[" + m_nFirstUnSeenMsgUID.ToString() + "] ,";
            //Log(LogTypeEnum.INFO, sLogStr);

        }
        

        

        /// <summary>
        /// Searches the currently selected folder for the message UIDs
        /// </summary>
        /// <returns></returns>
        public List<int> GetSelectedFolderMessageIDs(bool newOnly)
        {
            List<int> UIDs = new List<int>();
            string[] terms = new string[1];
            if (newOnly)
                terms[0] = "new";
            else
                terms[0] = "all";
            ArrayList result = new ArrayList();
            SearchMessage(terms, false, result);

            foreach (String id in result)
            {
                if (!String.IsNullOrEmpty(id) || !id.Equals("\"\""))
                {
                    try
                    {
                        UIDs.Add(Convert.ToInt32(id));
                    }
                    catch (Exception) { }
                }
            }

            return UIDs;
        }
       
        /// <summary>
        /// New method for processing folder list
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <returns></returns>
        public IMAPFolderCollection ProcessFolders(string rootFolder)
        {
            IMAPFolderCollection folders = new IMAPFolderCollection();
            List<IMAPFolder> subFolders = new List<IMAPFolder>();
            _rawFolderList.Clear();
            string cmd = "LIST {0} \"*\"\r\n";
            if (String.IsNullOrEmpty(rootFolder))
                rootFolder = "\"\"";

            ArrayList result = new ArrayList();
            SendAndReceive(String.Format(cmd, rootFolder), ref result);

            foreach (string s in result)
            {
                if (!s.StartsWith("*")) continue;
                int idx = s.IndexOf("\"");
                char dirDivider = s[idx + 1];
                int idx2 = s.IndexOf(" ", idx + 2);
                string folderString = s.Substring(idx2 + 1).Replace("\"","");
                string[] folderParts = folderString.Split(new char[] { dirDivider });
                StringBuilder folderPath = new StringBuilder();
                    
                for (int i = 0; i < folderParts.Length; i++)
                {
                    if (i > 0 && i < folderParts.Length)
                        folderPath.Append(dirDivider);
                    folderPath.Append(folderParts[i]);
                        

                    IMAPFolder f = new IMAPFolder();
                    f.FolderName = folderParts[i];
                    f.FolderPath = folderPath.ToString();
                    f.Selectable = !s.ToLower().Contains("\\noselect");
                    if (i > 0)
                    {
                        // if this is greater than 0 then we must have already processed its parent folder. find it
                        // in the list
                        foreach (IMAPFolder pf in folders)
                        {
                            if (pf.FolderName.Equals(folderParts[i - 1]))
                            {
                                f.ParentFolder = pf;

                                bool subAdded = false;
                                foreach (IMAPFolder testfolder in pf.SubFolders)
                                {
                                    if (testfolder.FolderPath.Equals(f.FolderPath))
                                        subAdded = true;
                                }

                                if (!subAdded)
                                {
                                    pf.SubFolders.Add(f);
                                    subFolders.Add(f);
                                }
                                //subFolders.Add(f);
                                break;
                            }

                        }
                    }

                    bool alreadyAdded = false;

                    foreach (IMAPFolder test in folders)
                    {
                        if (test.FolderPath.Equals(f.FolderPath))
                            alreadyAdded = true;
                    }
                        
                    if (!alreadyAdded)
                    {
                        folders.Add(f);
                        _rawFolderList.Add(f);
                    }
                        
                }
            }

            List<IMAPFolder> tmp1 = new List<IMAPFolder>();
            foreach (IMAPFolder f in folders)
                if (f.ParentFolder != null)
                    tmp1.Add(f);

            foreach (IMAPFolder f in tmp1)
                folders.Remove(f);

            return folders;
        }
                                                   
        /// <summary>
        /// Examine the sFolder/mailbox after login
        /// </summary>
        /// <param name="sFolder">Mailbox folder</param>
        public void ExamineFolder(string sFolder)
        {
            if (!m_bIsLoggedIn)
            {
                try
                {
                    Restore(false, m_useSSL);
                }
                catch (IMAPException e)
                {
                    if (e.Type != IMAPException.IMAPErrorEnum.IMAP_ERR_INSUFFICIENT_DATA)
                        throw e;

                    throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_NOTCONNECTED);
                }
            }
            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            IMAPException e_examine = new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_EXAMINE, sFolder);
            IMAPException e_invalidparam = new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_INVALIDPARAM);
            if (sFolder.Length == 0)
            {
                throw e_invalidparam;
            }
            if (m_bIsFolderExamined)
            {
                if (m_sMailboxName == sFolder)
                {
                    Log(LogTypeEnum.INFO, "Folder is already selected");
                    return;
                }
                else m_bIsFolderExamined = false;
            }
            ArrayList asResultArray = new ArrayList();
            string sCommand = IMAP_EXAMINE_COMMAND;
            sCommand += " " + sFolder + IMAP_COMMAND_EOL;
            try
            {
                eImapResponse = SendAndReceive(sCommand, ref asResultArray);
                if (eImapResponse == IMAPResponseEnum.IMAP_SUCCESS_RESPONSE)
                {
                    m_sMailboxName = sFolder;
                    m_bIsFolderExamined = true;
                }
                else throw e_examine;
            }
            catch (Exception e)
            {
                throw e;
            }
            //-------------------------
            // PARSE RESPONSE

            bool bResult = false;
            foreach (string sLine in asResultArray)
            {
                // If this is an unsolicited response starting with '*'
                if (sLine.IndexOf(IMAP_UNTAGGED_RESPONSE_PREFIX) != -1)
                {
                    // parse the line by space
                    string[] asTokens;
                    asTokens = sLine.Split(' ');
                    if (asTokens[2] == "EXISTS")
                    {
                        // The line will look like "* 2 EXISTS"
                        m_nTotalMessages = Convert.ToInt32(asTokens[1]);
                    }
                    else if (asTokens[2] == "RECENT")
                    {
                        // The line will look like "* 2 RECENT"
                        m_nRecentMessages = Convert.ToInt32(asTokens[1]);
                    }
                    else if (asTokens[2] == "[UNSEEN")
                    {
                        // The line will look like "* OK [UNSEEN 2]"
                        string sUIDPart = asTokens[3].Substring(0, asTokens[3].Length - 1);
                        m_nFirstUnSeenMsgUID = Convert.ToInt32(sUIDPart);
                    }
                }
                // If this line looks like "<command-tag> OK ..."
                else if (sLine.IndexOf(IMAP_SERVER_RESPONSE_OK) != -1)
                {
                    bResult = true;
                    break;
                }
            }

            if (!bResult)
                throw e_examine;

            m_examinedFolder = sFolder;

            string sLogStr = "TotalMessages[" + m_nTotalMessages.ToString() + "] ,";
            sLogStr += "RecentMessages[" + m_nRecentMessages.ToString() + "] ,";
            if (m_nFirstUnSeenMsgUID > 0)
                sLogStr += "FirstUnSeenMsgUID[" + m_nFirstUnSeenMsgUID.ToString() + "] ,";
            //Log(LogTypeEnum.INFO, sLogStr);

        }
        #endregion

        #region Restore Method
        /// <summary>
        /// Restore the connection using available old data
        /// Select the sFolder if previously selected
        /// </summary>
        /// <param name="bSelectFolder">If true then it selects the folder</param>
        /// <param name="useSSL">Set to true if using a secure connection</param>
        void Restore(bool bSelectFolder, bool useSSL)
        {
            IMAPException e_insufficiantdata = new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_INSUFFICIENT_DATA);
            if (m_sHost.Length == 0 ||
                m_sUserId.Length == 0 ||
                m_sPassword.Length == 0)
            {
                throw e_insufficiantdata;
            }
            try
            {
                m_bIsLoggedIn = false;
                Login(m_sHost, m_nPort, m_sUserId, m_sPassword, useSSL);
                if (bSelectFolder && m_sMailboxName.Length > 0)
                {
                    if (m_bIsFolderSelected)
                    {
                        m_bIsFolderSelected = false;
                        SelectFolder(m_sMailboxName);
                    }
                    else if (m_bIsFolderExamined)
                    {
                        m_bIsFolderExamined = false;
                        ExamineFolder(m_sMailboxName);
                    }
                    else SelectFolder(m_sMailboxName);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        #endregion

        #region Quota Methods
        /// <summary>
        /// Check if enough quota is available
        /// </summary>
        /// <param name="sFolderName">Mailbox folder</param>
        /// <returns>true if enough mail quota</returns>
        public bool HasEnoughQuota(string sFolderName)
        {
            try
            {
                bool bUnlimitedQuota = false;
                int nUsedKBytes = 0;
                int nTotalKBytes = 0;

                GetQuota(sFolderName, ref bUnlimitedQuota,
                    ref nUsedKBytes, ref nTotalKBytes);

                if (bUnlimitedQuota || (nUsedKBytes < nTotalKBytes))
                    return true;
                else
                    return false;
            }
            catch (IMAPException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get the quota for specific folder
        /// </summary>
        /// <param name="sFolderName">Mailbox folder</param>
        /// <param name="bUnlimitedQuota">Is unlimited quota</param>
        /// <param name="nUsedKBytes">Used quota in Kbytes</param>
        /// <param name="nTotalKBytes">Total quota in KBytes</param>
        public void GetQuota(string sFolderName, ref bool bUnlimitedQuota,
            ref int nUsedKBytes, ref int nTotalKBytes)
        {
            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            bool bResult = false;
            bUnlimitedQuota = false;
            nUsedKBytes = 0;
            nTotalKBytes = 0;
            if (!m_bIsLoggedIn)
            {
                try
                {
                    Restore(false, m_useSSL);
                }
                catch (IMAPException e)
                {
                    if (e.Type != IMAPException.IMAPErrorEnum.IMAP_ERR_INSUFFICIENT_DATA)
                        throw e;

                    throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_NOTCONNECTED);
                }
            }
            if (sFolderName.Length == 0)
            {
                throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_INVALIDPARAM);
            }

            ArrayList asResultArray = new ArrayList();
            string sCommand = IMAP_GETQUOTA_COMMAND;
            sCommand += " " + String.Format("\"{0}\"",sFolderName) + IMAP_COMMAND_EOL;
            try
            {
                eImapResponse = SendAndReceive(sCommand, ref asResultArray);
                if (eImapResponse == IMAPResponseEnum.IMAP_SUCCESS_RESPONSE)
                {
                    m_sMailboxName = sFolderName;
                    m_bIsFolderExamined = true;
                    string quotaPrefix = IMAP_UNTAGGED_RESPONSE_PREFIX + " ";
                    quotaPrefix += IMAP_QUOTA_RESPONSE + " ";
                    foreach (string sLine in asResultArray)
                    {
                        if (sLine.StartsWith(quotaPrefix) == true)
                        {
                            // Find the open and close paranthesis, and extract
                            // the part inside out.
                            int nStart = sLine.IndexOf('(');
                            int nEnd = sLine.IndexOf(')', nStart);
                            if (nStart != -1 &&
                                nEnd != -1 &&
                                nEnd > nStart)
                            {
                                string sQuota = sLine.Substring(nStart + 1, nEnd - nStart - 1);
                                if (sQuota.Length > 0)
                                {
                                    // Parse the space-delimited quota information which
                                    // will look like "STORAGE <used> <total>"
                                    string[] asArrList; // = new ArrayList();
                                    asArrList = sQuota.Split(' ');

                                    // get the used and total kbytes from these tokens
                                    if (asArrList.Length == 3 &&
                                        asArrList[0] == "STORAGE")
                                    {
                                        nUsedKBytes = Convert.ToInt32(asArrList[1], 10); ;
                                        nTotalKBytes = Convert.ToInt32(asArrList[2], 10);
                                    }
                                    else
                                    {
                                        string error = "Invalid Quota information :" + sQuota;
                                        Log(LogTypeEnum.ERROR, error);
                                        break;
                                    }
                                }
                                else
                                {
                                    bUnlimitedQuota = true;
                                }
                            }
                            else
                            {
                                string error = "Invalid Quota IMAP Response : " + sLine;
                                Log(LogTypeEnum.ERROR, error);
                                break;
                            }
                        }
                        // If the line looks like "<command-tag> OK ..."
                        else if (sLine.IndexOf(IMAP_SERVER_RESPONSE_OK) != -1)
                        {
                            bResult = true;
                            break;
                        }
                    }

                    if (!bResult)
                        throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_QUOTA);
                    if (bUnlimitedQuota)
                        Log(LogTypeEnum.INFO, "GETQUOTA quota=[unlimited].");
                    else
                    {
                        string sLogStr = "GETQUOTA used=[" + nUsedKBytes.ToString() +
                            "], total=[" + nTotalKBytes.ToString() + "]";
                        //Log(LogTypeEnum.INFO, sLogStr);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        #endregion

        #region Search Method
        /// <summary>
        /// Search the messages by specified criterias
        /// </summary>
        /// <param name="asSearchData">Search criterias</param>
        /// <param name="bExactMatch">Is it exact search</param>
        /// <param name="asSearchResult">search result</param>
        public void SearchMessage(string[] asSearchData, bool bExactMatch, ArrayList asSearchResult)
        {
            if (!m_bIsLoggedIn)
            {
                try
                {
                    Restore(true, m_useSSL);
                }
                catch (IMAPException e)
                {
                    if (e.Type != IMAPException.IMAPErrorEnum.IMAP_ERR_INSUFFICIENT_DATA)
                        throw e;

                    throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_NOTCONNECTED);
                }
            }
            if (!m_bIsFolderSelected && !m_bIsFolderExamined)
            {
                throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_NOTSELECTED);
            }
            int nCount = asSearchData.Length;
            if (nCount == 0)
            {
                throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_INVALIDPARAM);
            }
            //--------------------------
            // PREPARE SEARCH KEY/VALUE

            string sCommandSuffix = "";
            foreach (string sLine in asSearchData)
            {
                if (sLine.Length == 0)
                {
                    //throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_INVALIDPARAM); ;
                    continue;
                }

                // convert to lower case once for all
                sLine.ToLower();

                if (sCommandSuffix.Length > 0)
                    sCommandSuffix += " ";
                sCommandSuffix += sLine;
            }

            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            string sCommandString = IMAP_SEARCH_COMMAND + " " + sCommandSuffix;
            sCommandString += IMAP_COMMAND_EOL;
            ArrayList asResultArray = new ArrayList();
            try
            {
                //-----------------------
                // SEND SEARCH REQUEST
                eImapResponse = SendAndReceive(sCommandString, ref asResultArray);
                if (eImapResponse == IMAPResponseEnum.IMAP_SUCCESS_RESPONSE)
                {
                    //-------------------------
                    // PARSE RESPONSE
                    nCount = asResultArray.Count;
                    bool bResult = false;
                    string sPrefix = IMAP_UNTAGGED_RESPONSE_PREFIX + " ";
                    sPrefix += IMAP_SEARCH_RESPONSE;
                    foreach (string sLine in asResultArray)
                    {
                        int nPos = sLine.IndexOf(sPrefix);
                        if (nPos != -1)
                        {
                            nPos += sPrefix.Length;
                            string sSuffix = sLine.Substring(nPos);
                            sSuffix.Trim();
                            string[] asSearchRes = sSuffix.Split(' ');
                            foreach (string sResultLine in asSearchRes)
                            {
                                asSearchResult.Add(sResultLine);
                            }
                            bResult = true;
                            break;
                        }
                    }
                    if (!bResult)
                    {
                        throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_SEARCH, sCommandSuffix);
                    }
                }
                else
                    throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_SEARCH, asResultArray[0].ToString());
            }
            catch (IMAPException e)
            {
                //LogOut();
                throw e;
            }

        }
        #endregion

        #region Message Fetching and Parsing Methods
        /// <summary>
        /// Retreive the flags for the specified message
        /// </summary>
        /// <param name="msg"></param>
        public void ProcessMessageFlags(IMAPMessage msg)
        {
            string cmd = "UID FETCH {0} FLAGS\r\n";
            ArrayList result = new ArrayList();
            SendAndReceive(String.Format(cmd, msg.Uid), ref result);
            foreach (string s in result)
            {
                if (s.StartsWith("*"))
                {
                    if (s.ToLower().Contains(@"\seen"))
                    {
                        msg.Flags.New = false;

                    }
                    else if (s.ToLower().Contains(@"\unseen"))
                        msg.Flags.New = true;

                    if (s.ToLower().Contains(@"\answered"))
                        msg.Flags.Answered = true;
                    else
                        msg.Flags.Answered = false;

                    if (s.ToLower().Contains(@"\deleted"))
                        msg.Flags.Deleted = true;
                    else
                        msg.Flags.Deleted = false;

                    if (s.ToLower().Contains(@"\draft"))
                        msg.Flags.Draft = true;
                    else
                        msg.Flags.Draft = false;

                    if (s.ToLower().Contains(@"\recent"))
                        msg.Flags.Recent = true;
                    else
                        msg.Flags.Recent = false;
                }
            }
        }

        /// <summary>
        /// Marks the specified message as \Seen on the server
        /// </summary>
        /// <param name="msg"></param>
        public void MarkMessageAsRead(IMAPMessage msg)
        {
            string cmd = "UID STORE {0} +FLAGS (\\Seen)\r\n";
            ArrayList result = new ArrayList();
            SendAndReceive(String.Format(cmd, msg.Uid), ref result);
            if (result[0].ToString().ToLower().Contains("ok"))
                msg.Flags.New = false;
        }
              
        private List<IMAPMailAddress> ParseAddresses(string s)
        {
            List<IMAPMailAddress> addresses = new List<IMAPMailAddress>();

            // first we get each address as a seperate string
            string[] adrlist = s.Split(new string[] { ">," }, StringSplitOptions.None);

            // if there is more than one address, need to do something special
            if (adrlist.Length > 1)
            {
                foreach (string str in adrlist)
                    addresses.Add(GetAddressFromString(str));
            }
            else
            {
                addresses.Add(GetAddressFromString(s));
            }

            return addresses;

        }

        private IMAPMailAddress GetAddressFromString(string s)
        {
            IMAPMailAddress addr = new IMAPMailAddress();
            bool hasDisplayName = false;
            int idxAddr = 0;
            int idxAddrEnd = 0;

            s = s.Trim();
            idxAddr = s.IndexOf("<");
            idxAddrEnd = s.IndexOf(">");
            if (idxAddrEnd == -1)
                idxAddrEnd = s.Length - 1;
            // first we need to check if there is a display name or not
            if (idxAddr > 0)
            {
                // if the '<' that denotes the address is not in the first character then we know there is a display name
                hasDisplayName = true;
            }

            if (hasDisplayName)
            {
                string tempName = s;
                tempName = tempName.Remove(idxAddr);
                string tempAddr = s.Substring(idxAddr+1, idxAddrEnd - idxAddr);
                // tempName should now only contain the display name data
                // tempAddr should now only contain the address

                addr.DisplayName = tempName.Replace("\"", "").Trim();
                addr.Address = tempAddr.Replace("<", "").Replace(">", "").Trim();
            }
            else
            {
                string tempAddr = s;
                
                if (idxAddr > 0 && idxAddrEnd > 0)
                    tempAddr = s.Substring(idxAddr + 1, idxAddrEnd - idxAddr);

                addr.DisplayName = tempAddr.Replace("<","").Replace(">","").Trim();
                addr.Address = tempAddr.Replace("<","").Replace(">","").Trim();
            }

            addr.DisplayName = addr.DisplayName.Replace("\"", "");
            addr.Address = addr.Address.Replace("\"", "");

            return addr;
        }                                                                                                                                                    
        #endregion

        #region New Message Parsing Methods
        /// <summary>
        /// Retrieves the header information from the server and populates the IMAPMessage objects properties
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="partID"></param>
        /// <returns></returns>
        public bool ProcessMessageHeader(IMAPMessage msg, int partID)
        {
            Dictionary<string, string> headerData = new Dictionary<string, string>();
            string cmd = "UID FETCH {0} BODY[{1}]\r\n";

            cmd = String.Format(cmd, msg.Uid, partID > 0 ? partID + ".MIME" : "HEADER");

            ArrayList result = new ArrayList();
            ArrayList lineToProcess = new ArrayList();
            SendAndReceive(cmd, ref result);
            string temp = "";
            for (int i = 0; i < result.Count; i++)
            {
                // start of response, just skip it
                if (result[i].ToString().StartsWith("*") || result[i].ToString() == String.Empty || result[i].ToString().StartsWith(" ") || result[i].ToString().StartsWith("IMAP"))
                    continue;
                
                temp = result[i].ToString();

                // check each line after this line, looking for a tab or space. this indicates that those lines
                // are associated with the line in temp and should be appended. The loop ends when neither a tab or a space
                // is found, and the loop should not even be entered if one of those characters are not found.

                string currentLine = (i + 1 < result.Count - 1) ? result[i + 1].ToString() : "";

                while (currentLine.StartsWith("\t") || currentLine.StartsWith(" "))
                {
                    if (String.IsNullOrEmpty(currentLine))
                        break;

                    if (currentLine.StartsWith(" "))
                        temp += currentLine.TrimEnd();
                    else
                        temp += currentLine.Trim();

                    i++;
                    currentLine = (i + 1 < result.Count - 1) ? result[i + 1].ToString() : "";
                }
                                
                lineToProcess.Add(temp);
            }

            // now we process each data line into its name and value
            foreach (string line in lineToProcess)
            {
                int idx = line.IndexOf(":");
                if (idx == -1) continue;

                string name = line.Substring(0, idx).Replace("-","");
                int len = line.Length - idx;
                string value = line.Substring(idx + 1, line.Length - idx-1);
                // if a certain data item is already there then we just append the additional data.
                // this usually occurs with the Received: field
                if (headerData.ContainsKey(name))
                {
                    headerData[name] += value;
                }
                else
                {
                    headerData.Add(name, value);
                }
            }

            foreach (string name in headerData.Keys)
            {
                // process special cases first
                if (name.ToLower().Equals("to"))
                {
                    msg.SetPropValue(name,ParseAddresses(headerData[name].ToString()));
                    continue;
                }

                if (name.ToLower().Equals("from"))
                {
                    msg.SetPropValue(name, ParseAddresses(headerData[name].ToString()));
                    continue;
                }

                if (name.ToLower().Equals("cc"))
                {
                    msg.SetPropValue(name, ParseAddresses(headerData[name].ToString()));
                    continue;
                }

                if (name.ToLower().Equals("bcc"))
                {
                    msg.SetPropValue(name, ParseAddresses(headerData[name].ToString()));
                    continue;
                }

                if (name.ToLower().Equals("date"))
                {
                    string date = headerData[name].ToString();
                    
                    // special processing needed for gmail. the format they send the date in does not automatically convert                    
                    if (date.IndexOf(" (") > 0)
                    {
                        date = date.Substring(0, headerData[name].IndexOf("("));
                    }

                    DateTime dt = new DateTime();
                    DateTime.TryParse(date, out dt);
                    msg.SetPropValue(name, dt);
                    continue;
                }
                
                if (!msg.SetPropValue(name, headerData[name].ToString().Trim()))
                {
                    //if (!name.StartsWith("X"))
                    //    Log(LogTypeEnum.WARN, String.Format("IMAPMessage does not contain property for {0}", name));
                }
            }

            msg.HeaderLoaded = true;

            if (msg.ContentType == null)
                msg.ContentType = "text/plain";

            

            return true;
        }


        private bool BoundaryAllDashes(string[] boundary)
        {
            
            for (int i = 0; i < boundary.Length; i++)
            {
                for (int k = 0; k < boundary[i].Length; k++)
                {

                    if (boundary[i][k] != '-')
                        return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Downloads the entire body of the message from the server. includes content and attachments. 
        /// Structure is then parsed and the various data items are seperated and stored in the message object.
        /// </summary>
        /// <param name="msg"></param>
        public void ProcessBodyContent(IMAPMessage msg)
        {
            string cmd = "UID FETCH {0} BODY[]\r\n";            
            ArrayList result = new ArrayList();
            Log(LogTypeEnum.INFO, String.Format("Downloading body of message {0} in folder {1}", msg.Uid, msg.Folder.FolderName));
            // pausing the logger because otherwise we would be logging all of the binary data for attachments which slows
            // down the processing quite noticably. Can be unpaused for debugging purposes, but make sure to re-pause it.
            _logger.Paused = true;
            SendAndReceive(String.Format(cmd, msg.Uid), ref result);
            _logger.Paused = false;
            Log(LogTypeEnum.INFO, "Download complete");
            bool withinSection = false;

            string boundary = "";
            List<string> boundaryList = new List<string>();
            boundaryList.Add("");
            // first we need to extract the boundary string from the header
            // we use a collection because there might be other boundary demarcators in the content of the message and we
            // need to capture all of them
            if (msg.ContentType != null)
            {
                if (msg.ContentType.ToLower().Contains("boundary"))
                {
                    boundaryList.Clear();
                    boundary = "--";
                    string t = msg.ContentType;
                    int idx = t.ToLower().IndexOf("boundary");         
                    int idx2 = t.IndexOf("=", idx);
                    boundary += t.Substring(idx2 + 1).Replace("\"","").TrimStart();
                    int idx3 = boundary.IndexOf(";");
                    if (idx3 > -1)
                        boundary = boundary.Substring(0, idx3);
                    boundaryList.Add(boundary);
                    if (boundary.Length > 10)
                        boundary = boundary.Substring(0, 10);
                }
            }

            bool plainText = false;
            
            for (int i = 0; i < result.Count; i++)
            {
                string line = result[i].ToString();
                
                //if (line.ToLower().Contains("content-type"))
                //{
                //    if (!line.ToLower().Contains("multipart"))
                //        plainText = true;
                //}
                if (String.IsNullOrEmpty(msg.ContentType))
                    plainText = true;
                else if (msg.ContentType.ToLower().Contains("multipart"))
                    plainText = false;
                else
                    plainText = true;
                
                if (line.Contains("OK Success") || line.Contains("OK FETCH completed"))
                    break;

                if (!ListContainsString(boundaryList, line) && !withinSection)
                    continue;
                
                if (ListContainsString(boundaryList, line) && 
                    BoundaryAllDashes(boundaryList.ToArray()) ? true : 
                    !line.EndsWith("--"))
                    withinSection = true;


                
                if (withinSection)
                {
                    
                    
                    IMAPMessageContent content = new IMAPMessageContent();
                    
                    // first we process the section header data, stopping when we hit an empty line

                    if (!plainText)
                    {
                        while (!line.Equals(String.Empty))
                        {

                            line = result[++i].ToString();
                            if (line.ToLower().Contains("boundary"))
                            {
                                string newBoundary = "--";
                                string t = line;
                                int idx = t.ToLower().IndexOf("boundary");
                                int idx2 = t.IndexOf("=", idx);
                                newBoundary += t.Substring(idx2 + 1).Replace("\"", "");
                                boundaryList.Add(newBoundary);
                            }
                            string[] headerField = GetNameValue(line);
                            if (headerField.Length == 2)
                            {
                                PropertyInfo[] props = content.GetType().GetProperties();
                                foreach (PropertyInfo pinfo in props)
                                {
                                    if (pinfo.Name.ToLower().Equals(headerField[0].ToLower()))
                                    {
                                        pinfo.SetValue(content, headerField[1].Trim(), null);
                                        break;
                                    }
                                }
                            }


                            if (i + 2 < result.Count - 1)
                            {
                                if (result[i + 1].ToString().Equals("") && result[i + 2].ToString().Equals(""))
                                    break;
                            }
                            else
                            {
                                break;
                            }

                        }
                    }
                    else
                    {
                        content.ContentType = msg.ContentType;
                    }
                    
                    bool isGarbageSection = content.ContentType != null ? content.ContentType.ToLower().Contains("multipart") : false;

                    StringBuilder contentString = new StringBuilder();
                    bool hardEndOfMsg = false;
                    // now we are in the section body. continue until we hit the next section
                    while (true)
                    {
                        if (i + 1 > result.Count - 1)
                        {
                            hardEndOfMsg = true;
                            break;
                        }
                        line = result[++i].ToString();
                        if (boundary.Equals(""))
                        {
                            if (line.StartsWith(")") || line.StartsWith(String.Format(" UID {0})",msg.Uid)))
                            {
                                // lets make sure this is the last ')' in the message, just to be safe
                                int endOfMessage = 0;
                                for (int k = result.Count - 1; k > 0; k--)
                                {
                                    if (result[k].ToString().StartsWith(")") || result[k].ToString().StartsWith(String.Format(" UID {0})", msg.Uid)))
                                    {
                                        endOfMessage = k;
                                        break;
                                    }
                                }

                                if (i == endOfMessage)
                                    break;
                            }
                        }
                        else
                            if (ListContainsString(boundaryList, line)) { i--; break; }

                        contentString.AppendLine(line);    

                    }

                    if (!isGarbageSection && !hardEndOfMsg)
                    {
                        
                        
                        if (content.ContentTransferEncoding != null ? content.ContentTransferEncoding.ToLower().Equals("base64"): false)
                        {
                            content.BinaryData = Convert.FromBase64String(contentString.ToString());

                            if (content.ContentType.Contains("text/plain;") && content.ContentDisposition != "attachment;")
                            {
                                content.TextData = System.Text.Encoding.UTF8.GetString(content.BinaryData);
                            }
                        }
                        else
                        {
                            content.TextData = contentString.ToString();
                        }

                        msg._bodyParts.Add(content);
                        Log(LogTypeEnum.INFO, String.Format("Added content section of type {0}", content.ContentType));
                    }

                    withinSection = false;
                    
                }


            }

            if (msg._bodyParts.Count == 0)
            {
                Log(LogTypeEnum.ERROR, "No body parts added for this message");
            }
            else
            {
                foreach (IMAPMessageContent content in msg._bodyParts)
                {
                    if (String.IsNullOrEmpty(content.ContentType))
                        Log(LogTypeEnum.ERROR, "No content type found for this part");
                }
            }

        }

        /// <summary>
        /// Helper method to determine any of the strings in the souce contain the specified string
        /// </summary>
        /// <param name="source">Source list of strings</param>
        /// <param name="str">String to test with</param>
        /// <returns>true/false</returns>
        private bool ListContainsString(List<string> source, string str)
        {                        
            foreach (string s in source)
            {
                if (s.Equals(""))
                {
                    if (str.Equals(""))
                        return true;
                    
                } else 
                
                if (str.Contains(s))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Takes a string which contains something like Content-Type: Blah and returns Content-Type and Blah as sepearte
        /// string in an array.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string[] GetNameValue(string s)
        {
            
            int idx = s.IndexOf(":");
            if (idx == -1) return new string[] { "" };

            string name = s.Substring(0, idx).Replace("-", "");
            string value = s.Substring(idx + 1, s.Length - idx - 1);

            return new string[] { name, value };
        }

        /// <summary>
        /// Takes the IMAPMessageContent objects created in ProcessBodyStructure and downloads the content
        /// </summary>
        /// <param name="msg"></param>
        public void ProcessBodyParts(IMAPMessage msg)
        {
            List<IMAPMessageContent> processedContent = new List<IMAPMessageContent>();
            
            foreach (IMAPMessageContent content in msg._bodyParts)
            {
                if (content.ContentDisposition != null || content.ContentDescription != null)
                {
                    string disp = content.ContentDisposition == null ? "" : content.ContentDisposition;

                    if (disp.ToLower().Contains("inline") && (content.ContentId != null || content.ContentDescription !=null))
                    {
                        // this is an embedded image. 
                        IMAPFileAttachment image = new IMAPFileAttachment();
                        image.FileData = content.BinaryData;
                        image.FileEncoding = content.ContentTransferEncoding;
                        image.FileName = content.ContentId == null ? "" : content.ContentId.Replace("<", "").Replace(">", "");
                        if (content.ContentId == null && content.ContentDescription != null)
                            image.FileName = content.ContentDescription;
                        image.FileSize = content.BinaryData.Length;
                        image.FileType = content.ContentType.Substring(0, content.ContentType.IndexOf(";") > -1 ? content.ContentType.IndexOf(";") : content.ContentType.Length);
                        msg._embedded.Add(image);
                        processedContent.Add(content);
                    }
                    else if (content.ContentId != null || content.ContentDescription != null)
                    {
                        // this is an attached file
                        IMAPFileAttachment file = new IMAPFileAttachment();
                        file.FileData = content.BinaryData;
                        file.FileEncoding = content.ContentTransferEncoding;
                        file.FileName = content.ContentId== null ? "" : content.ContentId.Replace("<", "").Replace(">", "");
                        if (content.ContentId == null && content.ContentDescription != null)
                            file.FileName = content.ContentDescription;
                        file.FileSize = content.BinaryData.Length;
                        file.FileType = content.ContentType.Substring(0, content.ContentType.IndexOf(";") > -1 ? content.ContentType.IndexOf(";") : content.ContentType.Length);
                        msg._attachments.Add(file);
                        processedContent.Add(content);
                    }

                    
                }
            }

            foreach (IMAPMessageContent content in processedContent)
                msg._bodyParts.Remove(content);
            
            msg.ContentLoaded = true;
        }        
        #endregion
    }
}
