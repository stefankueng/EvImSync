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
using System.Collections.Generic;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Commands
{
    /// <summary>
    /// Implements ICommand and provides common functionality
    /// </summary>
    public abstract class BaseCommand : ICommand
    {
        /// <summary>
        /// Callback signature for event that sends updates about the amount of data that has been
        /// received for a given command
        /// </summary>
        /// <param name="cmd">The command that is executing</param>
        /// <param name="bytesReceived">Total number of bytes received</param>
        /// <param name="totalBytes">Total number of bytes expected</param>
        public delegate void CommandDataReceivedCallback(ICommand cmd, long bytesReceived, long totalBytes);

        public event CommandDataReceivedCallback CommandDataReceived;
        
        #region Private Members
        private string _commandString;
        private string _commandData = "";
        protected List<string> _parameters;
        protected List<object> _parameterObjs;
        private int _commandNum;        
        private string _commandDetail = "";
        #endregion

        #region Abstract Members
        protected abstract bool ValidateParameters();
        #endregion

        #region ICommand Members

        public virtual string CommandStringPlain
        {
            get
            {
                if (ValidateParameters())
                    return _commandString;

                return String.Empty;
            }
            protected set
            {
                _commandDetail = value;

                _commandString = _commandDetail;
            }
        }

        public virtual string CommandString
        {
            get
            {
                if (ValidateParameters())
                    return _commandString;
                
                return String.Empty;
            }
            protected set
            {
                ResetCommand();
                _commandDetail = value.Replace("\r\n", "");

                _commandString = GenerateCommand();                
            }
        }

        public virtual string[] Parameters
        {
            get
            {
                return _parameters.ToArray();
            }

        }

        public virtual object[] ParameterObjects
        {
            get { return _parameterObjs.ToArray(); }
        }

        public virtual string CommandID
        {
            get
            {
                return String.Format("IMAP00{0}", _commandNum);
            }
        }

        public virtual string ResponseOK
        {
            get { return String.Format("{0} OK", CommandID); }
        }

        public virtual string ResponseGoAhead
        {
            get { return string.Empty; }
        }

        public virtual string ResponseNO
        {
            get { return String.Format("{0} NO", CommandID); }
        }

        public virtual string ResponseBAD
        {
            get { return String.Format("{0} BAD", CommandID); }
        }

        public virtual string CommandData
        {
            get { return _commandData; }
            set { _commandData = value.Replace("\r", "").Replace("\n", "\r\n") + "\r\n"; }
        }


        public virtual bool UseSameCmdIDAsLastCommand
        {
            get { return false; }
        }

        #endregion

        #region Public Properties
        public int CommandNumber
        {
            get { return _commandNum; }
            set { _commandNum = value;
                _commandString = GenerateCommand(); }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Initialize common fields
        /// </summary>
        protected BaseCommand(CommandDataReceivedCallback callback)
        {
            _parameters = new List<string>();
            _parameterObjs = new List<object>();
            _commandString = "";
            CommandDataReceived = callback;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Called when data is received
        /// </summary>
        /// <param name="receivedBytes"></param>
        /// <param name="totalBytes"></param>
        public void OnCommandDataReceived(long receivedBytes, long totalBytes)
        {
            if (CommandDataReceived != null)
                CommandDataReceived(this, receivedBytes, totalBytes);

        }
        #endregion

        #region Private Methods

        private void ResetCommand()
        {            
            _commandString = CommandID + " ";
        }

        private string GenerateCommand()
        {
            return String.Format("{0} {1}\r\n", CommandID, _commandDetail);
        }
        #endregion


    }
}
