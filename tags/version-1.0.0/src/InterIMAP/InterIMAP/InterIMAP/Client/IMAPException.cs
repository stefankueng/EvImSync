/********************************************************************************************
 * IMAPException.cs
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
    /// IMAP Exception class which implements IMAP releted exceptions
    /// </summary>
    public class IMAPException : ApplicationException
    {
        #region Private Fields
        /// <summary>
        /// Exception message string
        /// </summary>
        private string message;
        /// <summary>
        /// Error Type: ImapErrorEnum
        /// </summary>
        private IMAPErrorEnum errorType;
        #endregion

        #region Public Properties
        /// <summary>
        /// Property : Type (ImapErrorEnum)
        /// </summary>
        public IMAPErrorEnum Type
        {
            get
            {
                return errorType;
            }
            set
            {
                errorType = value;
            }
        }
        #endregion

        #region Error Enum
        /// <summary>
        /// enum for Imap exception errors
        /// </summary>
        public enum IMAPErrorEnum
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
        #endregion

        #region CTORS
        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">string</param>
		public IMAPException (String message) : base (message) 
		{
			this.message = message;
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">string</param>
		/// <param name="inner">Exception</param>
		public IMAPException (String message, Exception inner) : base(message,inner) 
		{
			this.message = message;
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Type">ImapErrorEnum</param>
		public IMAPException(IMAPErrorEnum Type) 
		{
			errorType = Type;
			message = GetDescription(Type);
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="Type">ImapErrorEnum</param>
		/// <param name="error">string</param>
		public IMAPException(IMAPErrorEnum Type, string error) 
		{
			errorType = Type;
			message = GetDescription(Type);
			message = message + " " + error;
		}
        #endregion

        #region Private Util Methods
        /// <summary>
        /// Get Description for specified Type
        /// </summary>
        /// <param name="Type">ImapErrorEnum type</param>
        /// <returns>string</returns>
        private string GetDescription(IMAPErrorEnum Type)
        {
            switch (Type)
            {
                case IMAPErrorEnum.IMAP_ERR_URI:
                    return "Failure parsing the IMAP URL.";
                case IMAPErrorEnum.IMAP_ERR_MESSAGEUID:
                    return "Invalid UserName/Password in the IMAP URL.";
                case IMAPErrorEnum.IMAP_ERR_CONNECT:
                    return "Failure connecting to the IMAP server.";
                case IMAPErrorEnum.IMAP_ERR_NOTCONNECTED:
                    return "Not connected to any IMAP server. Do Login first.";
                case IMAPErrorEnum.IMAP_ERR_LOGIN:
                    return "Failed authenticating the user/password in the IMAP server.";
                case IMAPErrorEnum.IMAP_ERR_INSUFFICIENT_DATA:
                    return "Not enough data to Restore the connection.";
                case IMAPErrorEnum.IMAP_ERR_LOGOUT:
                    return "Failed to Logout from IMAP server.";
                case IMAPErrorEnum.IMAP_ERR_TIMEOUT:
                    return "Timeout while waiting for response from the IMAP server.";
                case IMAPErrorEnum.IMAP_ERR_SOCKET:
                    return "Socket Error. Failed receiving message.";
                case IMAPErrorEnum.IMAP_ERR_QUOTA:
                    return "Failure getting the quota information for the folder/mailbox.";
                case IMAPErrorEnum.IMAP_ERR_SELECT:
                    return "Failure selecting IMAP folder/mailbox.";
                case IMAPErrorEnum.IMAP_ERR_NOTSELECTED:
                    return "No Folder is currently selected.";
                case IMAPErrorEnum.IMAP_ERR_EXAMINE:
                    return "Failure examining IMAP folder/mailbox.";
                case IMAPErrorEnum.IMAP_ERR_SEARCH:
                    return "Failure searching IMAP with the given criteria.";
                case IMAPErrorEnum.IMAP_ERR_SEARCH_EXACT:
                    return "Failure getting the exact search value.";
                case IMAPErrorEnum.IMAP_ERR_INVALIDSEARCHKEY:
                    return "Unsupported search key passed to SearchMessage API.";
                case IMAPErrorEnum.IMAP_ERR_GETMIME:
                    return "Failure fetching mime for the message.";
                case IMAPErrorEnum.IMAP_ERR_FETCHMSG:
                    return "Failure fetching message from IMAP folder/mailbox.";
                case IMAPErrorEnum.IMAP_ERR_MEMALLOC:
                    return "Failure allocating memory.";
                case IMAPErrorEnum.IMAP_ERR_ENCODINGERROR:
                    return "Failure encoding audio message.";
                case IMAPErrorEnum.IMAP_ERR_FILEIO:
                    return "Failure reading/writing the audio content to file.";
                case IMAPErrorEnum.IMAP_ERR_STOREMSG:
                    return "Failure storing message in IMAP.";
                case IMAPErrorEnum.IMAP_ERR_EXPUNGE:
                    return "Failure permanently deleting marked messages (EXPUNGE).";
                case IMAPErrorEnum.IMAP_ERR_INVALIDPARAM:
                    return "Invalid parameter passed to API. See Log for details";
                case IMAPErrorEnum.IMAP_ERR_CAPABILITY:
                    return "Failure capability command.";
                case IMAPErrorEnum.IMAP_ERR_SERIOUS:
                    return "Serious Problem with IMAP. Contact System Admin.";
                case IMAPErrorEnum.IMAP_ERR_FETCHBODYSTRUCT:
                    return "Failure bodystructure command";
                default:
                    throw new Exception("UnKnow Exception");

            }

        }
        #endregion
    }
}
