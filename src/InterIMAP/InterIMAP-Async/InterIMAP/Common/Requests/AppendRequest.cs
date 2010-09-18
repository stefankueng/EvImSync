using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{
    public class AppendRequest : BaseRequest
    {
        public AppendRequest(string emlContent, string flags, IFolder parentFolder, RequestCompletedCallback callback)
            : base(callback)
        {
            Command = new AppendCommand(emlContent, flags, parentFolder, null);
            ProcessorType = typeof (AppendProcessor);
        }
    }
}
