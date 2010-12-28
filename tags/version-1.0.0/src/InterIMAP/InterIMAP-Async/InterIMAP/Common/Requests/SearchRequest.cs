using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{
    public class SearchRequest : BaseRequest
    {
        public SearchRequest(IFolder folder, SearchCriteria criteria, RequestCompletedCallback callback)
            : base(callback)
        {
            PreCommand = new SelectFolderCommand(folder, null);
            Command = new SearchCommand(criteria, null);
            ProcessorType = typeof (SearchProcessor);
        }
    }
}
