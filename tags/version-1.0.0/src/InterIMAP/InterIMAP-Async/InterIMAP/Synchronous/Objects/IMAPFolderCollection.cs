/********************************************************************************************
 * IMAPFolderCollection.cs
 * Part of the InterIMAP Library
 * Copyright (C) 2004-2007 Rohit Joshi
 * Copyright (C) 2008 Jason Miesionczek
 * Original Author: Rohit Joshi
 * Based on this article on codeproject.com:
 * IMAP Client library using C#
 * http://www.codeproject.com/KB/IP/imaplibrary.aspx?msg=2498332
 * Posted: August 16th 2004
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
using System.Text;
using System.Xml.Serialization;

namespace InterIMAP.Synchronous
{
    /// <summary>
    /// Collection class to make selecting specific folders a little easier
    /// </summary>
    [Serializable]
    public class IMAPFolderCollection : List<IMAPFolder>
    {
        /// <summary>
        /// Search within this folder for a sub-folder with the specified name. Not Recursive.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [XmlIgnore]
        public IMAPFolder this[string name]
        {
            get
            {
                foreach (IMAPFolder f in this)
                {
                    if (f.FolderName.Equals(name))
                    {
                        f.Examine();
                        if (f._client.Config.AutoGetMsgID)
                            f.GetMessageIDs(false);
                        return f;
                    }
                }

                return null;
            }
        }

        
        
    }
}
