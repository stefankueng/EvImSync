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
using InterIMAP.Common.Attributes;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Asynchronous.Objects
{
    /// <summary>
    /// Represents a single Contact in the mailbox
    /// </summary>
    [LinkToTable("Contact")]
    public class Contact : IContact
    {
        #region Private Fields        
        private readonly int _id;
        private readonly IMAPAsyncClient _client;
        #endregion

        #region Public Properties
        ///<summary>
        /// The Contact's First Name
        ///</summary>
        public string FirstName
        {
            get { return _client.DataManager.GetValue<Contact, String>(this, "FirstName"); }
            set { _client.DataManager.SetValue(this, "FirstName", value); }
        }

        /// <summary>
        /// The Contact's Last Name
        /// </summary>
        public string LastName
        {
            get { return _client.DataManager.GetValue<Contact, String>(this, "LastName"); }
            set { _client.DataManager.SetValue(this, "LastName", value); }
        }

        /// <summary>
        /// The Contact's E-Mail Address
        /// </summary>
        public string EMail
        {
            get { return _client.DataManager.GetValue<Contact, String>(this, "EMail"); }
            set { _client.DataManager.SetValue(this, "EMail", value); }
        }

        /// <summary>
        /// The full name of this contact
        /// </summary>
        public string FullName
        {
            get { return _client.DataManager.GetValue<Contact, String>(this, "FullName"); }
            set { _client.DataManager.SetValue(this, "FullName", value); }
        }

        /// <summary>
        /// The Contact's ID
        /// </summary>
        public int ID
        {
            get { return _id; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Create a new Contact object with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="client"></param>
        public Contact(IMAPAsyncClient client, int id)
        {
            _id = id;
            _client = client;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(FullName))
                return string.Format("{0} <{1}>", FullName, EMail);
            
            if (!string.IsNullOrEmpty(FirstName) ||
                !string.IsNullOrEmpty(LastName) &&
                !string.IsNullOrEmpty(EMail))
                return string.Format("{0} {1} <{2}>", FirstName, LastName, EMail);

            if (string.IsNullOrEmpty(FirstName) &&
                string.IsNullOrEmpty(LastName) &&
                !string.IsNullOrEmpty(EMail))
                return string.Format("{0}", EMail);

            

            return string.Format("{0} {1}", FirstName, LastName);


        }
        #endregion
    }
}
