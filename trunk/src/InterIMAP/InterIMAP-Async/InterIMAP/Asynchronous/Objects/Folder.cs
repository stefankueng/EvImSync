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
using InterIMAP.Common.Attributes;
using InterIMAP.Common.Interfaces;
using InterIMAP.Asynchronous.Client;
namespace InterIMAP.Asynchronous.Objects
{
    /// <summary>
    /// A single Folder object in the mailbox
    /// </summary>
    [LinkToTable("Folder")]
    public class Folder : IFolder
    {

        #region Private Fields
        private readonly int _id;
        private readonly IMAPAsyncClient _client;
        #endregion

        #region IFolder Members

        /// <summary>
        /// This Folder's ID
        /// </summary>
        public int ID
        {
            get
            {
                return _id;
            }
            
        }

        public string Name
        {
            get
            {
                return _client.DataManager.GetValue<Folder, String>(this, "Name");
            }
            set
            {
                _client.DataManager.SetValue(this, "Name", value);
                _client.DataManager.UpdateFullPath(this);
            }
        }

        public int ParentID
        {
            get
            {
                return _client.DataManager.GetValue<Folder, int>(this, "ParentID");
            }
            set
            {
                _client.DataManager.SetValue(this, "ParentID", value);
                _client.DataManager.UpdateFullPath(this);
            }
        }

        public string FullPath
        {
            get
            {
                return _client.DataManager.GetValue<Folder, String>(this, "FullPath");
            }
            set
            {
                _client.DataManager.SetValue(this, "FullPath", value);
            }
        }

        public IFolder[] SubFolders
        {
            get { return _client.MailboxManager.GetSubFolders(this); }
        }

        public IMessage[] Messages
        {
            get { return _client.MailboxManager.GetMessagesByFolder(this); }
        }

        public int Exists
        {
            get { return _client.DataManager.GetValue<Folder, int>(this, "Exists"); }
            set { _client.DataManager.SetValue(this, "Exists", value); }
        }

        public int Recent
        {
            get { return _client.DataManager.GetValue<Folder, int>(this, "Recent"); }
            set { _client.DataManager.SetValue(this, "Recent", value); }
        }

        public int Unseen
        {
            get { return _client.DataManager.GetValue<Folder, int>(this, "Unseen"); }
            set { _client.DataManager.SetValue(this, "Unseen", value); }
        }

        public IFolder Parent
        {
            get { return ParentID > -1 ? _client.MailboxManager.GetFolderByID(ParentID) : null; }
        }
        #endregion
        
        #region CTOR
        /// <summary>
        /// Create a new Folder object with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="client"></param>
        public Folder(IMAPAsyncClient client, int id)
        {
            _id = id;
            _client = client;
        }
        #endregion
    }
}