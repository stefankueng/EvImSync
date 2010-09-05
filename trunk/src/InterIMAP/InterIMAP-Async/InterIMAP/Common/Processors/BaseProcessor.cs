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

namespace InterIMAP.Common.Processors
{
    /// <summary>
    /// Defines common values and functionality for a Processor
    /// </summary>
    public abstract class BaseProcessor : IProcessor
    {
        #region Static Methods
        
        /// <summary>
        /// Creates an new instance of the specified processor type, and then processes the results
        /// </summary>
        /// <param name="procType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IProcessor RunProcessor(Type procType, CommandResult result)
        {
            //T proc = new T();
            IProcessor proc = (IProcessor)Activator.CreateInstance(procType);

            proc.CmdResult = result;
            proc.Request = null;
            proc.ProcessResult();

            return proc;
        }
        #endregion

        #region Protected Fields

        protected CommandResult _cmdResult;
        protected IRequest _request;
        protected IMAPAsyncClient _client;
        
        #endregion

        #region Public Properties
        public CommandResult CmdResult
        {
            get { return _cmdResult; }
            set { _cmdResult = value; }
        }

        public IRequest Request
        {
            get { return _request; }
            set { _request = value;  }
        }

        public IMAPAsyncClient Client
        {
            get { return _client; }
            set { _client = value; }
        }
        #endregion

        #region Virtual Methods        
        public virtual void ProcessResult()
        {
            if (_cmdResult == null)
                throw new NullReferenceException("_cmdResult cannot be null");
        }
        #endregion

        #region Private Methods
        protected void PrintResults()
        {
            foreach (string s in _cmdResult.Results)
                Console.WriteLine(s);
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default Constructor
        /// </summary>
        protected BaseProcessor()
        {
            
        }
        #endregion
    }
}
