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

using InterIMAP.Common.Commands;

namespace InterIMAP.Common.Interfaces
{
    /// <summary>
    /// Common elements for all IMAP command classes
    /// </summary>
    public interface ICommand
    {
        #region Properties
        /// <summary>
        /// The final command string to be sent to the server
        /// </summary>
        string CommandString { get; }

        /// <summary>
        /// List of parameters to pass into the command
        /// </summary>
        string[] Parameters { get; }

        /// <summary>
        /// Storage list for the objects that were given as parameters
        /// </summary>
        object[] ParameterObjects { get; }

        /// <summary>
        /// A unique number to identify this command
        /// </summary>
        int CommandNumber { get; set; }

        /// <summary>
        /// The ID for this command
        /// </summary>
        string CommandID { get; }

        /// <summary>
        /// The OK response string for this command
        /// </summary>
        string ResponseOK { get; }

        /// <summary>
        /// The NO response string for this command
        /// </summary>
        string ResponseNO { get; }

        /// <summary>
        /// The BAD response string for this command
        /// </summary>
        string ResponseBAD { get; }

        /// <summary>
        /// Event that is fired when data for this command is received
        /// </summary>
        event BaseCommand.CommandDataReceivedCallback CommandDataReceived;

        /// <summary>
        /// Internal method that is called when the data received event is fired
        /// </summary>
        /// <param name="receivedBytes"></param>
        /// <param name="totalBytes"></param>
        void OnCommandDataReceived(long receivedBytes, long totalBytes);
        #endregion

        


    }
}
