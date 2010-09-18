using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{
    public class DeleteFolderRequest : BaseRequest
    {
        public DeleteFolderRequest(IFolder folder, RequestCompletedCallback callback)
            : base(callback)
        {
            /*
             * Process for deleting a folder:
             * the IMAP spec states that deleting a folder should NOT delete any sub folders.
             * So in that case we must rename any and all sub folders to include the name of the 
             * parent folder that is being deleted. for example:
             * 
             * INBOX/Sub1/SubSub1
             *           /SubSub2
             *           /SubSub3
             *           
             * if we delete Sub1, Sub1's sub folders become:
             * 
             * INBOX/Sub1_SubSub1
             * INBOX/Sub1_SubSub2
             * INBOX/Sub1_SubSub3
             * 
             * This request is ONLY for deleting the folder on the server. A higher level construct
             * is needed to facilitate the renaming of sub folders.
             * 
             * */
            Command = new DeleteFolderCommand(folder, null);
            ProcessorType = typeof (DeleteFolderProcessor);
        }
    }

    internal class DeleteFolderCommand : BaseCommand
    {
        protected override bool ValidateParameters()
        {
            return true;
        }

        public DeleteFolderCommand(IFolder deadFolder, CommandDataReceivedCallback callback)
            : base(callback)
        {
            CommandString = String.Format("DELETE \"{0}\"", deadFolder.FullPath);
        }
    }

    internal class DeleteFolderProcessor : BaseProcessor
    {
        
    }
}
