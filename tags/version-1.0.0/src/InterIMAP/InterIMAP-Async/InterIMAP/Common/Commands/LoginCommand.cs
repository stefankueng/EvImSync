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

namespace InterIMAP.Common.Commands
{
    /// <summary>
    /// IMAP Command to Login to the server
    /// </summary>
    public class LoginCommand : BaseCommand
    {                        
        #region CTOR
        /// <summary>
        /// Generate a new LoginCommand
        /// </summary>
        /// <param name="username">The username of the account</param>
        /// <param name="password">The password of the account</param>
        /// <param name="callback"></param>
        public LoginCommand(string username, string password, CommandDataReceivedCallback callback) 
            : base(callback)
        {
            _parameters.Add(username);
            _parameters.Add(password);

            CommandString = String.Format("LOGIN \"{0}\" \"{1}\"", Parameters);
        }


        #endregion
        /// <summary>
        /// Validates that the username parameter is not empty, password is allowed to be empty.
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateParameters()
        {
            return !String.IsNullOrEmpty(Parameters[0]);
        }
    }
}
