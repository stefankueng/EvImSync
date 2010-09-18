using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;

namespace IMAPShell.Commands
{
    [CommandInfo("password","Change password for the current configuration","password")]
    public class PasswordCommand : BaseCommand
    {
        public PasswordCommand(IMAPShell.Shell.IMAPShell shell, string[] args)
            : base(shell, "password", args)
        {
            
        }

        public override IMAPShell.Shell.CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);


            Console.Write("Enter new password: ");
            string password = PasswordInput.ReadPassword();
            Console.Write("Confirm password: ");
            string confirm = PasswordInput.ReadPassword();
            if (password.CompareTo(confirm)==0)
            {
                result.SuccessMessage = "Password has been updated.";
                Shell.Config.Password = password;
                Shell.PrintConfig();
                return result;
            }
            else
            {                
                return CommandResult.CreateError(Command, Args, "Passwords do not match. Please try again.");
            }

            return result;
        }
    }
}
