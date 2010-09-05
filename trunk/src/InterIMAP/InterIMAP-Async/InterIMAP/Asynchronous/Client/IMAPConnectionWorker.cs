/*******************************************************************************************
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
using System.Threading;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Processors;
using InterIMAP.Common.Interfaces;
using InterIMAP.Asynchronous.Helpers;
using InterIMAP.Common;

namespace InterIMAP.Asynchronous.Client
{
    /// <summary>
    /// Thread object responsible for executing a request
    /// </summary>
    public class IMAPConnectionWorker
    {
        #region Private Fields

        private readonly IMAPAsyncClient _client;
        private IMAPConnection _conn;
        private IMAPConfig _config;
        private Thread _thread;
        private bool _shuttingDown;
        private bool _loggedIn;
        private bool _processingRequest;
        private bool _failed;
        private readonly int _workerID;
        private const int HEARTBEAT_INTERVAL = 120;
        private TimeSpan ts;
        private readonly WorkerLogger _logger;
        private int _connectionTries = 0;
        private int _completedRequests = 0;
        #endregion

        #region Public Properties
        /// <summary>
        /// The IMAPConnection object this worker will use
        /// </summary>
        public IMAPConnection Connection
        {
            get { return _conn; }
            set { _conn = value; }
        }

        /// <summary>
        /// The IMAPConfig this worker will use
        /// </summary>
        public IMAPConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// The ID of this worker thread
        /// </summary>
        public int WorkerID
        {
            get { return _workerID;  }
        }

        /// <summary>
        /// Flag indicating if this worker is connected
        /// </summary>
        public bool IsAlive
        {
            get { return (_thread.IsAlive && _conn.IsConnected && !_failed); }
        }

        /// <summary>
        /// Flag indicating if this worker failed to connect
        /// </summary>
        public bool Failed
        {
            get { return _failed; }
        }

        /// <summary>
        /// The WorkerLogger instance used by this worker
        /// </summary>
        public WorkerLogger Logger
        {
            get { return _logger; }
        }

        public int CompletedRequests { get { return _completedRequests; } }

        #endregion

        #region CTOR
        /// <summary>
        /// 
        /// </summary>        
        public IMAPConnectionWorker(IMAPAsyncClient client, int id)
        {
            _client = client;
            _workerID = id;
            _logger = new WorkerLogger(id);
            _config = _client.Config;
            _conn = new IMAPConnection(_config, _logger);
            _shuttingDown = false;
            _loggedIn = false;
            _processingRequest = false;
            
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Start this worker
        /// </summary>
        public void Start()
        {
            _thread = new Thread(DoWork);
            _thread.IsBackground = true;
            _thread.Name = String.Format("Worker{0}", _workerID);
            _logger.Start();
            _logger.Log(LogType.INFO, "{0} Starting...", _thread.Name);
            _thread.Start();
            _connectionTries = 0;
        }

        /// <summary>
        /// Stops this worker
        /// </summary>
        public void Stop()
        {
            if (_thread.IsAlive)
            {
                _shuttingDown = true;
            }
                
        }

        private void DoWork()
        {
            
            
            Login();
            ts = new TimeSpan(DateTime.Now.Ticks);
            while (true)
            {                
                if (!_conn.IsConnected)
                {
                    Login();
                }
                
                if (_shuttingDown)
                {
                    ShutDown();
                    break;
                }

                if (_client.RequestManager.BatchRequestPending)
                {
                    DoBatchRequest();
                }

                if (_client.RequestManager.RequestPending)
                {
                    IRequest req = _client.RequestManager.GetNextRequest();
                    if (req != null)
                        DoRequest(req);
                }



                //HeartBeat();
                Thread.Sleep(10);
            }
        }

        private void DoBatchRequest()
        {
            IBatchRequest req = _client.RequestManager.GetNextBatchRequest();
            if (req == null) return;
            foreach (IRequest r in req.Requests)
            {
                DoRequest(r);
            }
        }

        private void ShutDown()
        {
            if (_conn.IsConnected)
                _conn.Disconnect();

            _logger.Log(LogType.INFO, "{0} Stopping...", _thread.Name);
            _logger.Stop();
        }

        private void Login()
        {
            if (_connectionTries > 3)
            {
                _failed = true;
                return;
            }
            _conn.Connect();

            if (!_conn.IsConnected)
            {
                _failed = true;
                _connectionTries++;
                return;
            }

            LoginCommand lc = new LoginCommand(_config.UserName, _config.Password, null);
            CommandResult cr = _conn.ExecuteCommand(lc);            
            if (((LoginProcessor)BaseProcessor.RunProcessor(typeof(LoginProcessor), cr)).LoggedIn)
            {
                _loggedIn = true;
                _failed = false;
            }

            if (!_loggedIn)
            {
                _shuttingDown = true;
                _failed = true;
            }
        }

        private void DoRequest(IRequest req)
        {
            _processingRequest = true;
            
            if (req.PreCommand != null)
                _conn.ExecuteCommand(req.PreCommand);

            req.Result = _conn.ExecuteCommand(req.Command);            
            req.RunProcessor();

            if (req.PostCommand != null)
                _conn.ExecuteCommand(req.PostCommand);

            req.OnRequestCompleted();
            _client.RequestManager.RequestCompleted(req);
            _completedRequests++;
            _processingRequest = false;
            
        }

        private void HeartBeat()
        {
            if (_processingRequest) return;
            
            TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
            
            if (ts2.TotalSeconds < ts.TotalSeconds + HEARTBEAT_INTERVAL) return;
            _conn.ExecuteCommand(new HeartBeatCommand(null));
            //Console.WriteLine("Worker{0} idle", _workerID);
            _logger.Log(LogType.INFO, "Worker {0} idle", _workerID);
            ts = new TimeSpan(DateTime.Now.Ticks);
        }

        #endregion




    }
}
