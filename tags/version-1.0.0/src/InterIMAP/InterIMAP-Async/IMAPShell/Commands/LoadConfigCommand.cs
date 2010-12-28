using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;
using InterIMAP;

namespace IMAPShell.Commands
{
    [CommandInfo("loadconfig", "Loads configuration info from a file", "loadconfig <config file>")]
    public class LoadConfigCommand : BaseCommand
    {
        public LoadConfigCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "loadconfig", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Args.Length == 0)
            {                
                return CommandResult.CreateError(Command, Args, "You must specify a config file to load");
            }

            string file = Args[0];
            if (File.Exists(file))
            {
                Shell.Client.Config = new IMAPConfig(file);
                Shell.Config = Shell.Client.Config;
                Shell.Client.Stop();
                Shell.PrintConfig();
                result.SuccessMessage = "Configuration loaded. Please re-connect.";
            }
            else
            {                
                return CommandResult.CreateError(Command, Args, String.Format("Could not find file '{0}'", file));
            }

            return result;
        }
    }
}