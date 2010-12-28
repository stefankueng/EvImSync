/********************************************************************************************
 * IMAPMailAddress.cs
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
    /// Simple container to store the address and display name of contacts used in a message
    /// </summary>
    [Serializable]
    public class IMAPMailAddress
    {
        #region Private Fields
        private string _address;        
        private string _displayName;        
        #endregion

        #region Public Properties
        /// <summary>
        /// The proper name of the owner of the e-mail address
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        /// <summary>
        /// The e-mail address
        /// </summary>
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default Constructor
        /// </summary>
        public IMAPMailAddress() 
        {
            _displayName = String.Empty;
            _address = String.Empty;
        }

        /// <summary>
        /// Construct a new IMAPMailAddress object specifying the display name and address
        /// </summary>
        /// <param name="display"></param>
        /// <param name="addr"></param>
        public IMAPMailAddress(string display, string addr)
        {
            _address = addr;
            _displayName = display;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return DisplayName == String.Empty ? Address : String.Format("{0} <{1}>", DisplayName, Address);
        }
        #endregion
    }
}
