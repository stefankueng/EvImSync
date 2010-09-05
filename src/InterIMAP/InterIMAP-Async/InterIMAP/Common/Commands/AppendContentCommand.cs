using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Commands
{
    public class AppendContentCommand : BaseCommand
    {
        protected override bool ValidateParameters()
        {
            return true;
        }

        /// <summary>
        /// Appends a message to the current mailbox
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="parentFolder"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AppendContentCommand(string emlContent, IFolder parentFolder, CommandDataReceivedCallback callback)
            : base(callback)
        {
            _parameters.Add(emlContent);
            CommandStringPlain = emlContent;
        }
        public override bool UseSameCmdIDAsLastCommand
        {
            get { return true; }
        }
    }
}
