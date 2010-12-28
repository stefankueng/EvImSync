using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IMAPShell.Attributes;
using IMAPShell.Helpers;
using IMAPShell.Shell;
using InterIMAP;
using InterIMAP.Common;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;
using InterIMAP.Common.Requests;

namespace IMAPShell.Commands
{
    [RequiresConnection]
    [CommandInfo("list", "Lists the messages in the current folder", "list [asc|desc (default)]")]
    public class MessageListCommand : BaseCommand
    {
        private const int MSGS_PER_BLOCK = 20;
        private object _lockObj = new object();
        
        public MessageListCommand(Shell.IMAPShell shell, string[] args)
            : base(shell, "list", args)
        {

        }

        public override CommandResult Execute()
        {
            CommandResult result = new CommandResult(Command, Args);
            /*
             * Here is where things get interesting. In order to make this command work as efficiently 
             * as possible the best route to take is to download and display the header data for messages 
             * 20 or so at a time. After each group the user can choose to get the next group, go back to
             * the previous group, view details of a specific message, or cancel.
             * 
             * Possible arguments
             * 
             * -s <num>     message number to start with
             * -e <num>     message number to end with
             * 
             * */
            if (Shell.CurrentFolder == null)
            {                
                return CommandResult.CreateError(Command, Args, "This folder does not contain any message data.");
            }
            Shell.Client.Aggregator.ClearLogs();
            Arguments processedArgs = new Arguments(Args);

            // first we have to get all the UIDs for the messages in the current folder
            bool messageListComplete = false;
            Shell.Client.RequestManager.SubmitRequest(
                new MessageListRequest(Shell.CurrentFolder, delegate
                                                              {
                                                                  messageListComplete = true;
                                                              }),false);
            while (!messageListComplete) {}
            MessageListDirection direction = MessageListDirection.Descending;
            if (Args.Length > 0)
            {
                if (Args[0].Equals("desc"))
                    direction = MessageListDirection.Descending;
                else if (Args[0].Equals("asc"))
                    direction = MessageListDirection.Ascending;
            }

            IMessage[] msgList = Shell.Client.MailboxManager.GetMessagesByFolder(Shell.CurrentFolder, direction);

            if (msgList.Length == 0)
            {
                result.SuccessMessage = "This folder does not contain any messages.";
                return result;
            }

            bool view;
            bool del;
            int uid;
            DoMessageGroups(msgList, out view, out del, out uid);

            if (view)
            {
                ColorConsole.WriteLine("\nViewing message {0}\n", uid);
            }

            if (del)
            {
                DeleteMessage(uid);
            }

            return result;
        }

        private void DeleteMessage(int uid)
        {
            ColorConsole.Write("\n^05:00Deleting message {0}... ", uid);
            IMessage msg = Shell.Client.MailboxManager.GetMessageByUID(uid, Shell.CurrentFolder.ID);
            
            DeleteMessageRequest dmr = new DeleteMessageRequest(msg, 
                delegate(IRequest req)
                    {
                        if (req.Result.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE)
                        {
                            if (Shell.Client.MailboxManager.RemoveMessage(uid, Shell.CurrentFolder.ID))
                                ColorConsole.WriteLine("^13:00Done\n");
                        }

                        
                    });
            Shell.Client.Aggregator.ClearLogs();
            Shell.Client.RequestManager.SubmitAndWait(dmr, false);
            
        }

        private int DoMessageGroups(IMessage[] msgList, out bool view, out bool del, out int uid)
        {
            bool cancel = false;
            view = false;
            del = false;
            uid = 0;
            int startNum = 0;
            int endNum = msgList.Length-1;
            int currentGroup = 1;
            int maxGroups = (int)Math.Ceiling((double)msgList.Length/MSGS_PER_BLOCK);
            int detailNum = 0;
            while (!cancel)
            {
                ShowMessageGroup(msgList, startNum);
                cancel = GroupPrompt(ref startNum, ref currentGroup, maxGroups, out view, out del, out uid);
            }
            
            return detailNum;
        }

