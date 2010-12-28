using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;
using InterIMAP.Common.Requests;
using CommandResult=IMAPShell.Shell.CommandResult;

namespace IMAPShell.Commands
{
    [RequiresConnection]
    [CommandInfo("getnew", "Checks the current folder for new messages", "getnew")]
    public class GetNewMessagesCommand : BaseCommand
    {
        public GetNewMessagesCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "getnew", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);

            if (Shell.CurrentFolder == null)
            {                
                return CommandResult.CreateError(Command, Args, "This folder does not contain any messages");
            }

            
            List<IMessage> newMsgs = new List<IMessage>();
            Shell.Client.RequestManager.SubmitAndWait(new MessageListRequest(Shell.CurrentFolder, null), false);                        
            Shell.Client.RequestManager.SubmitAndWait(new SearchRequest(Shell.CurrentFolder, new SearchCriteria(true), 
                delegate(IRequest req)
                    {
                        SearchProcessor sp = req.GetProcessorAsType<SearchProcessor>();
                        newMsgs.AddRange(sp.Messages);
                        
                    }), false);
            

            foreach (IMessage msg in newMsgs)
                Console.WriteLine(msg.UID);

            ColorConsole.WriteLine("\n^11:00{0} new message(s) found\n", newMsgs.Count);

            return result;
        }
    }
}