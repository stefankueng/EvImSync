using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    
    [CommandInfo("printconfig", "Show the current configuration information", "printconfig [-p]\n\n-p: Shows password in plain text")]
    public class PrintConfigCommand : BaseCommand
    {
        public PrintConfigCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "printconfig", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Args.Length == 0)
            {
                Shell.PrintConfig();
                return result;
            }

            if (Args[0].Equals("-p"))
            {
                Shell.PrintFullConfig(true);
                return result;
            }
            else
            {
                return CommandResult.CreateError(Command, Args,
                                                 String.Format("Invalid parameters: {0}", string.Join(" ", Args)));
            }

            return result;
        }
    }
}