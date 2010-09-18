/********************************************************************************************
 * IMAPSearchResult.cs
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

namespace InterIMAP
{
    /// <summary>
    /// Container to hold all the information about a search including the query used and the results found
    /// </summary>
    public class IMAPSearchResult
    {
        #region Private Fields
        private IMAPSearchQuery _query;
        private IMAPFolder _folder;
        private IMAPMessageCollection _messages;
        #endregion

        #region Public Properties
        /// <summary>
        /// Query used to generate these results
        /// </summary>
        public IMAPSearchQuery Query
        {
            get { return _query; }
            set { _query = value; }
        }

        /// <summary>
        /// Folder the search was performed in
        /// </summary>
        public IMAPFolder Folder
        {
            get { return _folder; }
            set { _folder = value; }
        }

        /// <summary>
        /// Messages that were found based on query
        /// </summary>
        public IMAPMessageCollection Messages
        {
            get { return _messages; }
            set { _messages = value; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default Constructor
        /// </summary>
        public IMAPSearchResult()
        {
            _query = null;
            _folder = null;
            _messages = new IMAPMessageCollection();
        }
        #endregion
    }
}
