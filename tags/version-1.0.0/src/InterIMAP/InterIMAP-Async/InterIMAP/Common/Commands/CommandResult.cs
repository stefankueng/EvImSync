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
using System.Collections;

namespace InterIMAP.Common.Commands
{
    /// <summary>
    /// Stores the received data, and server response code from the
    /// ExecuteCommand method in IMAPConnection
    /// </summary>
    public class CommandResult
    {
        #region Private Fields
        private readonly ArrayList _resultData;
        private readonly IMAPResponse _resultResponse;
        private readonly TimeSpan _elapsedTime;
        #endregion

        #region Public Properties
        /// <summary>
        /// The text received from the command
        /// </summary>
        public ArrayList Results
        {
            get { return _resultData; }
        }

        /// <summary>
        /// The basic response received from the server
        /// </summary>
        public IMAPResponse Response
        {
            get { return _resultResponse; }
        }

        /// <summary>
        /// Th amount of time it took this command to complete
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Creates a new command result object with the results and response
        /// </summary>
        /// <param name="results"></param>
        /// <param name="response"></param>
        /// <param name="time">The amount of time it took for this command to execute</param>
        public CommandResult(ArrayList results, IMAPResponse response, TimeSpan time)
        {
            _resultData = results;
            _resultResponse = response;
            _elapsedTime = time;
        }
        #endregion
    }
}
