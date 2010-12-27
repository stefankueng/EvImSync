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
            System.Text.StringBuilder b = new System.Text.StringBuilder(folderName);
            for (int i = 0; i < b.Length; ++i)
            {
                if (b[i] == '+')
                    b[i] = '&';
                else if (b[i] == '&')
                    b[i] = '+';
            }
            folderName = b.ToString();
            byte[] utf7String = System.Text.Encoding.UTF7.GetBytes(folderName);
            folderName = System.Text.Encoding.ASCII.GetString(utf7String);
            folderName = folderName.Replace('/', ',').Replace('+', '&');

            string fullFolder = parentFolder != null
                                    ? String.Format("{0}/{1}", parentFolder.FullPath, folderName)
                                    : folderName;

            fullFolder = fullFolder.Replace("\"", "");

            _parameters.Add(fullFolder);
            CommandString = String.Format("CREATE \"{0}\"", fullFolder);
        }
    }
}
