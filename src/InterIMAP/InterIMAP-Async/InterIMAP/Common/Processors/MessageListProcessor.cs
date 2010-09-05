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

namespace InterIMAP.Common.Processors
{
    /// <summary>
    /// Processes the string of message UIDs into an array of Integers
    /// </summary>
    public class MessageListProcessor : BaseProcessor
    {
        #region Private Fields
        private readonly List<int> uids = new List<int>();
        private IFolder sourceFolder;
        #endregion

        #region Public Properties
        /// <summary>
        /// List of UIDs retreived from the active folder
        /// </summary>
        public int[] UIDs
        {
            get
            {
                return uids.ToArray();
            }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default Constructor
        /// </summary>
        public MessageListProcessor()
        {
            //_tempResults = new List<string>();
            uids = new List<int>();                        
        }
        #endregion

        public override void ProcessResult()
        {
            base.ProcessResult();

            sourceFolder = (IFolder)Request.Command.ParameterObjects[0];
            
            
            foreach (string s in CmdResult.Results)
            {
                if (s.StartsWith("* SEARCH "))
                    ParseUID(s.Replace("* SEARCH ",""));

                break;
            }

            if (uids.Count == 0) return;

            CreateMessageStubs();
        }

        private void ParseUID(string input)
        {
            string[] idstrings = input.Split(new char[] {' '});
            foreach (string id in idstrings)
            {
                int uid;
                if (Int32.TryParse(id, out uid))
                    uids.Add(uid);
            }
        }

        private void CreateMessageStubs()
        {
            foreach (int uid in uids)
            {
                _client.MailboxManager.AddMessage(uid, sourceFolder.ID);
            }
        }
    }
}
