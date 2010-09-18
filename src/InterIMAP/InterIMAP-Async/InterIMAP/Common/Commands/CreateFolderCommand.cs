using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Commands
{
    public class CreateFolderCommand : BaseCommand
    {
        protected override bool ValidateParameters()
        {
            return true;
        }

        public CreateFolderCommand(string folderName, IFolder parentFolder, CommandDataReceivedCallback callback)
            : base(callback)
        {
            string fullFolder = parentFolder != null
                                    ? String.Format("{0}/{1}", parentFolder.FullPath, folderName)
                                    : folderName;

            fullFolder = fullFolder.Replace("\"", "");

            _parameters.Add(fullFolder);
            CommandString = String.Format("CREATE \"{0}\"", fullFolder);
        }
    }
}
