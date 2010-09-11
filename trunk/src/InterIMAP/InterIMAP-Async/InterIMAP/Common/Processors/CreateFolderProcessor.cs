using System;
using System.Collections.Generic;
using System.Text;

namespace InterIMAP.Common.Processors
{
    public class CreateFolderProcessor : BaseProcessor
    {
        private bool _folderCreated;

        public bool FolderCreated { get { return _folderCreated; } }
        
        public override void ProcessResult()
        {
            base.ProcessResult();

            if (CmdResult.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE)
            {
                _folderCreated = true;
            }
        }
    }
}
