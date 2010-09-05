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
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using InterIMAP.Common;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Commands;
using System.Security.Authentication;
using InterIMAP.Asynchronous.Helpers;

namespace InterIMAP.Asynchronous.Client
{
    /// <summary>
    /// Provides all of the necessary plumbing to interact with an IMAP server
    /// </summary>
    public class IMAPConnection
    {        
        
        #region Private Fields
        private string _serverHost;
        private ushort _serverPort;
        private string _username;
        private string _password;
        private bool _isConnected;
        private int _commandCount;
        //private TcpClient _tcpClient;
        private NetworkStream _stdStream;
        private StreamReader _stdReader;
        private SslStream _sslStream;
        private StreamReader _sslReader;
        private bool _useSSL;
        private WorkerLogger _logger;
        private IMAPConfig _config;
        private Socket _socket;
        private NetworkStream _nstream;
        #endregion

        #region Constants
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        private const ushort IMAP_DEFAULT_PORT = 143;
        /// <summary>
        /// IMAP Command constant
        /// </summary>
        private const ushort IMAP_DEFAULT_SSL_PORT = 993;
                
        #endregion        

        #region Public Properties
        /// <summary>
        /// The logging object to use
        /// </summary>
        public WorkerLogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        /// <summary>
        /// The server hostname to connect to
        /// </summary>
        public string ServerHost
        {
            get { return _serverHost; }
            set { _serverHost = value; }
        }

        /// <summary>
        /// The port the server should connect to
        /// </summary>
        public ushort ServerPort
        {
            get { return _serverPort; }
            set { _serverPort = value; }
        }

        /// <summary>
        /// The Username of the account
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// The Password of the account
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Flag indicating whether this connection is active
        /// </summary>
        public bool IsConnected
        {
            get { return _socket != null ? _socket.Connected : false; }
            //set { _isConnected = value; }
        }

        /// <summary>
        /// Flag to indicate that this connection should use SSL
        /// </summary>
        public bool UseSSL
        {
            get { return _useSSL; }
            set { _useSSL = value; }
        }

        /// <summary>
        /// The IMAPConfig object to use for this connection
        /// </summary>
        public IMAPConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        #endregion

        #region CTOR
        /// <summary>
        /// Create new IMAPConnection object specifying the IMAPConfig to use
        /// </summary>
        public IMAPConnection(IMAPConfig config, WorkerLogger logger)
        {
            _config = config;
            _useSSL = _config.UseSSL;
            _serverHost = _config.Host;
            _serverPort = _useSSL ? IMAP_DEFAULT_SSL_PORT : IMAP_DEFAULT_PORT;
            _username = _config.UserName;
            _password = _config.Password;
            _logger = logger;            
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Logging function
        /// </summary>
        /// <param name="type">Log type;LogTypeEnum</param>
        /// <param name="log">Log data</param>
        public virtual void Log(LogType type, string log)
        {                        
            if (_logger != null)
                _logger.Log(type, log);            
        }

        /// <summary>
        /// Callback for RemoteCertificateValidation object. Use for SSL connections.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        private void Capability()
        {
            CapabilityCommand cc = new CapabilityCommand(null);
            
            CommandResult cr = ExecuteCommand(cc);

            foreach (string s in cr.Results)
                Log(LogType.INFO, s);
        }

        /// <summary>
        /// Executes the specified command on this connection
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public CommandResult ExecuteCommand(ICommand cmd)
        {            
            if (!IsConnected) return null;
            ArrayList resultArray = new ArrayList();
            PrepareCommand(cmd);

            IMAPResponse response = IMAPResponse.IMAP_SUCCESS_RESPONSE;
            byte[] data = GetStringBytes(cmd.CommandString);
            Stopwatch sw = new Stopwatch();
            try
            {
                WriteToStream(data, 0, data.Length);
                long bytesReceived = 0;
                long totalBytes = 0;
                
                while (true)
                {
                    string result = ReadLine();
                    if (result == null) break;
                    sw.Start();                    
                    resultArray.Add(result);
                                        
                    if (result.StartsWith(cmd.ResponseOK))
                    {                        
                        //Log(LogType.IMAP, result);
                        response = IMAPResponse.IMAP_SUCCESS_RESPONSE;
                        break;
                    }
                    
                    if (result.StartsWith(cmd.ResponseNO) || result.StartsWith(cmd.ResponseBAD))
                    {                        
                        //Log(LogType.IMAP, result);
                        response = IMAPResponse.IMAP_FAILURE_RESPONSE;
                        break;
                    }

                    if (resultArray.Count == 1)
                        totalBytes = DetermineTotalSize(result);
                    else
                    {
                        //if (cmd is MessagePartCommand)
                        //    bytesReceived += GetStringBytes(result, GetCurrentEncoding()).LongLength;
                        //else
                        bytesReceived += GetStringBytes(result).LongLength;
                    }

                    if (bytesReceived > totalBytes)
                        totalBytes = bytesReceived;

                    cmd.OnCommandDataReceived(bytesReceived, totalBytes);
                    if (cmd.GetType() != typeof(MessagePartCommand))
                        Log(LogType.IMAP, result);
                }
                sw.Stop();

                //if (cmd is MessagePartCommand)
                //    System.Diagnostics.Debugger.Break();

            }
            catch (Exception e)
            {                
                _logger.Log(LogType.IMAP, e.Message);
            }
            //Log(LogType.IMAP, "");
            return new CommandResult(resultArray, response, sw.Elapsed);
        }

        private long DetermineTotalSize(string firstLine)
        {
            Match match = Regex.Match(firstLine, "^\\*\\s.*\\{(?<size>(\\d+))\\}");
            if (match.Success)
                return Convert.ToInt64(match.Groups["size"].Value);

            return 0;
        }
        
        private void PrepareCommand(ICommand cmd)
        {
            cmd.CommandNumber = ++_commandCount;
        }

        private void WriteToStream(byte[] buffer, int offset, int count)
        {
            try
            {
                if (!IsConnected)
                    Connect();

                if (UseSSL)
                    _sslStream.Write(buffer, offset, count);
                else
                    _stdStream.Write(buffer, offset, count);
            }
            catch (Exception nfe)
            {
                _logger.Log(LogType.IMAP, nfe.Message);
            }
        }

        private byte[] GetStringBytes(string s)
        {
            return Encoding.ASCII.GetBytes(s.ToCharArray());
        }

/*
        private byte[] GetStringBytes(string s, Encoding enc)
        {
            return enc.GetBytes(s);
        }
*/
        #endregion

        #region Send and Receive Methods
        /// <summary>
        /// Reads a single line of text from either the SSL or Standard connection
        /// </summary>
        /// <returns></returns>
        private string ReadLine()
        {
            string line = "";
            try
            {
                line = UseSSL ? _sslReader.ReadLine() : _stdReader.ReadLine();
            }
            catch (IOException ioe)
            {
                    
            }
            return line;
        }
                
        /// <summary>
        /// Sends a raw string to the server, optionally in the format of a command
        /// </summary>
        /// <param name="data">The string to send to the server</param>
        /// <param name="asCommand">True to treat this string as a command</param>
        public void SendRaw(string data, bool asCommand)
        {
            if (asCommand)
            {
                CustomCommand cc = new CustomCommand(data, null);
                PrepareCommand(cc);
                data = cc.CommandString;
                Log(LogType.INFO, data);
            }

            byte[] b = GetStringBytes(data);
            try
            {
                WriteToStream(b, 0, data.Length);
                bool bRead = true;
                while (bRead)
                {
                    string result = ReadLine();
                    bRead = false;
                    Log(LogType.IMAP, result);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogType.IMAP, e.Message);
            }

        }

        
        #endregion

        #region Connection Management Methods
        public static void SetTcpKeepAlive(Socket socket, uint keepaliveTime, uint keepaliveInterval)
        {
            /* the native structure
            struct tcp_keepalive {
            ULONG onoff;
            ULONG keepalivetime;
            ULONG keepaliveinterval;
            };
            */

            // marshal the equivalent of the native structure into a byte array
            uint dummy = 0;
            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((uint)(keepaliveTime)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)keepaliveTime).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
            BitConverter.GetBytes((uint)keepaliveInterval).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);

