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
using InterIMAP.Asynchronous.Helpers;

namespace InterIMAP.Asynchronous.Client
{
    /// <summary>
    /// A class used to manage the pool of available server connections
    /// </summary>
    public class IMAPConnectionPool
    {
        #region Private Fields
        //private static IMAPConnectionPool _instance;
        private int _numConnections;
        private readonly List<IMAPConnectionWorker> _workers;
        private IMAPConfig _config;
        private readonly IMAPAsyncClient _client;
        private readonly List<WorkerLogger> _loggers;        
        #endregion

        #region CTOR
        /// <summary>
        /// Create a new connection pool for the specified client
        /// </summary>
        /// <param name="client"></param>
        public IMAPConnectionPool(IMAPAsyncClient client)
        {
            _numConnections = 5;
            _workers = new List<IMAPConnectionWorker>();
            _config = null;
            _client = client;
            _config = _client.Config;            
            _loggers = new List<WorkerLogger>();
        }
        #endregion

        #region Static Properties
        /// <summary>
        /// Set the maximum number of connections to maintain. Default is 5.
        /// </summary>
        public int MaxConnections
        {
            get { return _numConnections; }
            set { _numConnections = value; }
        }

        /// <summary>
        /// The IMAPConfig object to use for the connections in the pool
        /// </summary>
        public IMAPConfig Config
        {
            get { return _config; }   
            set { _config = value; }

        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Retreive the instance of this class
        /// </summary>
        /// <returns></returns>
        //public static IMAPConnectionPool GetInstance()
        //{
        //    if (_instance == null)
        //        _instance = new IMAPConnectionPool();
            
        //    return _instance;
        //}
        #endregion

        #region Public Properties
        /// <summary>
        /// Collection of loggers from each worker connection
        /// </summary>
        public WorkerLogger[] Loggers
        {
            get { return _loggers.ToArray(); }
        }
        #endregion

        #region Instance Methods

        /// <summary>
        /// Start all of the connection threads
        /// </summary>        
        public void StartUp()
        {
            StartUp(_numConnections);
        }
        /// <summary>
        /// Start all of the connection threads
        /// </summary>        
        /// <param name="maxConns">Maxiumum number of worker connections to use</param>
        public void StartUp(int maxConns)
        {            
            _numConnections = maxConns;
            
            if (_config == null)
                throw new Exception("No configuration object specified for connection pool");

            if(_numConnections < 1)
                throw new ArgumentOutOfRangeException("maxConns","Must specify at least one connection");

            //IMAPMailboxManager.InitializeMailbox();

            for (int i = 0; i < _numConnections; i++)
            {
                IMAPConnectionWorker worker = new IMAPConnectionWorker(_client, i);
                _loggers.Add(worker.Logger);
                worker.Logger.MessageLogged += Logger_MessageLogged;
                _workers.Add(worker);
                worker.Start();
            }

        }

        void Logger_MessageLogged(int workerId, string msg)
        {
            _client.Aggregator.AddMessage(workerId, msg);
        }

        /// <summary>
        /// Determines if there are any active, connected workers
        /// </summary>
        /// <returns></returns>
        public bool AnybodyAlive()
        {
            foreach (IMAPConnectionWorker worker in _workers)
                if (worker.IsAlive)
                    return true;

            return false;
        }
       
        /// <summary>
        /// Determines if all worker connections are active
        /// </summary>
        /// <returns></returns>
        public bool EveryoneAlive()
        {
            if (_workers.Count == 0) return false;
            
            foreach (IMAPConnectionWorker worker in _workers)
                if (!worker.IsAlive)
                    return false;

            return true;
        }

        /// <summary>
        /// Determines if any workers failed to connect
        /// </summary>
        /// <returns></returns>
        public bool AnyFailures()
        {
            foreach (IMAPConnectionWorker worker in _workers)
                if (worker.Failed)
                    return true;

            return false;
        }

        /// <summary>
        /// Aborts all IMAPConnectionWorkers
        /// </summary>
        public void Shutdown()
        {
            foreach (IMAPConnectionWorker worker in _workers)
                worker.Stop();
        }
        #endregion

        

    }
}
