using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Commands
{
    /// <summary>
    /// Sends a carefully crafted search command to the server
    /// </summary>
    public class SearchCommand : BaseCommand
    {
        protected override bool ValidateParameters()
        {
            return true;
        }

        /// <summary>
        /// Create a new Search command using the specified criteria
        /// </summary>        
        /// <param name="criteria"></param>
        /// <param name="callback"></param>
        public SearchCommand(SearchCriteria criteria, CommandDataReceivedCallback callback)
            : base(callback)
        {
            _parameters.Add(criteria.ToString());
            _parameterObjs.Add(criteria);
            
            CommandString = String.Format("UID SEARCH {0}", criteria.ToString());
        }
    }
}
