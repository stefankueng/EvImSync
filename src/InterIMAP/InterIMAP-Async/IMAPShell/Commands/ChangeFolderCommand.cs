using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Requests;

namespace IMAPShell.Commands
{
    [RequiresConnection]
    [CommandInfo("cd", "Change to the specified folder", "cd <folder name>")]
    public class ChangeFolderCommand : BaseCommand
    {
        public ChangeFolderCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "cd", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Args.Length == 0)
            {
                string folderName = null;
                folderName = Shell.CurrentFolder == null ? "/" : Shell.CurrentFolder.FullPath;
                Console.WriteLine(folderName);
                return result;
            }

            string newFolderName = null;
            IFolder newFolder = null;

            if (Args.Length > 1) // if folder name has spaces, combine separate args into one folder name
            {
                newFolderName = string.Join(" ", Args);
                    
            }
            else
            {
                newFolderName = Args[0];
            }

            if (newFolderName.Equals("."))
            {                
                return result;
            }

            if (newFolderName.Equals(".."))
            {
                if (Shell.CurrentFolder != null)
                {
                    if (Shell.CurrentFolder.ParentID > -1)
                    {
                        newFolder = Shell.Client.MailboxManager.GetFolderByID(Shell.CurrentFolder.ParentID);
                        SetCurrentFolder(newFolder);
                        return result;
                    }
                    else
                    {
                        SetCurrentFolder(null);
                        return result;
                    }

                }
                
            }

            // first we check if the specified folder is a sub folder of the current folder
            
            foreach (IFolder folder in (Shell.CurrentFolder == null ? Shell.Client.MailboxManager.Folders : Shell.CurrentFolder.SubFolders))
            {
                if (folder.Name.Equals(newFolderName) || folder.FullPath.Equals(newFolderName))
                {
                    newFolder = folder;
                    SetCurrentFolder(newFolder);
                    return result;
                }
            }

            // if we didn't find it there, then we check all folders, compile a list of all folders
            // that match the file name regardless of full path, and let the user chose which folder
            // they would like to select. If the result is only 1, then we just use that.
            
            List<IFolder> matchingFolderList = new List<IFolder>();
            foreach (IFolder folder in Shell.Client.MailboxManager.GetAllFolders())
            {
                if (!folder.Name.Equals(newFolderName) && !folder.FullPath.Equals(newFolderName)) continue;
                matchingFolderList.Add(folder);
            }

            if (matchingFolderList.Count == 1)
            {
                newFolder = matchingFolderList[0];
                SetCurrentFolder(newFolder);
                return result;
            }
            else
            {
                SetCurrentFolder(PromptMatchingFolders(matchingFolderList.ToArray()));
            }
           

            return result;
        }

        private void SetCurrentFolder(IFolder folder)
        {
            Shell.CurrentFolder = folder;
            if (folder == null) return;
            //bool done = false;
            Shell.Client.RequestManager.SubmitAndWait(new MessageListRequest(folder,null), false);
            //while (!done) { Thread.Sleep(1);}
        }

        private IFolder PromptMatchingFolders(IFolder[] folderList)
        {
            IFolder folderToSelect = null;
            Dictionary<int, IFolder> _folderNumMap = new Dictionary<int, IFolder>();
            
            Console.WriteLine("\nPlease enter the number of the folder to change to: \n");
            for (int i = 0; i < folderList.Length; i++)
            {
                _folderNumMap.Add(i+1, folderList[i]);                
                ColorConsole.WriteLine("^08:00[^15:00{0}^08:00] ^07:00{1}", i+1, folderList[i].FullPath);                
            }

            
            while (true)
            {
                Console.Write("\nSelect folder: ");
                string folderInput = Console.ReadLine();

                int folderNum = GetInputNumber(folderInput);
                if (folderNum == 0 || (folderNum < 1 && folderNum > folderList.Length+1))
                {
                    ColorConsole.WriteLine("^12:00Invalid entry");

                }
                else
                {
                    return _folderNumMap[folderNum];
                }

            }            
        }

        private int GetInputNumber(string input)
        {
            int num = 0;
            if (Int32.TryParse(input, out num))
            {
                return num;
            }

            return num;
        }
        
    }
}