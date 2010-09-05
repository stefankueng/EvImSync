using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP;
using System.Collections;
using System.Threading;

namespace InterIMAPConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //IMAPConfig config = new IMAPConfig("<host>", "<username>", "<password>", false, true, "");
            //config.SaveConfig("settings.cfg");
            IMAPConfig config = new IMAPConfig(@"c:\test1.cfg");
            config.CacheFile = "";
            
            IMAPClient client = null;
            try
            {
                client = new IMAPClient(config, null, 5);
            }
            catch (IMAPException e)
            {
                Console.WriteLine(e.Message);
                return;
            }


            //IMAPFolder drafts = client.Folders["Drafts"];

            //IMAPMessage newMessage = new IMAPMessage();
            //newMessage.From.Add(new IMAPMailAddress("Jason Miesionczek", "atmospherian@gmail.com"));
            //newMessage.To.Add(new IMAPMailAddress("Jason Miesionczek", "jason@reprisemedia.com"));
            //newMessage.Date = DateTime.Now;
            //newMessage.Subject = "this is a new message";
            //drafts.AppendMessage(newMessage, "this is the content of the new message");
            //IMAPFolder f = client.Folders["INBOX"];
            //Console.WriteLine(f.Messages.Count);
            //IMAPMessage msg = f.GetMessageByID(21967);
            //Console.WriteLine(msg.TextData.Data);
            //foreach (IMAPFileAttachment attachment in msg.Attachments)
            //{
            //    attachment.SaveFile("C:\\");
            //}
            //Console.ReadLine();
            
            IMAPFolder f = client.Folders["INBOX"];

            int[] msgCount = null;

            while (msgCount.Length == 0)
            {
                msgCount = f.CheckForNewMessages();
                Thread.Sleep(1000);
            }

            foreach (int id in msgCount)
            {
                IMAPMessage msg = f.GetMessageByID(id);
                // do some logic here
                msg.MarkAsRead();
            }

            //IMAPFolder f = client.Folders["Deleted Items"];
            //IMAPMessage m = f.GetMessageByID(707);
            //IMAPFolder d = client.Folders["Deleted Items"];
            //IMAPMessage m = d.Messages[0];
            //IMAPMessage m = f.GetMessageByID(375);
            //IMAPMessage m = f.Messages[0];
            //m.RefreshData(true, true);
            
            //client._imap.ProcessMessageHeader(m, 0); // 2893
            //client._imap.ProcessBodyStructure(m);
            //client._imap.ProcessMessageHeader(m, 0);
            //client._imap.ProcessBodyStructure(m);
            //client._imap.ProcessBodyParts(m);
            //client._imap.ProcessAttachments(m);
            //IMAPSearchQuery query = new IMAPSearchQuery();
            //query.Range = new DateRange(DateTime.Now.AddDays(-6), DateTime.Now);
            //IMAPSearchResult sResult = f.Search(query);

            //IMAPSearchResult sResult = f.Search(IMAPSearchQuery.QuickSearchDateRange(DateTime.Now.AddDays(-6), DateTime.Now));
            //IMAPSearchResult sResult = f.Search(IMAPSearchQuery.QuickSearchFrom("Christine Fade", "cfade@reprisemedia.com"));
            //IMAPSearchResult sResult = f.Search(IMAPSearchQuery.QuickSearchNew());
            
            //IMAPFolder test = f.SubFolders["Test"];
            //IMAPFolder del = client.Folders["Deleted Items"];
            //f.CopyMessageToFolder(f.Messages[0], test);
            //test.DeleteMessage(test.Messages[0]);
            //f.MoveMessageToFolder(f.Messages[0], test);
            
            //test.EmptyFolder();
            //Console.WriteLine("{0} - {1}", sResult.Query.Range.StartDate, sResult.Query.Range.EndDate);
            //foreach (IMAPMessage msg in sResult.Messages)
            //{
            //    msg.RefreshData(true, true, false);
            //    Console.WriteLine("{0}: {1}", msg.Date, msg.Subject);
            //    Console.WriteLine(msg.TextData.Data);
            //}
            //m.Attachments[1].SaveFile("C:\\");
            Console.ReadLine();
            foreach (IMAPMessage msg in client.Folders["INBOX"].Messages)
            {
                if (msg.BodyParts.Count == 0)
                    Console.WriteLine(msg.Uid);
            }
            client.Logoff();
        }
    }
}
