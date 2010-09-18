/********************************************************************************************
 * IMAPSearchQuery.cs
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

namespace InterIMAP
{
    /// <summary>
    /// Class for building a search query to search for messages within a folder
    /// </summary>
    public class IMAPSearchQuery
    {
        #region Private Fields
        private List<IMAPMailAddress> _to;
        private List<IMAPMailAddress> _from;
        private List<IMAPMailAddress> _cc;
        private List<IMAPMailAddress> _bcc;
        private string _subject;
        private string _content;
        private string _date;
        private string _beforeDate;
        private string _afterDate;
        private bool _new;
        private bool _recent;
        private bool _answered;
        private bool _deleted;
        private bool _draft;
        private int _largerThan;
        private int _smallerThan;
        private DateRange _dateRange;
        #endregion

        #region Public Properties
        /// <summary>
        /// Search for messages with addresses in the Tofield
        /// </summary>
        public List<IMAPMailAddress> To
        {
            get { return _to; }
            set { _to = value; }
        }

        /// <summary>
        /// Search for messages with addresses in the From field
        /// </summary>
        public List<IMAPMailAddress> From
        {
            get { return _from; }
            set { _from = value; }
        }

        /// <summary>
        /// Search for messages with addresses in the CC field
        /// </summary>
        public List<IMAPMailAddress> CC
        {
            get { return _cc; }
            set { _cc = value; }
        }

        /// <summary>
        /// Search for messages with addresses in the BCC field
        /// </summary>
        public List<IMAPMailAddress> BCC
        {
            get { return _bcc; }
            set { _bcc = value; }
        }

        /// <summary>
        /// Search for messages where the subject contains the specified string
        /// </summary>
        public string Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        /// <summary>
        /// Search for messages where the Text and HTML content contains the specified string
        /// </summary>
        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        /// <summary>
        /// Search for messages where the Received date equals the specified date
        /// </summary>
        public string Date
        {
            get { return _date; }
            set { _date = value; }
        }

        /// <summary>
        /// Search for messages where the received date is before the specified date
        /// </summary>
        public string BeforeDate
        {
            get { return _beforeDate; }
            set { _beforeDate = value; }
        }

        /// <summary>
        /// Search for messages where the received date is after the specified date
        /// </summary>
        public string AfterDate
        {
            get { return _afterDate; }
            set { _afterDate = value; }
        }

        /// <summary>
        /// Search for new messages
        /// </summary>
        public bool New
        {
            get { return _new; }
            set { _new = value; }
        }

        /// <summary>
        /// Search for recent messages
        /// </summary>
        public bool Recent
        {
            get { return _recent; }
            set { _recent = value; }
        }

        /// <summary>
        /// Search for answered messages
        /// </summary>
        public bool Answered
        {
            get { return _answered; }
            set { _answered = value; }
        }

        /// <summary>
        /// Search for messages marked as deleted
        /// </summary>
        public bool Deleted
        {
            get { return _deleted; }
            set { _deleted = value; }
        }

        /// <summary>
        /// Search for messages marked as a draft
        /// </summary>
        public bool Draft
        {
            get { return _draft; }
            set { _draft = value; }
        }

        /// <summary>
        /// Search for messages that are larger than the specified size (bytes)
        /// </summary>
        public int LargerThan
        {
            get { return _largerThan; }
            set { _largerThan = value; }
        }

        /// <summary>
        /// Search for messages that are smaller than the specified size (bytes)
        /// </summary>
        public int SmallerThan
        {
            get { return _smallerThan; }
            set { _smallerThan = value; }
        }

        /// <summary>
        /// Search for messages whose date falls within the start and end date of this range
        /// </summary>
        public DateRange Range
        {
            get { return _dateRange; }
            set { _dateRange = value; }
        }
        #endregion

        #region CTORs
        /// <summary>
        /// Default constructor
        /// </summary>
        public IMAPSearchQuery()
        {
            _to = new List<IMAPMailAddress>();
            _from = new List<IMAPMailAddress>();
            _cc = new List<IMAPMailAddress>();
            _bcc = new List<IMAPMailAddress>();

            _subject = String.Empty;
            _content = String.Empty;

            _date = String.Empty;
            _beforeDate = String.Empty;
            _afterDate = String.Empty;

            _new = false;
            _recent = false;
            _draft = false;
            _deleted = false;
            _answered = false;

            _largerThan = -1;
            _smallerThan = -1;

            _dateRange = null;
        }
        #endregion

        #region Static Quick Search Methods
        /// <summary>
        /// Quickly create a search query specifying the display name and/or email address for the To: field
        /// </summary>
        /// <param name="name">display name of the person to search for</param>
        /// <param name="addr">e-mail address of the person to search for</param>
        /// <returns>IMAPSearchQuery object suitable for passing into the search method</returns>
        public static IMAPSearchQuery QuickSearchTo(string name, string addr)
        {
            IMAPSearchQuery query = new IMAPSearchQuery();
            IMAPMailAddress a = new IMAPMailAddress();
            a.Address = addr;
            a.DisplayName = name;
            query.To.Add(a);

            return query;
        }

        /// <summary>
        /// Quickly create a search query specifying the display name and/or email address for the From: field
        /// </summary>
        /// <param name="name">display name of the person to search for</param>
        /// <param name="addr">e-mail address of the person to search for</param>
        /// <returns>IMAPSearchQuery object suitable for passing into the search method</returns>
        public static IMAPSearchQuery QuickSearchFrom(string name, string addr)
        {
            IMAPSearchQuery query = new IMAPSearchQuery();
            IMAPMailAddress a = new IMAPMailAddress();
            a.Address = addr;
            a.DisplayName = name;
            query.From.Add(a);

            return query;
        }

        /// <summary>
        /// Quickly create a search query specifying the display name and/or email address for the CC: field
        /// </summary>
        /// <param name="name">display name of the person to search for</param>
        /// <param name="addr">e-mail address of the person to search for</param>
        /// <returns>IMAPSearchQuery object suitable for passing into the search method</returns>
        public static IMAPSearchQuery QuickSearchCC(string name, string addr)
        {
            IMAPSearchQuery query = new IMAPSearchQuery();
            IMAPMailAddress a = new IMAPMailAddress();
            a.Address = addr;
            a.DisplayName = name;
            query.CC.Add(a);

            return query;
        }

        /// <summary>
        /// Quickly create a search query specifying the display name and/or email address for the BCC: field
        /// </summary>
        /// <param name="name">display name of the person to search for</param>
        /// <param name="addr">e-mail address of the person to search for</param>
        /// <returns>IMAPSearchQuery object suitable for passing into the search method</returns>
        public static IMAPSearchQuery QuickSearchBCC(string name, string addr)
        {
            IMAPSearchQuery query = new IMAPSearchQuery();
            IMAPMailAddress a = new IMAPMailAddress();
            a.Address = addr;
            a.DisplayName = name;
            query.BCC.Add(a);

            return query;
        }

        /// <summary>
        /// Quickly create a search query specifying the subject string to search for
        /// </summary>
        /// <param name="subject">The text to search for within the subject</param>
        /// <returns>IMAPSearchQuery object suitable for passing into the search methods</returns>
        public static IMAPSearchQuery QuickSearchSubject(string subject)
        {
            IMAPSearchQuery query = new IMAPSearchQuery();
            query.Subject = subject;

            return query;
        }

        /// <summary>
        /// Quickly create a search query specifying the content string to search for
        /// </summary>
        /// <param name="content">The text to search with the Text and HTML content sections</param>
        /// <returns>IMAPSearchQuery object suitable for passing into the search methods</returns>
        public static IMAPSearchQuery QuickSearchContent(string content)
        {
            IMAPSearchQuery query = new IMAPSearchQuery();
            query.Content = content;

            return query;
        }

        /// <summary>
        /// Quickly create a search query searching only for new messages
        /// </summary>
        /// <returns>IMAPSearchQuery object suitable for passing into the search methods</returns>
        public static IMAPSearchQuery QuickSearchNew()
        {
            IMAPSearchQuery query = new IMAPSearchQuery();
            query.New = true;

            return query;
        }
        /// <summary>
        /// Quickly create a search query searching only for messages who date is within the date range
        /// </summary>
        /// <param name="sdate"></param>
        /// <param name="edate"></param>
        /// <returns></returns>
        public static IMAPSearchQuery QuickSearchDateRange(string sdate, string edate)
        {
            IMAPSearchQuery query = new IMAPSearchQuery();
            query.Range = new DateRange(sdate, edate);

            return query;
        }

        /// <summary>
        /// Quickly create a search query searching only for messages who date is within the date range
        /// </summary>
        /// <param name="sdate"></param>
        /// <param name="edate"></param>
        /// <returns></returns>
        public static IMAPSearchQuery QuickSearchDateRange(DateTime sdate, DateTime edate)
        {
            IMAPSearchQuery query = new IMAPSearchQuery();
            query.Range = new DateRange(sdate, edate);

            return query;
        }
        #endregion
    }

    /// <summary>
    /// Simple class to facilitate the searching of messages within a start and end date
    /// </summary>
    public class DateRange
    {
        #region Private Fields
        private DateTime _startDate;
        private DateTime _endDate;
        #endregion

        #region Public Properties
        /// <summary>
        /// The beginning of the date range
        /// </summary>
        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        /// <summary>
        /// The end of the date range
        /// </summary>
        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Construct a DateRange object using strings to specify the start and end dates
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public DateRange(string startDate, string endDate)
        {                        
            if (!DateTime.TryParse(startDate, out _startDate))
            {
                throw new ArgumentException("Could not convert start date");
            }

            if (!DateTime.TryParse(endDate, out _endDate))
            {
                throw new ArgumentException("Could not convert end date");
            }
        }

        /// <summary>
        /// Construct a DateRange object using DateTime objects for the start and end dates
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public DateRange(DateTime startDate, DateTime endDate)
        {
            _startDate = startDate;
            _endDate = endDate;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Determines if the specified date falls within the range of this instance
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <returns>True if within the range, False if outside the range</returns>
        public bool DateWithinRange(DateTime date)
        {
            if (date >= _startDate && date <= _endDate)
                return true;
            else
                return false;
        }
        #endregion
    }
}
