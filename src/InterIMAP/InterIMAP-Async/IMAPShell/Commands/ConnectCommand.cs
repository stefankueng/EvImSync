using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Interfaces;
using IMAPShell.Shell;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Requests;

namespace IMAPShell.Commands
{
    
    [CommandInfo("connect", "Connect to the server specified in the configuration", "connect [<num workers>]: Default number of workers is 5")]
    public class ConnectCommand : BaseCommand
    {
        
        private bool foldersPopulated = false;
        
        public ConnectCommand(IMAPShell.Shell.IMAPShell shell, string[] args):
            base (shell,"connect", args)
        {
            
        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);
            if (Shell.Client.ConnectionPool.AnybodyAlive())
            {
                return CommandResult.CreateError(Command, Args,
                                                 String.Format("Already connected to {0}", Shell.Config.Host));
            }

            int numWorkers = 5;
            if (Args != null && Args.Length > 0)
            {
                numWorkers = Convert.ToInt32(Args[0]);
            }
            Shell.Client.NumberOfWorkers = numWorkers;
            Shell.Client.Aggregator.ClearLogs();
            //ColorConsole.ResetAfterWrite = false;
            ColorConsole.Write("^03:00Attempting connection to {0}... ", Shell.Config.Host);
            result.Type = Shell.Client.Start() ? ResultType.Success : ResultType.Error;
            if (result.Type == ResultType.Success)
            {
                //result.SuccessMessage = String.Format("Successfully connected to {0}", Shell.Config.Host);
                ColorConsole.WriteLine("^11:00Connected");
                ColorConsole.Write("^03:00Getting folder list... ");
                Shell.Client.RequestManager.SubmitAndWait(new FolderTreeRequest("\"\"", FolderTreeCallback), false);                
                ColorConsole.Write("^03:00Populating folder data... ");
                Shell.Client.MailboxManager.PopulateFolderData(null, FolderDataCallback);
                while (!foldersPopulated) { Thread.Sleep(1); }
                
                return result;
            }
            else if (result.Type == ResultType.Error)
            {
                //ColorConsole.WriteLine("^12:00Failed");
                result.ErrorMessage =
                    String.Format(
                        "The was a problem connecting to {0}. Please check your settings and connection and try again.",
                        Shell.Config.Host);
            }

            return result;
        }

        private void FolderDataCallback(IBatchRequest req)
        {
            foldersPopulated = true;
            ColorConsole.WriteLine("^11:00Done\n");
        }

        private void FolderTreeCallback(IRequest req)
        {
            
            ColorConsole.WriteLine("^11:00Done");
        }
    }
}
