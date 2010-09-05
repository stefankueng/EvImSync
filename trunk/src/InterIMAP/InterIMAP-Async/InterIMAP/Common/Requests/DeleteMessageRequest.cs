using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{
    public class DeleteMessageRequest : BaseRequest
    {
        public DeleteMessageRequest(IMessage msg, RequestCompletedCallback callback)
            : base(callback)
        {
            PreCommand = new SelectFolderCommand(msg.Folder, null);
            Command = new ChangeFlagCommand(msg, MessageFlag.Deleted, true, null);
            PostCommand = new ExpungeCommand(null);
            ProcessorType = typeof (DeleteMessageProcessor);
        }
    }

    internal class DeleteMessageProcessor : BaseProcessor
    {
        //public override void ProcessResult()
        //{
        //    base.ProcessResult();

        //    foreach (string line in CmdResult.Results)
        //        _request.Client.Aggregator.AddMessage(0,line);
        //}
    }
}