            // write SIO_VALS to Socket IOControl
            socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }

        /// <summary>
        /// Establish a connection to the IMAP Server. Calls Capability() on success.
        /// </summary>
        /// <returns></returns>
        public IMAPResponse Connect()
        {
            IMAPResponse response = IMAPResponse.IMAP_SUCCESS_RESPONSE;

            _commandCount = 0;

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
                //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                //_socket.IOControl(IOControlCode.KeepAliveValues, )
                SetTcpKeepAlive(_socket, 1000,1000);
                _socket.SendBufferSize = int.MaxValue;
                _socket.ReceiveBufferSize = int.MaxValue;
                _socket.SendTimeout = int.MaxValue;
                _socket.ReceiveTimeout = int.MaxValue;
                _socket.Connect(ServerHost, ServerPort);
                _nstream = new NetworkStream(_socket, true);

                //_tcpClient = new TcpClient(ServerHost, ServerPort);
                if (UseSSL)
                {
                    //_sslStream = new SslStream(_tcpClient.GetStream(), false,
                    //    ValidateServerCertificate,
                    //    null);
                    _sslStream = new SslStream(_nstream, false,
                        ValidateServerCertificate,
                        null);

                    try
                    {
                        _sslStream.AuthenticateAsClient(ServerHost);
                    }
                    catch (AuthenticationException e)
                    {
                        Log(LogType.ERROR, e.Message);
                        //_tcpClient.Close();
                        _socket.Close();
                        _isConnected = false;
                        return IMAPResponse.IMAP_FAILURE_RESPONSE;
                    }

                    _sslReader = new StreamReader(_sslStream);

                }
                else
                {
                    _stdStream = _nstream;//_tcpClient.GetStream();
                    _stdReader = new StreamReader(_nstream);
                }

                string result = ReadLine();
                if (result.Contains("OK"))
                {
                    Log(LogType.IMAP, result);
                    _isConnected = true;
                    Capability();
                }
                else
                {
                    Log(LogType.IMAP, result);
                    response = IMAPResponse.IMAP_FAILURE_RESPONSE;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                Log(LogType.ERROR, e.Message);
                _isConnected = false;
            }

            return response;
        }

        /// <summary>
        /// Close this connection to the server
        /// </summary>
        public void Disconnect()
        {
            _commandCount = 0;
            if (IsConnected)
            {
                if (_stdStream != null)
                    _stdStream.Close();
                if (_stdReader != null)
                    _stdReader.Close();
                if (_sslStream != null)
                    _sslStream.Close();
                if (_sslReader != null)
                    _sslReader.Close();

                _isConnected = false;

                Log(LogType.INFO, "Disconnected");
            }
        }
        #endregion
    }
}
