using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{
    public class RenameFolderRequest : BaseRequest
    {
        public RenameFolderRequest(IFolder folder, string newName, RequestCompletedCallback callback)
            : base(callback)
        {
            Command = new RenameFolderCommand(folder, newName, null);
            ProcessorType = typeof (RenameFolderProcessor);
        }
    }

    internal class RenameFolderCommand : BaseCommand
    {
        protected override bool ValidateParameters()
        {
            return true;
        }

        public RenameFolderCommand(IFolder folder, string newName, CommandDataReceivedCallback callback)
            : base(callback)
        {
            string fullNewName = folder.Parent == null
                                     ? newName
                                     : String.Format("{0}/{1}", folder.Parent.FullPath, newName);

            CommandString = String.Format("RENAME \"{0}\" \"{1}\"", folder.FullPath, fullNewName);
        }
    }

    internal class RenameFolderProcessor : BaseProcessor
    {
        public override void ProcessResult()
        {
            base.ProcessResult();
        }
    }
}
