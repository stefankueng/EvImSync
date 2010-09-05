using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;
using InterIMAP.Common.Interfaces;

namespace IMAPShell.Commands
{
    [RequiresConnection]
    [CommandInfo("dir", "List sub folders within the current folder", "dir")]
    public class FolderListCommand : BaseCommand
    {
        public FolderListCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "dir", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            IFolder cFolder = Shell.CurrentFolder;
            IFolder[] subFolders = Shell.Client.MailboxManager.GetChildFolders(cFolder);
            
            
            
            ColorConsole.WriteLine("\n^15:00 {0} {1}     {2}^07:00", ("Name").PadRight(30), ("Unseen").PadRight(2), ("Exists").PadRight(5));
            ColorConsole.Write("^08:00{0}", new string('-', Console.BufferWidth));
            foreach (IFolder f in subFolders)
            {
                ColorConsole.WriteLine("^07:00{0}{1} {2}     {3}", f.SubFolders.Length > 0 ? "+":" ", f.Name.PadRight(30), f.Unseen.ToString().PadLeft(6), f.Exists.ToString().PadLeft(6));
            }

            Console.WriteLine();

            return result;
        }
    }
}