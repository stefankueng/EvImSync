using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;
using InterIMAP.Common;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Requests;

namespace IMAPShell.Commands
{
    [RequiresConnection]
    [CommandInfo("rename", "Rename the specified folder", "rename -o \"<source folder name>\" -n \"<new folder name>\"")]
    public class RenameCommand : BaseCommand
    {
        public RenameCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "rename", args)
        {
            
        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Args.Length == 0)
            {                
                return CommandResult.CreateError(Command, Args, "You are missing some arguments");
            }

            //Arguments args = new Arguments(Args);
            ArgumentParser parser = new ArgumentParser(Args);

            if (parser["o"] == null && parser["n"] == null)
            {
                return CommandResult.CreateError(Command, Args, "You are missing some arguments");
            }

            string sourceFolder = parser["o"].Trim(' ','"');
            string destFolder = parser["n"].Trim(' ','"');

            IFolder sourceFolderObj = Shell.FindFolder(sourceFolder);

            // if we can't find it at all, then throw an error
            if (sourceFolderObj == null)
            {
                return CommandResult.CreateError(Command, Args,
                                                 String.Format(
                                                     "The source folder '{0}' could not be found. Try specifying its full path.",
                                                     sourceFolder));
            }

            IFolder parentFolder = sourceFolderObj.Parent;
            RenameFolderRequest rfr = new RenameFolderRequest(sourceFolderObj, destFolder,
                delegate(IRequest req)
                    {
                        if (req.Result.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE)
                        {
                            Shell.Client.MailboxManager.RenameFolder(sourceFolderObj, destFolder);
                        }
                    });

            Shell.Client.RequestManager.SubmitAndWait(rfr, false);

            return result;
        }
    }
}