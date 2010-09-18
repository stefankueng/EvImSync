using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    
    [CommandInfo("clear", "Clears the screen", "clear")]
    public class ClearCommand : BaseCommand
    {
        public ClearCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "clear", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            Shell.ClearScreen();

            return result;
        }
    }
}