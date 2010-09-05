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

using InterIMAP.Asynchronous.Helpers;
using InterIMAP.Common.Data;

namespace InterIMAP.Asynchronous.Client
{
    /// <summary>
    /// The main entry point into the IMAP library
    /// </summary>
    public class IMAPAsyncClient
    {
        #region Private Fields
        private IMAPConfig _config;
        private readonly IMAPConnectionPool _connectionPool;
        private readonly IMAPMailboxManager _mailboxManager;
        private readonly IMAPRequestManager _requestManager;
        private readonly DataManager _dataManager;
        private int _numConnections;
        private readonly LoggerAggregator _aggregator;
        #endregion

        #region Public Properties
        /// <summary>
        /// The current configuration being used by this client
        /// </summary>
        public IMAPConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        /// <summary>
        /// This clients pool of connections
        /// </summary>
        public IMAPConnectionPool ConnectionPool
        {
            get { return _connectionPool; }
        }

        /// <summary>
        /// This clients Mailbox
        /// </summary>
        public IMAPMailboxManager MailboxManager
        {
            get { return _mailboxManager; }
        }

        /// <summary>
        /// This clients Request system
        /// </summary>
        public IMAPRequestManager RequestManager
        {
            get { return _requestManager; }
        }

        /// <summary>
        /// This clients Data Manager
        /// </summary>
        public DataManager DataManager
        {
            get { return _dataManager; }
        }

        /// <summary>
        /// This clients log aggregator
        /// </summary>
        public LoggerAggregator Aggregator
        {
            get { return _aggregator; }
        }

        /// <summary>
        /// Flag indicating if any worker connections are active
        /// </summary>
        public bool IsAlive
        {
            get { return _connectionPool.AnybodyAlive(); }
        }

        /// <summary>
        /// Flag indicating if all worker connections are active and no errors were reported
        /// </summary>
        public bool ReadyToGo
        {
            get { return _connectionPool.EveryoneAlive() && !_connectionPool.AnyFailures(); }
        }

        /// <summary>
        /// Flag indicating if there were any errors reported during startup
        /// </summary>
        public bool WeHaveAProblem
        {
            get { return _connectionPool.AnyFailures(); }
        }

        /// <summary>
        /// Get/Set number of worker threads being used. Must Stop/Start for changes to take effect.
        /// </summary>
        public int NumberOfWorkers
        {
            get { return _numConnections; }
            set { _numConnections = value; }
        }
    
        #endregion

        #region CTOR
        /// <summary>
        /// Create a new IMAPAsyncClient using the specified configuration and number of worker connections
        /// </summary>
        /// <param name="config"></param>
        /// <param name="numberWorkers"></param>
        public IMAPAsyncClient(IMAPConfig config, int numberWorkers)
        {
            _config = config;
            _connectionPool = new IMAPConnectionPool(this);
            _numConnections = numberWorkers;
            _dataManager = new DataManager(this);
            _mailboxManager = new IMAPMailboxManager(this);
            _requestManager = new IMAPRequestManager(this);
            _aggregator = new LoggerAggregator();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts up the worker connections. Blocks calling thread until either all workers are ready, 
        /// or an error was detected
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            _connectionPool.StartUp(_numConnections);
            while (true)
            {
                if (ReadyToGo) return true;

                if (WeHaveAProblem) return false;
            }
        }

        /// <summary>
        /// Close any active worker connections
        /// </summary>
        public void Stop()
        {
            _connectionPool.Shutdown();
        }
        #endregion
    }
}
