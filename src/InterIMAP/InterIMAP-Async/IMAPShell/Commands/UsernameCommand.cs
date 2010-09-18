using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    
    [CommandInfo("username", "Change the username in the currently loaded configuration", "username <new username>")]
    public class UsernameCommand : BaseCommand
    {
        public UsernameCommand(IMAPShell.Shell.IMAPShell shell, string[] args):
            base(shell, "username", args)
        {
            
        }

        public override IMAPShell.Shell.CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);
            
            if (Args.Length <= 0)
            {                
                return CommandResult.CreateError(Command, Args, "You must specify a username.");
                
            }

            Shell.Config.UserName = Args[0];
            result.Type = ResultType.Success;
            result.SuccessMessage = "Successfully updated comfiguration. Please reconnect.";

            Shell.PrintConfig();
            Shell.Client.Stop();

            return result;
        }
    }
}
