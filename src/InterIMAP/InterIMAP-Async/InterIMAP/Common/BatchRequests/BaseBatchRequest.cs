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
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.BatchRequests
{
    /// <summary>
    /// Common functionality for batch requests
    /// </summary>
    public class BaseBatchRequest : IBatchRequest
    {
        #region Private Fields

        private readonly List<IRequest> _reqs;
        #endregion

        #region Progress Delegates
        /// <summary>
        /// Delegate signature for progress events
        /// </summary>
        /// <param name="req"></param>
        public delegate void BatchRequestProgressCallback(IBatchRequest req);
        #endregion

        #region Public Properties
        public List<IRequest> Requests
        {
            get { return _reqs; }
            
        }
        #endregion

        #region CTOR
        protected BaseBatchRequest()
        {
            _reqs = new List<IRequest>();
        }
        #endregion
    }
}
