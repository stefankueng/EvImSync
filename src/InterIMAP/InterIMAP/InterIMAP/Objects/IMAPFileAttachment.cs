/********************************************************************************************
 * IMAPFileAttachment.cs
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
using System.IO;

namespace InterIMAP
{
    /// <summary>
    /// Contains all the data about a single file that is either attached to a message or embedded in it
    /// </summary>
    [Serializable]   
    public class IMAPFileAttachment
    {
        #region Private Fields
        private byte[] _fileData;        
        private string _fileName;        
        private int _fileSize;        
        private string _fileEncoding;
        private string _fileType;
        private string _partID;
        #endregion

        #region Public Properties
        /// <summary>
        /// The section of the body that contains this attachment
        /// </summary>
        public string PartID
        {
            get { return _partID; }
            set { _partID = value; }
        }
        /// <summary>
        /// The content-type of the file attachment
        /// </summary>
        public string FileType
        {
            get { return _fileType; }
            set { _fileType = value; }
        }
        /// <summary>
        /// The array of bytes that comprise the file
        /// </summary>
        public byte[] FileData
        {
            get { return _fileData; }
            set { _fileData = value; }
        }

        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// The size of the file
        /// </summary>
        public int FileSize
        {
            get { return _fileSize; }
            set { _fileSize = value; }
        }

        /// <summary>
        /// The encoding method of the file
        /// </summary>
        public string FileEncoding
        {
            get { return _fileEncoding; }
            set { _fileEncoding = value; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default Constructor
        /// </summary>
        public IMAPFileAttachment() { }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a MemoryStream of the file data
        /// </summary>
        /// <returns></returns>
        public Stream GetFileStream()
        {                        
            return new MemoryStream(_fileData);
        }

        /// <summary>
        /// Saves this file, using the retreived filename, to the specified location
        /// </summary>
        /// <param name="downloadLocation">Path on the local filesystem to which the file will be saved</param>
        public void SaveFile(string downloadLocation)
        {
            FileStream fs = new FileStream(downloadLocation + _fileName, FileMode.Create, FileAccess.Write);
            fs.Write(_fileData, 0, _fileData.Length);
            fs.Close();
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Returns the filename of this attachment object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _fileName;
        }
        #endregion
    }
}
