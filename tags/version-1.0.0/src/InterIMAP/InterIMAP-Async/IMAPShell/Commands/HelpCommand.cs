using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    
    [CommandInfo("help", "Print this help message to the screen", "help <command name>\tDisplay usage information for the specified command")]
    public class HelpCommand : BaseCommand
    {
        public HelpCommand(IMAPShell.Shell.IMAPShell shell, string[] args)
            : base(shell, "help", args)
        {
            
        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Args != null && Args.Length > 0)
            {
                string cmd = Args[0];
                if (Shell.AvailableCommands.Contains(cmd))
                {
                    Shell.PrintHelp(Args[0]);
                    return result;
                }
                else
                {                    
                    return CommandResult.CreateError(Command, Args, String.Format("Command '{0}' not recognized", cmd));
                }
            }
            
            Shell.PrintHelp(null);

            return result;
        }
    }
}
