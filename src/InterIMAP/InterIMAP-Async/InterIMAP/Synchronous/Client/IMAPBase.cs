/********************************************************************************************
 * IMAPBase.cs
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
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Collections;

namespace InterIMAP.Synchronous
{
    /// <summary>
    /// Provides the basic connection management and server communication methods
    /// </summary>
    [Serializable]
    //[Obsolete("The Synchronous code base is no longer supported.")]
    public class IMAPBase
    {
        #region Protected Constants
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MSG_FLAG_SEEN = "seen";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MSG_FLAG_ANSWERED = "answered";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MSG_FLAG_FLAGGED = "flagged";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MSG_FLAG_DRAFT = "draft";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MSG_FLAG_DELETED = "deleted";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const ushort IMAP_MAX_MSG_FLAGS = 10;
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const ushort IMAP_DEFAULT_PORT = 143;
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const ushort IMAP_DEFAULT_SSL_PORT = 993;
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const ushort IMAP_DEFAULT_TIMEOUT = 30;
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected static ushort IMAP_COMMAND_VAL = 0;
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_COMMAND_PREFIX = "IMAP00";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_UNTAGGED_RESPONSE_PREFIX = "*";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_OK_RESPONSE = "OK";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_NO_RESPONSE = "NO";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_BAD_RESPONSE = "BAD";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_BAD_SERVER_RESPONSE = "* BAD";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_OK_SERVER_RESPONSE = "* OK";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_CAPABILITY_COMMAND = "CAPABILITY";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_CONNECT_COMMAND = "CONNECT";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_LOGIN_COMMAND = "LOGIN";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_LOGOUT_COMMAND = "LOGOUT";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_SELECT_COMMAND = "SELECT";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_EXAMINE_COMMAND = "EXAMINE";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_APPEND_COMMAND = "APPEND";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_QUOTA_RESPONSE = "QUOTA";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_GETQUOTA_COMMAND = "GETQUOTAROOT";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_LIST_COMMAND = "LIST";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_LIST_RESPONSE = "* LIST";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const char IMAP_APPEND_RESPONSE_START = '[';
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const char IMAP_APPEND_RESPONSE_END = ']';
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const char IMAP_GO_AHEAD_RESPONSE = '+';
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_SEARCH_COMMAND = "UID SEARCH";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_SEARCH_RESPONSE = "SEARCH";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_UIDFETCH_COMMAND = "UID FETCH";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_FETCH_COMMAND = "FETCH";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_BODYSTRUCTURE_COMMAND = "BODYSTRUCTURE";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_UIDSTORE_COMMAND = "UID STORE";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_EXPUNGE_COMMAND = "EXPUNGE";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_NOOP_COMMAND = "NOOP";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_COMMAND_EOL = "\r\n";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_NIL = "NIL";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_HEADER_EOL = "\r\n";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const char IMAP_MESSAGE_SIZE_START = '{';
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const char IMAP_MESSAGE_SIZE_END = '}';
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_CONTENT_TYPE = "content-type";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_RFC822 = "message/rfc822";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_ID = "message-id";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_MULTIPART = "multipart";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_CONTENT_ENCODING = "content-transfer-encoding";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_CONTENT_DESC = "content-description";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_CONTENT_DISP = "content-disposition";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_CONTENT_SIZE = "content-size";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_CONTENT_LINES = "content-lines";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MESSAGE_BASE64_ENCODING = "base64";/// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_MSG_DEFAULT_PART = "1";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_HEADER_SENDER_TAG = "sender";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_HEADER_FROM_TAG = "from";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_HEADER_IN_REPLY_TO_TAG = "in-reply-to";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_HEADER_REPLY_TO_TAG = "reply-to";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_HEADER_TO_TAG = "to";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_HEADER_CC_TAG = "cc";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_HEADER_BCC_TAG = "bcc";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_HEADER_SUBJECT_TAG = "subject";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_HEADER_DATE_TAG = "date";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_PLAIN_TEXT = "text/plain";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_AUDIO_WAV = "audio/wav";
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        protected const string IMAP_VIDEO_MPEG4 = "video/mpeg4";
        #endregion

        #region Public Properties
        /// <summary>
        /// Imap command identified which is combination of
        /// Imap identifier prefix and val
        /// eg. Prefix:IMAP00, Val: 1
        /// Imap command Identified= IMAP001
        /// </summary>
        protected string IMAP_COMMAND_IDENTIFIER
        {
            get
            {
                return IMAP_COMMAND_PREFIX + IMAP_COMMAND_VAL.ToString() + " ";
            }

        }

        /// <summary>
        /// Imap Server OK response which is combination of 
        /// Imap Identifier and Imap OK response.
        /// eg. IMAP001 OK
        /// </summary>
        protected string IMAP_SERVER_RESPONSE_OK
        {
            get
            {
                return IMAP_COMMAND_IDENTIFIER + IMAP_OK_RESPONSE;
            }
        }

        /// <summary>
        /// Imap Server NO response which is combination of 
        /// Imap Identifier and Imap NO response.
        /// eg. IMAP001 NO
        /// </summary>
        protected string IMAP_SERVER_RESPONSE_NO
        {
            get
            {
                return IMAP_COMMAND_IDENTIFIER + IMAP_NO_RESPONSE;
            }
        }

        /// <summary>
        /// Imap Server BAD response which is combination of
        /// Imap Identifier and Imap BAD response.
        /// eg. IMAP001 BAD
        /// </summary>
        protected string IMAP_SERVER_RESPONSE_BAD
        {
            get
            {
                return IMAP_COMMAND_IDENTIFIER + IMAP_BAD_RESPONSE;
            }
        }
        #endregion

        #region Private Fields
        /// <summary>
        /// Imap host
        /// </summary>
        protected string m_sHost = "";
        /// <summary>
        /// Imap port : default IMAP_DEFAULT_PORT : 143
        /// </summary>
        protected ushort m_nPort = IMAP_DEFAULT_PORT;
        /// <summary>
        /// User id
        /// </summary>
        protected string m_sUserId = "";
        /// <summary>
        /// User Password
        /// </summary>
        protected string m_sPassword = "";
        /// <summary>
        /// Is Imap server connected
        /// </summary>
        protected bool m_bIsConnected = false;
        /// <summary>
        /// Tcpclient object
        /// </summary>
        TcpClient m_oImapServ;
        /// <summary>
        /// Network stream object
        /// </summary>
        NetworkStream m_oNetStrm;
        /// <summary>
        /// StreamReader object
        /// </summary>
        StreamReader m_oRdStrm;
        /// <summary>
        /// SSL connection stream
        /// </summary>
        SslStream m_SSLStream;
        /// <summary>
        /// StreamReader for SSL conection
        /// </summary>
        StreamReader m_rdr;
        /// <summary>
        /// Global flag to tell the TCP functions to use the SSL stream or not
        /// </summary>
        protected bool m_useSSL = true;
        /// <summary>
        /// The logging object to use in this instance
        /// </summary>
        protected IMAPLogger _logger;
        #endregion

        #region Enums
        /// <summary>
        /// Imap server response result
        /// </summary>
        public enum IMAPResponseEnum
        {
            /// <summary>
            /// Imap Server responded "OK"
            /// </summary>
            IMAP_SUCCESS_RESPONSE,
            /// <summary>
            /// Imap Server responded "NO" or "BAD"
            /// </summary>
            IMAP_FAILURE_RESPONSE,
            /// <summary>
            /// Imap Server responded "*"
            /// </summary>
            IMAP_IGNORE_RESPONSE
        }
        /// <summary>
        /// Log type enum
        /// </summary>
        public enum LogTypeEnum
        {
            /// <summary>
            /// Information
            /// </summary>
            INFO,
            /// <summary>
            /// Warning
            /// </summary>
            WARN,
            /// <summary>
            /// Error
            /// </summary>
            ERROR,
            /// <summary>
            /// Imap Log information
            /// </summary>
            IMAP
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// The logging object to use
        /// </summary>
        public IMAPLogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }
        #endregion

        #region Helper Methods

        

        /// <summary>
        /// Logging function
        /// </summary>
        /// <param name="type">Log type;LogTypeEnum</param>
        /// <param name="log">Log data</param>
        public virtual void Log(LogTypeEnum type, string log)
        {
            //if (!IMAPBase.Debug)
            //    return;

            //imapLogActive = true;

            //if (InfoLogged != null)
            //    InfoLogged(type, log);
            IMAPLogger.LogType t = IMAPLogger.LogType.General;

            switch (type)
            {
                case LogTypeEnum.ERROR:
                    {
                        t = IMAPLogger.LogType.Error;
                        break;
                    }
                case LogTypeEnum.IMAP:
                    {
                        t = IMAPLogger.LogType.General;
                        break;
                    }
                case LogTypeEnum.INFO:
                    {
                        t = IMAPLogger.LogType.Info;
                        break;
                    }
                case LogTypeEnum.WARN:
                    {
                        t = IMAPLogger.LogType.Warning;
                        break;
                    }
            }

            if (_logger != null)
                _logger.Log(IMAPLogger.LoggingSource.IMAP, t, log);
            
            
            //Console.WriteLine(msg);
        }

        /// <summary>
        /// Callback for RemoteCertificateValidation object. Use for SSL connections.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            
            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        /// <summary>
        /// IMAP Capability command
        /// </summary>
        public void Capability()
        {
            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            ArrayList sResultArray = new ArrayList();
            string capabilityCommand = IMAP_CAPABILITY_COMMAND;
            capabilityCommand += IMAP_COMMAND_EOL;
            try
            {
                eImapResponse = SendAndReceive(capabilityCommand, ref sResultArray);
                if (eImapResponse != IMAPResponseEnum.IMAP_SUCCESS_RESPONSE)
                    throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_CAPABILITY);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Connection Management Methods
        /// <summary>
        /// Connect to specified host and port
        /// </summary>
        /// <param name="sHost">Imap host</param>
        /// <param name="nPort">Imap port</param>
        /// <param name="useSSL">Use a secure connection</param>
        /// <returns>ImapResponseEnum type</returns>
        protected IMAPResponseEnum Connect(string sHost, ushort nPort, bool useSSL)
        {


            IMAP_COMMAND_VAL = 0;
            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            IMAPException e_connect = new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_CONNECT, sHost);
            try
            {
                m_oImapServ = new TcpClient(sHost, nPort);
                m_oNetStrm = m_oImapServ.GetStream();
                m_oRdStrm = new StreamReader(m_oImapServ.GetStream());
                if (useSSL)
                {
                    m_SSLStream = new SslStream(m_oImapServ.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                    //m_SSLStream.ReadTimeout = 30000;
                    
                    try
                    {
                        m_SSLStream.AuthenticateAsClient(sHost);
                    }
                    catch (AuthenticationException e)
                    {
                        Console.WriteLine("Exception: {0}", e.Message);
                        if (e.InnerException != null)
                        {
                            Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                        }
                        Console.WriteLine("Authentication failed - closing the connection.");
                        m_oImapServ.Close();
                        return IMAPResponseEnum.IMAP_FAILURE_RESPONSE;
                    }

                    m_rdr = new StreamReader(m_SSLStream);
                }




                string sResult = useSSL ? m_rdr.ReadLine() : m_oRdStrm.ReadLine();
                if (sResult.StartsWith(IMAP_OK_SERVER_RESPONSE) == true)
                {
                    Log(LogTypeEnum.IMAP, sResult);
                    eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
                    Capability();
                }
                else
                {
                    Log(LogTypeEnum.IMAP, sResult);
                    eImapResponse = IMAPResponseEnum.IMAP_FAILURE_RESPONSE;
                }
            }
            catch
            {
                throw e_connect;
            }
            m_sHost = sHost;
            m_nPort = nPort;
            return eImapResponse;
        }


        /// <summary>
        /// Disconnect connection with Imap server
        /// </summary>
        protected void Disconnect()
        {
            IMAP_COMMAND_VAL = 0;
            if (m_bIsConnected)
            {
                if (m_oNetStrm != null)
                    m_oNetStrm.Close();
                if (m_oRdStrm != null)
                    m_oRdStrm.Close();
                if (m_SSLStream != null)
                    m_SSLStream.Close();

                Log(LogTypeEnum.INFO, "Disconnected");
            }

        }
        #endregion

        #region Send and Receive Methods
        public string ReadLine()
        {
            return m_useSSL ? m_rdr.ReadLine() : m_oRdStrm.ReadLine();

        }

        public void SendRaw(string data, bool asCommand)
        {
            if (asCommand)
            {
                IMAP_COMMAND_VAL++;
                data = IMAP_COMMAND_IDENTIFIER + data;
                Log(LogTypeEnum.INFO, data);
            }
            
            byte[] d = System.Text.Encoding.ASCII.GetBytes(data.ToCharArray());
            try
            {
                if (m_useSSL)
                    m_SSLStream.Write(d, 0, data.Length);
                else
                    m_oNetStrm.Write(d, 0, data.Length);
                bool bRead = true;
                while (bRead)
                {
                    string sResult = m_useSSL ? m_rdr.ReadLine() : m_oRdStrm.ReadLine();                                        
                    bRead = false;
                    Log(LogTypeEnum.IMAP, sResult);
                        
                   
                }
            }
            catch (Exception){}
        }

        /// <summary>
        /// Send command to server and retrieve response
        /// </summary>
        /// <param name="command">Command to send Imap Server</param>
        /// <param name="sResultArray">Imap Server response</param>
        /// <returns>ImapResponseEnum type</returns>
        public IMAPResponseEnum SendAndReceive(string command, ref ArrayList sResultArray)
        {
            IMAP_COMMAND_VAL++;
            string sCommand = IMAP_COMMAND_IDENTIFIER + command;
            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            Log(LogTypeEnum.IMAP, sCommand.TrimEnd(IMAP_COMMAND_EOL.ToCharArray()));
            byte[] data = System.Text.Encoding.ASCII.GetBytes(sCommand.ToCharArray());
            try
            {
                if (m_useSSL)
                    m_SSLStream.Write(data, 0, data.Length);
                else
                    m_oNetStrm.Write(data, 0, data.Length);
                bool bRead = true;
                while (bRead)
                {
                    string sResult = m_useSSL ? m_rdr.ReadLine() : m_oRdStrm.ReadLine();
                    sResultArray.Add(sResult);

                    if (sResult.StartsWith(IMAP_SERVER_RESPONSE_OK))
                    {
                        bRead = false;
                        Log(LogTypeEnum.IMAP, sResult);
                        eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
                    }
                    else if (sResult.StartsWith(IMAP_SERVER_RESPONSE_NO))
                    {
                        bRead = false;
                        Log(LogTypeEnum.IMAP, sResult);
                        eImapResponse = IMAPResponseEnum.IMAP_FAILURE_RESPONSE;
                    }
                    else if (sResult.StartsWith(IMAP_SERVER_RESPONSE_BAD))
                    {
                        bRead = false;
                        Log(LogTypeEnum.IMAP, sResult);
                        eImapResponse = IMAPResponseEnum.IMAP_FAILURE_RESPONSE;
                    }
                    else Log(LogTypeEnum.IMAP, sResult);
                }
            }
            catch (Exception e)
            {
                //LogOut();
                throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_SERIOUS, e.Message);

            }
            Log(LogTypeEnum.IMAP, "");
            return eImapResponse;
        }

        /// <summary>
        ///  retrieve response
        /// </summary>        
        /// <param name="sResultArray">Imap Server response</param>
        /// <returns>ImapResponseEnum type</returns>
        protected IMAPResponseEnum Receive(ref ArrayList sResultArray)
        {
            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            try
            {
                bool bRead = true;
                while (bRead)
                {

                    if (m_useSSL)
                    {
                        m_SSLStream.ReadTimeout = 15000;
                    }
                    else
                    {
                        m_oNetStrm.ReadTimeout = 15000;
                    }
                    string sResult = m_useSSL ? m_rdr.ReadLine() : m_oRdStrm.ReadLine();
                    if (m_useSSL)
                    {
                        m_SSLStream.ReadTimeout = 3000000;
                    }
                    else
                    {
                        m_oNetStrm.ReadTimeout = 3000000;
                    }
                    sResultArray.Add(sResult);

                    if (sResult.Contains(IMAP_SERVER_RESPONSE_OK))
                    {
                        bRead = false;
                        Log(LogTypeEnum.IMAP, sResult);
                        eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
                    }
                    else if (sResult.Contains(IMAP_SERVER_RESPONSE_NO))
                    {
                        bRead = false;
                        Log(LogTypeEnum.IMAP, sResult);
                        eImapResponse = IMAPResponseEnum.IMAP_FAILURE_RESPONSE;
                    }
                    else if (sResult.Contains(IMAP_SERVER_RESPONSE_BAD))
                    {
                        bRead = false;
                        Log(LogTypeEnum.IMAP, sResult);
                        eImapResponse = IMAPResponseEnum.IMAP_FAILURE_RESPONSE;
                    }
                    else Log(LogTypeEnum.IMAP, sResult);
                }
            }
            catch (Exception e)
            {
                //LogOut();
                throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_SERIOUS, e.Message);
                
            }
            Log(LogTypeEnum.IMAP, "");
            return eImapResponse;
        }

        /// <summary>
        /// Send command to server and retrieve response
        /// </summary>
        /// <param name="command">Command to send Imap Server</param>
        /// <param name="sResultArray">Imap Server response</param>
        /// <param name="nNumLines">Number of lines to receive</param>
        /// <returns>ImapResponseEnum type</returns>
        protected IMAPResponseEnum SendAndReceiveByNumLines(string command, ref ArrayList sResultArray, int nNumLines)
        {
            IMAP_COMMAND_VAL++;
            int nLineCount = 0;
            string sCommand = IMAP_COMMAND_IDENTIFIER + command;
            IMAPResponseEnum eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
            Log(LogTypeEnum.IMAP, sCommand.TrimEnd(IMAP_COMMAND_EOL.ToCharArray()));
            byte[] data = System.Text.Encoding.ASCII.GetBytes(sCommand.ToCharArray());
            try
            {
                if (m_useSSL)
                {
                    m_SSLStream.Write(data, 0, data.Length);
                    m_SSLStream.Flush();
                }
                else
                    m_oNetStrm.Write(data, 0, data.Length);
                bool bRead = true;
                while (bRead)
                {
                    string sResult = m_useSSL ? m_rdr.ReadLine() : m_oRdStrm.ReadLine();
                    sResultArray.Add(sResult);
                    nLineCount++;


                    if (sResult.StartsWith(IMAP_SERVER_RESPONSE_OK) ||
                        nLineCount == nNumLines)
                    {
                        bRead = false;
                        Log(LogTypeEnum.IMAP, sResult);
                        eImapResponse = IMAPResponseEnum.IMAP_SUCCESS_RESPONSE;
                    }
                    else if (sResult.StartsWith(IMAP_SERVER_RESPONSE_NO))
                    {
                        bRead = false;
                        Log(LogTypeEnum.IMAP, sResult);
                        eImapResponse = IMAPResponseEnum.IMAP_FAILURE_RESPONSE;
                    }
                    else if (sResult.StartsWith(IMAP_SERVER_RESPONSE_BAD))
                    {
                        bRead = false;
                        Log(LogTypeEnum.IMAP, sResult);
                        eImapResponse = IMAPResponseEnum.IMAP_FAILURE_RESPONSE;
                    }
                    else Log(LogTypeEnum.IMAP, sResult);
                }
            }
            catch (Exception e)
            {
                //LogOut();
                throw new IMAPException(IMAPException.IMAPErrorEnum.IMAP_ERR_SERIOUS, e.Message);

            }
            Log(LogTypeEnum.IMAP, "");
            return eImapResponse;
        }

        /// <summary>
        /// Read the Server Response by specified size
        /// </summary>
        /// <param name="sBuffer"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        protected int ReceiveBuffer(ref string sBuffer, int nSize)
        {
            int nRead = -1;
            char[] cBuff = new Char[nSize];
            
            // nRead will be the number of bytes the streamreader
            // actually inserts into the array before terminating

            


            // so if the bytes read is smaller than the bytes allocated
            // read again with an offset of the sum of nRead's and the
            // size of total bytes minus nRead.

            // keep looping and adding nRead as you go
            // eventually it will get to the end
            try
            {
                if (m_useSSL)
                {
                    nRead = m_rdr.Read(cBuff, 0, nSize);

                    m_SSLStream.ReadTimeout = 15000;
                    
                    while (nRead < nSize)
                    {
                        nRead += m_rdr.Read(cBuff, nRead, nSize - nRead);
                    }

                    m_SSLStream.ReadTimeout = 150000;

                }
                else
                {

                    nRead = m_oRdStrm.Read(cBuff, 0, nSize);

                    while (nRead < nSize)
                    {
                        nRead += m_oRdStrm.Read(cBuff, nRead, nSize - nRead);
                    }
                }
            }
            catch (IOException e)
            {
                Log(LogTypeEnum.ERROR, e.Message);
                
            }
                      
            string sTmp = new String(cBuff);

            sBuffer = sTmp;

            // i had to comment out the log function cause
            // it was giving me grief with this

            // Log (LogTypeEnum.IMAP, sBuffer);

            return nRead;
        }
        #endregion

        
    }
}
