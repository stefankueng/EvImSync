using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{
    public class MessageFlagRequest : BaseRequest
    {

        public MessageFlagRequest(IMessage msg, RequestCompletedCallback callback) : base(callback)
        {
            PreCommand = new ExamineFolderCommand(msg.Folder, null);
            Command = new MessageFlagCommand(msg, null);
            ProcessorType = typeof (MessageFlagProcessor);
        }
    }
}
