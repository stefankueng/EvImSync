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

namespace InterIMAP.Common.Interfaces
{
    
    /// <summary>
    /// Public properties of the Folder object
    /// </summary>
    public interface IFolder : IBaseObject
    {
        /// <summary>
        /// The internal ID of this folder
        /// </summary>
        //int ID { get; set; }

        /// <summary>
        /// The name of this folder
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The ID of this folders parent
        /// </summary>
        int ParentID { get; set; }

        /// <summary>
        /// The full path of this folder
        /// </summary>
        string FullPath { get; set; }

        /// <summary>
        /// The list of this folders children
        /// </summary>
        IFolder[] SubFolders { get; }

        /// <summary>
        /// Number of messages in this folder
        /// </summary>
        int Exists { get; set; }

        /// <summary>
        /// Number of recently added messages in this folder
        /// </summary>
        int Recent { get; set; }

        /// <summary>
        /// Number of new messages in this folder
        /// </summary>
        int Unseen { get; set; }

        /// <summary>
        /// List of messages in this folder
        /// </summary>
        IMessage[] Messages { get; }

        /// <summary>
        /// A reference to this folders parent folder
        /// </summary>
        IFolder Parent { get;  }
    }
}