using System;
using System.Net.Mail;
using InterIMAP;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Requests;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Processors;

namespace InterIMAPConsoleTest
{
    class Program
    {
        static void FolderTreeCallback(IRequest req)
        {
            Console.WriteLine();
            FolderListProcessor flp = req.GetProcessorAsType<FolderListProcessor>();
            
        }

        
        static void Main(string[] args)
        {
            IMAPConfig config = new IMAPConfig("imap.gmail.com", "atmospherian", "Xr3pr1s3Y", true, true, "");
            config.SaveConfig(@"c:\settings.cfg");
            //IMAPConfig config = new IMAPConfig(@"c:\test1.cfg");
            IMAPAsyncClient client = new IMAPAsyncClient(config, 5);

            client.MailboxManager.CreateNewMailbox(@"c:\test.mbx");

            
            client.Start();
            FolderTreeRequest ftr = new FolderTreeRequest("\"\"", null);
            client.RequestManager.SubmitAndWait(ftr,false);

            IBatchRequest batch = new SimpleBatchRequest();
            foreach (IFolder folder in client.MailboxManager.GetAllFolders())
            {
                FolderDataRequest fdr = new FolderDataRequest(folder, null);
                fdr.RequestCompleted += delegate(IRequest req)
                                            {
                                                FolderDataProcessor fdp = req.GetProcessorAsType<FolderDataProcessor>();
                                                IFolder f = fdp.Request.Command.ParameterObjects[0] as IFolder;
                                                Console.WriteLine("Data for {0} loaded. {1} Messages found.", f.Name, f.Exists);
                                            };
                batch.Requests.Add(fdr);
            }
           
            client.RequestManager.SubmitBatchAndWait(batch, false);
            batch.Requests.Clear();
            foreach (IFolder folder in client.MailboxManager.GetAllFolders())
            {
                MessageListRequest mlr = new MessageListRequest(folder, null);
                mlr.RequestCompleted += delegate(IRequest req)
                                            {
                                                MessageListProcessor fdp = req.GetProcessorAsType<MessageListProcessor>();
                                                IFolder f = fdp.Request.Command.ParameterObjects[0] as IFolder;
                                                Console.WriteLine("Message List for {0} loaded. {1} Messages found.", f.Name, f.Exists);
                                            };

                batch.Requests.Add(mlr);
            }

            client.RequestManager.SubmitBatchAndWait(batch, false);

            client.MailboxManager.DownloadEntireAccount(delegate(int messagesCompleted, int totalMessages, IFolder currentFolder)
                                                            {
                                                                Console.WriteLine();
                                                                Console.WriteLine("Message {0} of {1} downloaded from {2}", messagesCompleted, totalMessages, currentFolder.Name);
                                                            }, delegate(int totalFolders, int totalMessages, long totalTime)
                                                                   {
                                                                       Console.WriteLine("{0} Messages in {1} folders downloaded in {2} minutes.", totalMessages, totalFolders, new TimeSpan(totalTime).Minutes);
                                                                   });
            
            
            //config.CacheFi

            Console.WriteLine("Press any key");
            Console.ReadLine();

            
            
            client.Stop();
        }
    }
}
