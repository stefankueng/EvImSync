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

using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Requests;

namespace InterIMAP.Common.Interfaces
{
    /// <summary>
    /// Public properties and methods for a Request
    /// </summary>
    public interface IRequest
    {
        #region Properties
        /// <summary>
        /// The command to execute for this request
        /// </summary>
        ICommand Command { get; set; }

        /// <summary>
        /// The results of the command
        /// </summary>
        CommandResult Result { get; set; }

        /// <summary>
        /// The ID of this request
        /// </summary>
        int RequestID { get; set; }

        /// <summary>
        /// The processor to use on the results
        /// </summary>
        IProcessor ProcessorResult { get; set; }

        /// <summary>
        /// The type of processor to run on this request
        /// </summary>
        System.Type ProcessorType { get; set; }

        /// <summary>
        /// Command to be executed before the main command.
        /// </summary>
        ICommand PreCommand { get; set; }

        /// <summary>
        /// Command to be executed after the main command.
        /// </summary>
        ICommand PostCommand { get; set; }

        /// <summary>
        /// A reference to the client
        /// </summary>
        IMAPAsyncClient Client { get; set; }

        /// <summary>
        /// The method that is executed when the request completes.
        /// </summary>
        BaseRequest.RequestCompletedCallback RequestCompleteCallback { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Called when a request is finished processing. Fires the Completed event handler
        /// </summary>
        void OnRequestCompleted();

        /// <summary>
        /// Returns a IProcessor object as the specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetProcessorAsType<T>() where T : IProcessor;

        /// <summary>
        /// Runs the processor on the results
        /// </summary>
        void RunProcessor();
        #endregion
    }
}