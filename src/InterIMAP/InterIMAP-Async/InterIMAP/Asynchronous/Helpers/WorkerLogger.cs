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
using InterIMAP.Common;

namespace InterIMAP.Asynchronous.Helpers
{
    /// <summary>
    /// Delegate signature of callback method used when a message is logged
    /// </summary>
    /// <param name="workerId"></param>
    /// <param name="msg"></param>
    public delegate void MessageLoggedCallback(int workerId, string msg);
    
    /// <summary>
    /// Logs activity of the connection workers
    /// </summary>
    public class WorkerLogger
    {
                
        #region Private Fields
        private readonly int _workerID;
        private bool _active;        
        private readonly List<string> _logBuffer;
        #endregion

        #region CTOR
        /// <summary>
        /// Creates a WorkerLogger with the specified id
        /// </summary>
        /// <param name="id"></param>
        public WorkerLogger(int id)
        {
            _workerID = id;            
            _active = false;
            _logBuffer = new List<string>();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Collection of all messages that have been logged
        /// </summary>
        public string[] Buffer
        {
            get { return _logBuffer.ToArray(); }
        }
        #endregion

        #region Public Events

        public event MessageLoggedCallback MessageLogged;
        #endregion

        #region Public Methods
        /// <summary>
        /// Opens the StreamWriter for use
        /// </summary>
        public void Start()
        {            
            _active = true;
        }

        /// <summary>
        /// Temporarily stop logging events
        /// </summary>
        public void Pause()
        {
            _active = false;
        }

        /// <summary>
        /// Close the logging Stream
        /// </summary>
        public void Stop()
        {            
            _active = false;
        }

        /// <summary>
        /// Log an event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        /// <param name="p"></param>
        public void Log(LogType type, string msg, params object[] p)
        {
            if (_active == false) return;
            
            string m = p.Length > 0 ? String.Format(msg, p) : msg;
            string line = String.Format("{0} - {1}: {2}", DateTime.Now, type, m);
            _logBuffer.Add(String.Format("[{0}] {1}", _workerID, line));
            if (MessageLogged != null)
                MessageLogged(_workerID, line);
            
            
        }
        #endregion
    }
}
