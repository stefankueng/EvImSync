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
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Commands;

namespace InterIMAP.Common.Requests
{
    /// <summary>
    /// Defines common values and functionality for a Request
    /// </summary>
    public class BaseRequest : IRequest
    {
        /// <summary>
        /// The delegate that is to be used for providing request complete callback
        /// </summary>
        /// <param name="req"></param>
        public delegate void RequestCompletedCallback(IRequest req);
        
        #region Private Fields

        private ICommand _cmd;
        private CommandResult _result;
        private int _id;
        private IProcessor _processor;
        private Type _procType;
        private ICommand _preCmd;
        private ICommand _postCmd;
        private IMAPAsyncClient _client;
        #endregion

        public event RequestCompletedCallback RequestCompleted;


        #region IRequest Members
        public RequestCompletedCallback RequestCompleteCallback
        {
            get { return RequestCompleted; }
            set { RequestCompleted = value; }
        }

        public IMAPAsyncClient Client
        {
            get { return _client; }
            set { _client = value; }
        }
        
        public ICommand PreCommand
        {
            get { return _preCmd; }
            set { _preCmd = value; }
        }

        public ICommand PostCommand
        {
            get { return _postCmd;  }        
            set { _postCmd = value; }
        }

        public ICommand Command
        {
            get
            {
                return _cmd;
            }
            set
            {
                _cmd = value;
            }
        }

        public CommandResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }

        public int RequestID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public IProcessor ProcessorResult
        {
            get { return _processor; }
            set { _processor = value; }
        }

        public Type ProcessorType
        {
            get { return _procType; }
            set { _procType = value; }
        }

        #endregion

        public void OnRequestCompleted()
        {
            if (RequestCompleted != null)
                RequestCompleted(this);
        }

        /// <summary>
        /// Gets the ProcessorResult object cast as the required type
        /// </summary>
        /// <typeparam name="T">A type derived from IProcessor</typeparam>
        /// <returns></returns>
        public T GetProcessorAsType<T>() where T : IProcessor
        {
            return (T) _processor;
        }

        public void RunProcessor()
        {
            IProcessor proc = (IProcessor)Activator.CreateInstance(ProcessorType);
            ProcessorResult = proc;
            proc.CmdResult = Result;
            proc.Request = this;
            proc.Client = Client;
            proc.ProcessResult();
            
        }

        #region CTOR
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <param name="callback"></param>
        public BaseRequest(RequestCompletedCallback callback)
        {
            //_client = client;
            RequestCompleted += callback;
        }
        #endregion
    }
}