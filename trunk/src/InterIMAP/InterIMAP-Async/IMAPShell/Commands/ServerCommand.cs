using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    
    [CommandInfo("server", "Change the server in the currently loaded configuration", "server <new server name>")]
    public class ServerCommand : BaseCommand
    {
        public ServerCommand(IMAPShell.Shell.IMAPShell shell, string[] args):
            base (shell,"server", args)
        {
            
        }

        public override IMAPShell.Shell.CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Args.Length <= 0)
            {                
                return CommandResult.CreateError(Command, Args, "You must specify a server name.");
            }

            Shell.Config.Host = Args[0];
            result.Type = ResultType.Success;
            result.SuccessMessage = "Server configuration updated. Please reconnect.";

            Shell.PrintConfig();
            Shell.Client.Stop();

            return result;
        }
    }
}
