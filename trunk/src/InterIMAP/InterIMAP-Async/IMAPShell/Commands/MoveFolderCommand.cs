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
    [CommandInfo("mv", "Move the specified folder", "mv -f <folder name> -p <new parent folder|'/'>")]
    public class MoveFolderCommand : BaseCommand
    {
        public MoveFolderCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "mv", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            ArgumentParser parser = new ArgumentParser(Args);
            if (parser.Count < 2)
            {                
                return CommandResult.CreateError(Command, Args, "Incorrect number of arguments");
            }

            if (parser["f"] == null && parser["p"] == null)
            {                
                return CommandResult.CreateError(Command, Args, "Missing -f and/or -p arguments");
            }

            IFolder sourceFolder = Shell.FindFolder(parser["f"].Trim());
            IFolder parentFolder = Shell.FindFolder(parser["p"].Trim());

            if (sourceFolder == null)
                return CommandResult.CreateError(Command, Args, "Source folder could not be found");
            

            if (parentFolder == null && !parser["p"].Trim().Equals("/"))
                return CommandResult.CreateError(Command, Args, "Parent folder could not be found");

            if (sourceFolder == Shell.CurrentFolder)
            {
                return CommandResult.CreateError(Command, Args,
                                                 "You cannot move the current folder. Please change to a different folder first.");
            }

            Shell.Client.RequestManager.SubmitAndWait(new MoveFolderRequest(sourceFolder, parentFolder,
                delegate(IRequest req)
                    {
                        if (req.Result.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE)
                            Shell.Client.MailboxManager.MoveFolder(sourceFolder, parentFolder);
                    }),false);

            return result;
        }
    }
}