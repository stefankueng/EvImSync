using System;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Commands
{

    /// <summary>
    /// Command for message copy.
    /// </summary>
    public class CopyMessageCommand : BaseCommand
    {

        protected override bool ValidateParameters()
        {
            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyMessageCommand"/> class.
        /// </summary>
        /// <param name="msg">The message to copy.</param>
        /// <param name="destFolder">The destination folder.</param>
        /// <param name="callback">The callback to call when operation completed.</param>
        public CopyMessageCommand(IMessage msg, IFolder destFolder, CommandDataReceivedCallback callback)
            : base(callback)
        {
            //  UID COPY 4963 "DestFolder"
            const string cmd = "UID COPY {0} \"{1}\"";
            _parameters.Add(msg.UID.ToString());
            _parameters.Add(destFolder.FullEncodedPath);
            _parameterObjs.Add(msg);

            CommandString = String.Format(cmd, Parameters);
        }
    }
}
