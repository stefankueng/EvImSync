using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;
using InterIMAP.Common.Requests;

namespace IMAPShell.Commands
{
    [RequiresConnection]
    [CommandInfo("mkdir", "Create a new folder", "mkdir <folder name>")]
    public class CreateFolderCommand : BaseCommand
    {
        public CreateFolderCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "mkdir", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Args.Length == 0)
            {                
                return CommandResult.CreateError(Command, Args, "You must specify a folder name");
            }

            if (Regex.Match(Args[0], "[\\\\/\\.]+").Success)            
            {                
                return CommandResult.CreateError(Command, Args, "Folder name cannot contain characters '\\' '/' '.'");
            }

            
            CreateFolderRequest cfr = new CreateFolderRequest(Args[0],Shell.CurrentFolder,
                delegate(IRequest req)
                    {
                        CreateFolderProcessor cfp = req.GetProcessorAsType<CreateFolderProcessor>();
                        if (cfp.FolderCreated)
                        {
                            IFolder parentFolder = Shell.CurrentFolder;
                            Shell.Client.MailboxManager.AddFolder(Args[0], parentFolder);
                        }
                        
                    });

            Shell.Client.RequestManager.SubmitAndWait(cfr, false);

            


            return result;
        }
    }
}