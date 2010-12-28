using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    
    [CommandInfo("printresult","Shows the results of the last command","printresult [errors]")]
    public class PrintResultCommand : BaseCommand
    {
        public PrintResultCommand(IMAPShell.Shell.IMAPShell shell, string[] args)
            : base(shell, "printresult", args)
        {
            
        }

        public override IMAPShell.Shell.CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            bool errorsOnly = (Args.Length > 0 && Args[0].Equals("errors"));
            
            foreach (string logLine in Shell.Client.Aggregator.LogEntries)
            {
                bool thisLineIsError = false;
                string colorCode = "^07:00";
                if (logLine.Contains("WARN"))
                    colorCode = "^14:00";

                if (logLine.Contains("ERROR"))
                {
                    colorCode = "^12:00";
                    thisLineIsError = true;
                }

                if (logLine.Contains("INFO"))
                    colorCode = "^11:00";

                if (errorsOnly && thisLineIsError)
                    ColorConsole.WriteLine("{0}{1}", colorCode, logLine);
                else if (!errorsOnly)
                    ColorConsole.WriteLine("{0}{1}", colorCode, logLine);
            }

            return result;
        }
    }
}
