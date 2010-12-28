using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;
using InterIMAP.Common.Interfaces;

namespace IMAPShell.Commands
{
    [RequiresConnection]
    [CommandInfo("rmdir", "Removes the specified directory", "rmdir <folder name>")]
    public class DeleteCommand : BaseCommand
    {
        public DeleteCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "rmdir", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Args.Length == 0)
            {                
                return CommandResult.CreateError(Command, Args, "You must specify a folder to delete");
            }

            
            IFolder folderToKill = null;
            string folderToFind = Args.Length > 1 ? string.Join(" ", Args) : Args[0];

            folderToFind = folderToFind.Trim('"');

            folderToKill = Shell.FindFolder(folderToFind);

            if (folderToKill == null)
            {

                return CommandResult.CreateError(Command, Args,
                                                 String.Format("Could not find folder '{0}'", folderToFind));
            }

            Shell.Client.MailboxManager.DeleteFolder(folderToKill);

            return result;
        }
    }
}