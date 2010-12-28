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

namespace InterIMAP.Common.Attributes
{
    /// <summary>
    /// Specifies the header field inside the message header that corresponds to the attached property
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class HeaderName : Attribute
    {
        #region Private Fields

        private readonly string _headerName;
        #endregion

        #region Public Property
        /// <summary>
        /// Name of the header field this property is attached to
        /// </summary>
        public string Name { get { return _headerName; } }
        #endregion

        #region CTOR
        /// <summary>
        /// Attach this property to the specified header field
        /// </summary>
        /// <param name="name"></param>
        public HeaderName(string name)
        {
            _headerName = name;
        }
        #endregion
    }
}
