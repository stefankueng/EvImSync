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
    /// Public properties of the Message object
    /// </summary>
    public interface IMessage : IBaseObject
    {
        /// <summary>
        /// The message UID
        /// </summary>
        int UID { get; set; }

        /// <summary>
        /// The message Subject
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// The Date the message was sent
        /// </summary>
        DateTime DateSent { get; set; }

        /// <summary>
        /// The Date the message was received
        /// </summary>
        DateTime DateReceived { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ReceivedSPF { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ContentTransferEncoding { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string DeliveredTo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string XGMailReceived { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Organization { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string InReplyTo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string XOriginatingIP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Received { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string MimeVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ContentClass { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ReturnPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string XMailer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string XMimeOLE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string XOriginalArrivalTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string MessageID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string XMSTNEFCorrelator { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ThreadTopic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ThreadIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int FolderID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IContact[] FromContacts { get; }

        /// <summary>
        /// 
        /// </summary>
        IContact[] ToContacts { get; }

        /// <summary>
        /// 
        /// </summary>
        IContact[] CcContacts { get; }

        /// <summary>
        /// 
        /// </summary>
        IContact[] BccContacts { get; }

        /// <summary>
        /// 
        /// </summary>
        IMessageContent[] MessageContent { get; }

        /// <summary>
        /// 
        /// </summary>
        IFolder Folder { get; }

        /// <summary>
        /// Returns the content of the first part that contains text only data
        /// </summary>
        string TextData { get; }

        /// <summary>
        /// Returns the content of the first part that contains HTML data
        /// </summary>
        string HTMLData { get; }

        /// <summary>
        /// Flag indicating in this message has been seen
        /// </summary>
        bool Seen { get; set; }

        /// <summary>
        /// Flag indicating if this is a recent message
        /// </summary>
        bool Recent { get; }

        /// <summary>
        /// Flag indicating if this message is marked for deletion
        /// </summary>
        bool Deleted { get; set; }

        /// <summary>
        /// Flag indicating if this message is flagged... (that sounds rather redundant)
        /// </summary>
        bool Flagged { get; set; }

        /// <summary>
        /// Flag indicating if this message has been answered
        /// </summary>
        bool Answered { get; set; }

        /// <summary>
        /// Flag indicating if this message is a draft
        /// </summary>
        bool Draft { get; set; }

        /// <summary>
        /// Flag indicating if this message is new (opposite logic of Seen)
        /// </summary>
        bool New { get; set; }

        /// <summary>
        /// Flag indicating if the headers for this message have already been downloaded
        /// </summary>
        bool HeaderLoaded { get; }

        /// <summary>
        /// Flag indicating if the content for this message has already been downloaded
        /// </summary>
        bool ContentLoaded { get; }

        /// <summary>
        /// returns the state of the custom flag
        /// </summary>
        /// <param name="flag">the name of the custom flag</param>
        /// <returns>true if the flag exists, false if it does not exist</returns>
        bool GetCustomFlag(string flag);

        /// <summary>
        /// sets the custom flag
        /// </summary>
        /// <param name="flag">the flag name</param>
        /// <param name="value">if true, the flag is set. If false, the flag is removed</param>
        void SetCustomFlag(string flag, bool value);

        /// <summary>
        /// returns a list of custom flags this message has
        /// </summary>
        System.Collections.Generic.List<string> GetCustomFlags();

    }
}
