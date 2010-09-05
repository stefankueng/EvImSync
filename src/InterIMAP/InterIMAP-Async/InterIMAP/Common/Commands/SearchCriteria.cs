using System;
using System.Collections.Generic;
using System.Text;

namespace InterIMAP.Common.Commands
{
    /// <summary>
    /// Defines the information that is being searched for
    /// </summary>
    public class SearchCriteria
    {
        #region Private Fields

        private bool _searchForNew;
        #endregion

        #region Public Properties
        /// <summary>
        /// Indicates that the results should contain new messages only
        /// </summary>
        public bool SearchForNew
        {
            get { return _searchForNew; }
            set { _searchForNew = value; }            
        }
        #endregion

        #region CTORs
        /// <summary>
        /// Create a new search criteria including new messages
        /// </summary>
        /// <param name="includeNew"></param>
        public SearchCriteria(bool includeNew)
        {
            _searchForNew = includeNew;
        }
    
        #endregion

        #region Overrides
        /// <summary>
        /// Generates the proper search string based on the values that have been set
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (_searchForNew)
                sb.Append("UNSEEN");

            return sb.ToString();
        }
        #endregion
    }
}
