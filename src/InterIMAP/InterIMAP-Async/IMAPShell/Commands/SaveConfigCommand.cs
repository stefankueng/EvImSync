using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    [CommandInfo("saveconfig","Saves the current configuration info", "saveconfig [<config file>]")]
    public class SaveConfigCommand : BaseCommand
    {
        public SaveConfigCommand(IMAPShell.Shell.IMAPShell shell, string[] args)
            : base(shell,"saveconfig", args)
        {
            
        }

        public override IMAPShell.Shell.CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Shell.Config.ConfigFile == null)
            {
                if (Args.Length == 0)
                {                    
                    return CommandResult.CreateError(Command, Args, "You must specify a config file to save to");
                }
                else
                {
                    Shell.Config.ConfigFile = Args[0];
                }
            }

            Shell.Config.SaveConfig();
            result.SuccessMessage = String.Format("Configuration has been saved to {0}", Shell.Config.ConfigFile);

            return result;
        }
    }
}
