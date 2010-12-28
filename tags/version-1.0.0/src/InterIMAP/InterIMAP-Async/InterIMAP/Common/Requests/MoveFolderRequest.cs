using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{
    public class MoveFolderRequest : BaseRequest
    {
        public MoveFolderRequest(IFolder folder, IFolder parentFolder, RequestCompletedCallback callback)
            : base(callback)
        {
            Command = new MoveFolderCommand(folder, parentFolder, null);
            ProcessorType = typeof(MoveFolderProcessor);
        }
    }

    internal class MoveFolderCommand : BaseCommand
    {
        protected override bool ValidateParameters()
        {
            return true;
        }

        public MoveFolderCommand(IFolder folder, IFolder parentFolder, CommandDataReceivedCallback callback)
            : base(callback)
        {
            string newFolder = parentFolder == null
                                   ?
                                       folder.Name
                                   : String.Format("{0}/{1}", parentFolder.FullPath, folder.Name);

            CommandString = String.Format("RENAME \"{0}\" \"{1}\"", folder.FullPath, newFolder);
        }
    }

    internal class MoveFolderProcessor : BaseProcessor
    {

    }
}
