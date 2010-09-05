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

using System.Collections.Generic;
using System.Data;

namespace InterIMAP.Asynchronous.Helpers
{
    /// <summary>
    /// A system that collects all the log information for each active worker
    /// </summary>
    public class LoggerAggregator
    {
        #region Private Fields        
        private readonly DataTable dt;
        private static readonly object _dataLock = new object();
        #endregion

        #region Public Properties
        /// <summary>
        /// Raw data table where log information is stored
        /// </summary>
        public DataTable Logs
        {
            get { return dt; }
        }

        /// <summary>
        /// Array of log messages currently stored
        /// </summary>
        public string[] LogEntries
        {
            get
            {
                List<string> entries = new List<string>();
                foreach (DataRow row in dt.Rows)
                    entries.Add(row["Message"].ToString());

                return entries.ToArray();
            }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Create a new aggregator for the specified client
        /// </summary>
        public LoggerAggregator()
        {
            
            dt = new DataTable("Logs");
            dt.Columns.Add(new DataColumn("WorkerID", typeof (int)));
            dt.Columns.Add(new DataColumn("Message", typeof (string)));
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a new message
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="message"></param>
        public void AddMessage(int worker, string message)
        {
            lock (_dataLock)
            {
                DataRow row = dt.NewRow();
                row["WorkerID"] = worker;
                row["Message"] = message;
                dt.Rows.Add(row);
                dt.AcceptChanges();
            }
        }

        /// <summary>
        /// Get all the messages for the specified worker
        /// </summary>
        /// <param name="workerid"></param>
        /// <returns></returns>
        public string[] GetWorkerMessages(int workerid)
        {
            List<string> msgs = new List<string>();
            DataRow[] rows = dt.Select("WorkerID = " + workerid);
            foreach (DataRow row in rows)
            {
                msgs.Add(row["Message"].ToString());
            }

            return msgs.ToArray();
            
        }

        /// <summary>
        /// Purge the aggregator of all logs
        /// </summary>
        public void ClearLogs()
        {
            dt.Rows.Clear();
            dt.AcceptChanges();
        }
        #endregion
    }
}
