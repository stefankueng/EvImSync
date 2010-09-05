using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Commands
{
    public class MessageFlagCommand : BaseCommand
    {
        protected override bool ValidateParameters()
        {
            return true;
        }

        public MessageFlagCommand(IMessage msg, CommandDataReceivedCallback callback) : base(callback)
        {
            if (msg == null) return;

            _parameters.Add(msg.UID.ToString());
            _parameterObjs.Add(msg);

            CommandString = String.Format("UID FETCH {0} FLAGS", Parameters);
        }
    }
}
