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

namespace InterIMAP.Common
{
    /// <summary>
    /// enum for Imap exception errors
    /// </summary>
    public enum IMAPError
    {
        /// <summary>
        /// failure parsing the url
        /// </summary>
        IMAP_ERR_URI,
        /// <summary>
        /// invalid message uid in the url
        /// </summary>
        IMAP_ERR_MESSAGEUID,
        /// <summary>
        /// invalid username/password in the url
        /// </summary>
        IMAP_ERR_AUTHFAILED,
        /// <summary>
        /// failure connecting to imap server
        /// </summary>
        IMAP_ERR_CONNECT,
        /// <summary>
        /// not connected to any IMAP
        /// </summary>
        IMAP_ERR_NOTCONNECTED,
        /// <summary>
        /// failure logging into imap server
        /// </summary>
        IMAP_ERR_LOGIN,
        /// <summary>
        /// failure to logout from imap server
        /// </summary>
        IMAP_ERR_LOGOUT,
        /// <summary>
        /// not enough data to restore
        /// </summary>
        IMAP_ERR_INSUFFICIENT_DATA,
        /// <summary>
        /// timeout while waiting for response
        /// </summary>
        IMAP_ERR_TIMEOUT,
        /// <summary>
        /// socket error while receiving
        /// </summary>
        IMAP_ERR_SOCKET,
        /// <summary>
        /// failure getting the quota information
        /// </summary>
        IMAP_ERR_QUOTA,
        /// <summary>
        /// failure selecting a IMAP folder
        /// </summary>
        IMAP_ERR_SELECT,
        /// <summary>
        /// failure examining an IMAP folder
        /// </summary>
        IMAP_ERR_EXAMINE,
        /// <summary>
        ///  No folder is currently selected
        /// </summary>
        IMAP_ERR_NOTSELECTED,
        /// <summary>
        /// failure to search
        /// </summary>
        IMAP_ERR_SEARCH,
        /// <summary>
        /// failed to do exact match after search
        /// </summary>
        IMAP_ERR_SEARCH_EXACT,
        /// <summary>
        /// unsupported search key
        /// </summary>
        IMAP_ERR_INVALIDSEARCHKEY,
        /// <summary>
        /// failure to get message MIME
        /// </summary>
        IMAP_ERR_GETMIME,
        /// <summary>
        /// Message Header is in invalid format
        /// </summary>
        IMAP_ERR_INVALIDHEADER,
        /// <summary>
        /// Failed to fetch the bodystructure
        /// </summary>
        IMAP_ERR_FETCHBODYSTRUCT,
        /// <summary>
        /// failure to fetch a IMAP message
        /// </summary>
        IMAP_ERR_FETCHMSG,
        /// <summary>
        /// failure to allocate memory
        /// </summary>
        IMAP_ERR_MEMALLOC,
        /// <summary>
        /// failure to encode the audio content
        /// </summary>
        IMAP_ERR_ENCODINGERROR,
        /// <summary>
        /// failure to read/write the audio content
        /// </summary>
        IMAP_ERR_FILEIO,
        /// <summary>
        /// failure to store the message in IMAP
        /// </summary>
        IMAP_ERR_STOREMSG,
        /// <summary>
        /// failure to issue expunge command
        /// </summary>
        IMAP_ERR_EXPUNGE,
        /// <summary>
        /// invalid parameter to API
        /// </summary>
        IMAP_ERR_INVALIDPARAM,
        /// <summary>
        /// Capability command error
        /// </summary>
        IMAP_ERR_CAPABILITY,
        /// <summary>
        /// Serious Problem
        /// </summary>
        IMAP_ERR_SERIOUS

    }
}
