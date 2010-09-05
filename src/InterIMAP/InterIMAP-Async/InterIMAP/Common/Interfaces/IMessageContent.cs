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

namespace InterIMAP.Common.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessageContent : IBaseObject
    {        
        /// <summary>
        /// ContentDescription field
        /// </summary>
        string ContentDescription { get; set; }

        /// <summary>
        /// MIMEVersion field
        /// </summary>
        string MIMEVersion { get; set; }

        /// <summary>
        /// Filename if this section is an attachment
        /// </summary>
        string ContentFilename { get; set; }

        /// <summary>
        /// ContentDisposition field
        /// </summary>
        string ContentDisposition { get; set; }

        /// <summary>
        /// ContentId field
        /// </summary>
        string ContentId { get; set; }

        /// <summary>
        /// The Part of the message body this is
        /// </summary>
        string PartID { get; set; }

        /// <summary>
        /// Usually represents the plain text content of a message
        /// </summary>
        string TextData { get; set; }

        /// <summary>
        /// Contains the binary data of an attachment
        /// </summary>
        byte[] BinaryData { get; set; }

        /// <summary>
        /// The type of content
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// The encoding used for this content part
        /// </summary>
        string ContentTransferEncoding { get; set; }

        /// <summary>
        /// The size of the content
        /// </summary>
        Int64 ContentSize { get; set; }

        /// <summary>
        /// Number of lines (applies to TextData mainly)
        /// </summary>
        Int64 Lines { get; set; }

        /// <summary>
        /// The message this part belongs to
        /// </summary>
        int MessageID { get; set; }

        /// <summary>
        /// The MD5 field
        /// </summary>
        string MD5 { get; set; }

        /// <summary>
        /// The Language field
        /// </summary>
        string Language { get; set; }

        /// <summary>
        /// The charset field
        /// </summary>
        string Charset { get; set; }

        /// <summary>
        /// A reference to the message object
        /// </summary>
        IMessage Message { get; }

        /// <summary>
        /// The HTML data of this message
        /// </summary>
        string HTMLData { get; set; }

        /// <summary>
        /// Flag indicating if this part is an attachment
        /// </summary>
        bool IsAttachment { get; }

    }
}