        private bool GroupPrompt(ref int startNum,ref int currentGroup, int maxGroups, out bool view, out bool del, out int uid)
        {            
            bool validCommand = false;
            while (!validCommand)
            {
                ColorConsole.Write("\n^08:00[^07:00Group ^15:00{0}^07:00/{1}^08:00] ^08:00(^15:00n^08:00)^07:00ext, ^08:00(^15:00p^08:00)^07:00rev, ^08:00(^15:00d^08:00)^07:00elete, ^08:00(^15:00v^08:00)^07:00iew, ^08:00(^15:00c^08:00)^07:00ancel: ", currentGroup, maxGroups);
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();
                switch (key.Key)
                {
                    case ConsoleKey.N:
                        validCommand = true;
                        if (currentGroup == maxGroups)
                        {
                            validCommand = false;
                            break;
                        }
                        currentGroup++;
                        startNum += MSGS_PER_BLOCK;
                        break;
                    case ConsoleKey.P:
                        validCommand = true;
                        if (currentGroup == 1)
                        {
                            validCommand = false;
                            break;
                        }
                        currentGroup--;
                        startNum -= MSGS_PER_BLOCK;
                        break;
                    case ConsoleKey.V:
                        validCommand = true;
                        view = true;
                        del = false;
                        uid = GetUID("Enter UID of message to view: ");
                        if (uid == 0) view = false;
                        return true;
                    case ConsoleKey.D:
                        validCommand = true;
                        view = false;
                        del = true;
                        uid = GetUID("Enter UID of message to delete: ");
                        if (uid == 0) del = false;
                        return true;
                    case ConsoleKey.C:
                        validCommand = true;
                        view = false;
                        del = false;
                        uid = 0;
                        return true;
                        
                }
            }
            view = false;
            del = false;
            uid = 0;
            return false;
        }

        private int GetUID(string promptTxt)
        {            
            while (true)
            {
                Console.Write("\n{0}",promptTxt);
                string input = Console.ReadLine();

                if (String.IsNullOrEmpty(input))
                    return 0;

                int uid;
                if (Int32.TryParse(input, out uid))
                    return uid;
                
            }
        }

        private void ShowMessageGroup(IMessage[] msgList, int startNum)
        {
                                         //*UID(5)   From(15)      Subject(30)       Date
            ColorConsole.WriteLine("\n^15:00 UID   From            Subject                        Date");
            ColorConsole.Write("^08:00{0}", new string('-', Console.BufferWidth));

            int msgCount = 0;
            int blockSize = MSGS_PER_BLOCK;
            for (int i = startNum; i < startNum+MSGS_PER_BLOCK;i++)
            {
                if (i == msgList.Length)
                {
                    int maxGroups = msgList.Length/MSGS_PER_BLOCK;
                    blockSize = msgList.Length - (maxGroups*MSGS_PER_BLOCK);
                    break;
                }
                IMessage msg = msgList[i];
                if (!msg.HeaderLoaded)
                {
                    bool headerLoaded = false;
                    Shell.Client.RequestManager.SubmitRequest(new MessageFlagRequest(msg, delegate
                                                                                          {
                                                                                              Shell.Client.RequestManager.SubmitRequest(new MessageHeaderRequest(msg, delegate(IRequest req)
                                                                                              {
                                                                                                  //headerLoaded = true;
                                                                                                  MessageHeaderProcessor mhp = req.GetProcessorAsType<MessageHeaderProcessor>();
                                                                                                  PrintMessage(mhp.Message);
                                                                                                  msgCount++;

                                                                                              }), false);
                                                                                          }),false);
                    
                    
                }
                else
                {
                    PrintMessage(msg);
                    msgCount++;
                }
                
            }
            while (msgCount < blockSize) { Thread.Sleep(1); }
        }

        private void PrintMessage(IMessage msg)
        {
            lock (_lockObj)
            {
                string subject = TruncateString(msg.Subject, 30);
                string from = TruncateString(msg.FromContacts[0].ToString(), 15);

                ColorConsole.WriteLine("{0}{1} {2} {3} {4}", msg.Seen ? " " : "*", msg.UID.ToString().PadRight(5),
                                       from.PadRight(15), subject.PadRight(30), msg.DateReceived.ToString());
            }
        }
        
        private string TruncateString(string input, int len)
        {
            if (len < 3) throw new ArgumentOutOfRangeException("len","len needs to be greater or equal to 3");
            if (input.Length < len) return input;

            string temp = input.Remove(len - 3);
            temp = string.Concat(temp, "...");
            return temp;
        }
    }
}