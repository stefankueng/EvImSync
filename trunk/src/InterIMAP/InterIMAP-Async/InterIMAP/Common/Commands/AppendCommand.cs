using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Commands
{
    public class AppendCommand : BaseCommand
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
        public AppendCommand(string eml, string flags, IFolder parentFolder, CommandDataReceivedCallback callback)
            : base(callback)
        {
            string fullFolder = parentFolder.FullPath;
            fullFolder = fullFolder.Replace("\"", "");

            _parameters.Add(fullFolder);
            CommandData = eml;
            CommandString = String.Format("APPEND \"{0}\" (\\Seen {1}) {{{2}}}", fullFolder, flags, Encoding.ASCII.GetBytes(CommandData.ToCharArray()).GetLength(0) - 2);
        }

        public override string ResponseGoAhead
        {
            get { return "+ "; }
        }

    }
}
