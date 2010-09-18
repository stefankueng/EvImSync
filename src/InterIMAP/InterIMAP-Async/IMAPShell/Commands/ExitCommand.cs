using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    
    [CommandInfo("exit", "Exits the shell","exit")]
    public class ExitCommand : BaseCommand
    {
        public ExitCommand(IMAPShell.Shell.IMAPShell shell, string[] args)
            : base(shell, "exit", args)
        {
            
        }

        public override IMAPShell.Shell.CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            result.Type = ResultType.Exit;

            return result;
        }
    }
}
