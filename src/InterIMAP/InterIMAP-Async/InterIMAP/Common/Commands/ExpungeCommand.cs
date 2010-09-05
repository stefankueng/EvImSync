using System;
using System.Collections.Generic;
using System.Text;

namespace InterIMAP.Common.Commands
{
    public class ExpungeCommand : BaseCommand
    {
        protected override bool ValidateParameters()
        {
            return true;
        }

        public ExpungeCommand(CommandDataReceivedCallback callback)
            : base(callback)
        {
            CommandString = "EXPUNGE";
        }
    }
}
