using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    [CommandInfo("ssl", "Indicates whether SSL should be used", "ssl [<on|off>] ")]
    public class SSLCommand : BaseCommand
    {
        public SSLCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "ssl", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Args.Length == 0)
            {
                Shell.Config.UseSSL = !Shell.Config.UseSSL;
                Shell.PrintConfig();
                Shell.Client.Stop();
                result.SuccessMessage = "Configuration has been updated.";
            }
            else
            {
                string state = Args[0].ToLower();
                if (state.Equals("on"))
                    Shell.Config.UseSSL = true;
                else if (state.Equals("off"))
                    Shell.Config.UseSSL = false;
                else
                {
                    return CommandResult.CreateError(Command, Args,
                                                     String.Format("Did not understand state '{0}'", state));
                }

                Shell.PrintConfig();
                Shell.Client.Stop();
                result.SuccessMessage = "Configuration has been updated.";

                
            }

            return result;
        }
    }
}