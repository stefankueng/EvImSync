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
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Commands
{
    /// <summary>
    /// Updates a messages flag
    /// </summary>
    public class ChangeFlagCommand : BaseCommand
    {
        protected override bool ValidateParameters()
        {
            return true;
        }

        /// <summary>
        /// Create a new ChangeFlag command
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        /// <param name="callback"></param>
        public ChangeFlagCommand(IMessage msg, MessageFlag flag, bool value, CommandDataReceivedCallback callback)
            : base(callback)
        {
            //  UID STORE 4963 -FLAGS (\Seen)
            const string cmd = "UID STORE {0} {1}FLAGS (\\{2})";
            _parameters.Add(msg.UID.ToString());
            _parameters.Add(value ? "+":"-");
            _parameters.Add(flag.ToString());
            _parameterObjs.Add(msg);

            CommandString = String.Format(cmd, Parameters);

        }
    }
}
