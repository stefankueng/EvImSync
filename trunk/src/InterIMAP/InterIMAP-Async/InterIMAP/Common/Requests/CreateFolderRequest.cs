using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{
    public class CreateFolderRequest : BaseRequest
    {
        public CreateFolderRequest(string folderName, IFolder parentFolder, RequestCompletedCallback callback)
            : base(callback)
        {
            Command = new CreateFolderCommand(folderName, parentFolder, null);
            ProcessorType = typeof (CreateFolderProcessor);
        }
    }
}
