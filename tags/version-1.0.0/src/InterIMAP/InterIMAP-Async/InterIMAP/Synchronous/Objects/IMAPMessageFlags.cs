/********************************************************************************************
 * IMAPMessageFlags.cs
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

namespace InterIMAP.Synchronous
{
    /// <summary>
    /// Storage class for the various flags a message can have
    /// </summary>
    [Serializable]
    public class IMAPMessageFlags
    {
        #region Private Fields
        private bool _answered;
        private bool _daft;
        private bool _deleted;
        private bool _read;
        private bool _recent;
        #endregion

        #region Public Properties
        /// <summary>
        /// Indicates if this message has been replyed to
        /// </summary>
        public bool Answered
        {
            get { return _answered; }
            set { _answered = value; }
        }

        /// <summary>
        /// Indicates if this message is a draft
        /// </summary>
        public bool Draft
        {
            get { return _daft; }
            set { _daft = value; }
        }

        /// <summary>
        /// Indicates if this message has been marked for deletion during the next EXPUNGE
        /// </summary>
        public bool Deleted
        {
            get { return _deleted; }
            set { _deleted = value; }
        }

        /// <summary>
        /// Indicates if this is a new (unread) message
        /// </summary>
        public bool New
        {
            get { return _read; }
            set { _read = value; }
        }

        /// <summary>
        /// Indicates if this message was received recently
        /// </summary>
        public bool Recent
        {
            get { return _recent; }
            set { _recent = value; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default constructor
        /// </summary>
        public IMAPMessageFlags()
        {
            _answered = false;
            _daft = false;
            _deleted = false;
            _read = false;
            _recent = false;
        }
        #endregion
    }
}
