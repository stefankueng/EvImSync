using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Reflection;
using InterIMAP;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;
using InterIMAP.Common.Requests;
using InterIMAP.Common.Attributes;
using EveImSync.Enums;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace EveImSync
{
    public partial class MainFrm : Form
    {
        private IMAPAsyncClient client;
        private delegate void StringDelegate(string foo);
        private string username;
        private string password;
        private string notebook;
        private string notefolder;

        public MainFrm()
        {
            InitializeComponent();
            this.progressInfoList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AddInfoLine(string infoLine)
        {
            if (this.progressInfoList.InvokeRequired)
            {
                this.progressInfoList.Invoke(new StringDelegate(AddInfoLine),
                            new object[] { infoLine });
            }
            else
            {
                this.progressInfoList.Items.Add(infoLine);
                this.progressInfoList.Update();
            }
        }

        private void startsync_Click(object sender, EventArgs e)
        {
            username = userName.Text;
            password = passWord.Text;
            notebook = noteBook.Text;
            notefolder = notesFolder.Text;

            startsync.Enabled = false;
            MethodInvoker syncDelegate = new MethodInvoker(SyncEvernoteWithIMAP);
            syncDelegate.BeginInvoke(null, null);
        }

        public void SyncEvernoteWithIMAP()
        {
            AddInfoLine("extracting notes");
            string exportFile = ExtractNotes();
            if (exportFile != null && exportFile != "")
            {
                AddInfoLine("parsing Evernote notes");
                List<Note> notesEvernote = ParseNotes(exportFile);
                AddInfoLine(string.Format("Found {0} notes in Evernote", notesEvernote.Count));
                AddInfoLine("fetching list of emails");
                List<Note> notesIMAP = GetMailList();
                AddInfoLine(string.Format("Found {0} notes in IMAP", notesIMAP.Count));
                AddInfoLine("Analyzing notes, figuring out what notes need syncing");
                DiffNotesAndMails(ref notesEvernote, ref notesIMAP);
                AddInfoLine("Downloading emails as notes");
                DownloadAndImportMailsToEvernote(notesIMAP);
                UploadNotesAsMails(notefolder, notesEvernote, exportFile);
            }
        }

        private string ExtractNotes()
        {
            ENScriptWrapper ENScript = new ENScriptWrapper();
            // TODO: replace fixed path
            ENScript.ENScriptPath = @"C:\Program Files (x86)\Evernote\Evernote3.5\ENScript.exe";

            string exportFile = Path.GetTempFileName();

            if (ENScript.ExportNotebook(notebook, exportFile))
                return exportFile;

            return null;
        }

        private List<Note> ParseNotes(string exportFile)
        {
            List<Note> noteList = new List<Note>();
            XmlTextReader xtrInput;
            XmlDocument xdItem;

            xtrInput = new XmlTextReader(exportFile);

            while (xtrInput.Read())
            {
                while ((xtrInput.NodeType == XmlNodeType.Element) && (xtrInput.Name.ToLower() == "note"))
                {
                    xdItem = new XmlDocument();
                    xdItem.LoadXml(xtrInput.ReadOuterXml());
                    XmlNode node = xdItem.FirstChild;

                    // node is <note> element
                    // node.FirstChild.InnerText is <title>
                    node = node.FirstChild;

                    Note note = new Note();
                    note.Title = node.InnerText;
                    node = node.NextSibling;
                    note.Content = node.InnerXml;
                    XmlNodeList tagslist = xdItem.GetElementsByTagName("tag");
                    foreach (XmlNode n in tagslist)
                    {
                        note.Tags.Add(n.InnerText);
                    }

                    noteList.Add(note);
                }
            }
            xtrInput.Close();

            return noteList;
        }

        private List<Note> GetMailList()
        {
            List<Note> noteList = new List<Note>();

            // TODO: replace hard coded IMAP server
            IMAPConfig config = new IMAPConfig("imap.gmail.com", username, password, true, true, "/");
            client = new IMAPAsyncClient(config, 2);
            client.Start();

            GetMailsListRecursive(notefolder, ref noteList);

            return noteList;
        }

        private void GetMailsListRecursive(string folder, ref List<Note> noteList)
        {
            client.RequestManager.SubmitAndWait(new FolderTreeRequest(folder, null), false);
            IFolder currentFolder = client.MailboxManager.GetFolderByPath(folder);
            client.RequestManager.SubmitAndWait(new MessageListRequest(currentFolder, null), false);


            IMessage[] msgList = client.MailboxManager.GetMessagesByFolder(currentFolder);
            foreach (IMessage msg in msgList)
            {
                client.RequestManager.SubmitAndWait(new MessageFlagRequest(msg, delegate
                {
                    client.RequestManager.SubmitAndWait(new MessageHeaderRequest(msg, null), false);
                }), false);

                Note note = new Note();
                note.Title = msg.Subject;
                note.Tags.Add(folder);
                note.IMAPFolder = currentFolder;
                note.IMAPMessageUID = msg.UID;

                string hash = null;
                List<string> flags = msg.GetCustomFlags();
                foreach (string flag in flags)
                {
                    if (flag.StartsWith("XEveIm"))
                    {
                        hash = flag.Substring(6);
                        break;
                    }
                }

                bool bAdd = true;
                if ((hash != null) && (hash != ""))
                {
                    // does this note already exist?
                    note.ContentHash = hash;
                    foreach (Note n in noteList)
                    {
                        if (n.ContentHash == note.ContentHash)
                        {
                            n.Tags.Add(folder);
                            bAdd = false;
                            break;
                        }
                    }
                }
                if (bAdd)
                    noteList.Add(note);
            }

            IFolder[] subFolders = client.MailboxManager.GetSubFolders(currentFolder);
            foreach (IFolder f in subFolders)
            {
                GetMailsListRecursive(f.FullPath, ref noteList);
            }
        }

        private void DiffNotesAndMails(ref List<Note> notesEvernote, ref List<Note> notesIMAP)
        {
            foreach (Note n in notesIMAP)
            {
                if (n.ContentHash == "")
                {
                    // Notes with no hashs haven't been downloaded and processed yet, so they're new
                    // and must be imported into Evernote
                    n.Action = NoteAction.ImportToEvernote;
                }
                else
                {
                    // Notes with a hash that doesn't exist in Evernote have been removed from
                    // Evernote and should be removed on IMAP
                    bool existsInEvernote = notesEvernote.Find(delegate(Note findNote) { return findNote.ContentHash == n.ContentHash; }) != null;
                    if (!existsInEvernote)
                    {
                        n.Action = NoteAction.DeleteOnIMAP;
                    }
                }
            }

            foreach (Note n in notesEvernote)
            {
                bool existsInIMAP = notesIMAP.Find(delegate(Note findNote) { return findNote.ContentHash == n.ContentHash; }) != null;
                if (!existsInIMAP)
                    n.Action = NoteAction.UploadToIMAP;
            }
        }

        private void DownloadAndImportMailsToEvernote(List<Note> notesIMAP)
        {
            while (notesIMAP.Count > 0)
            {
                Note n = notesIMAP[0];
                //if (n.Action == NoteAction.ImportToEvernote)
                {
                    IMessage msg = client.MailboxManager.GetMessageByUID(n.IMAPMessageUID, n.IMAPFolder.ID);
                    AddInfoLine(string.Format("getting email\"{0}\"", msg.Subject));

                    FullMessageRequest fmr = new FullMessageRequest(client, msg);
                    //fmr.MessageProgress += new FullMessageProgressCallback(fmr_MessageProgress);
                    fmr.SubmitAndWait();
                    if (msg.ContentLoaded)
                    {
                        IMessageContent[] content = msg.MessageContent;
                        foreach (IMessageContent msgcontent in content)
                        {
                            if (!msgcontent.IsAttachment)
                            {
                                if ((msgcontent.TextData != null) && (msgcontent.TextData.Length > 0) && (n.ContentHash == ""))
                                {
                                    n.SetTextContent(msgcontent.TextData);
                                }
                                else if (msgcontent.HTMLData != null)
                                {
                                    n.SetHtmlContent(msgcontent.HTMLData);
                                }
                                Debug.Assert(n.ContentHash != "");
                            }
                            else
                            {
                                n.AddAttachment(msgcontent.BinaryData, msgcontent.ContentId, msgcontent.ContentType, msgcontent.ContentFilename);
                            }
                        }
                        n.Content = "<![CDATA[<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">" +
                                        "<en-note>" + n.Content + "</en-note>]]>";
                        // remove existing XEveIm flags
                        List<string> fls = new List<string>(msg.GetCustomFlags());
                        foreach (string flag in fls)
                        {
                            if (flag.StartsWith("XEveIm"))
                            {
                                client.MailboxManager.SetMessageFlag(msg, flag, false);
                            }
                        }

                        // update the XEveImHash tag for this email
                        string customFlag = "XEveIm" + n.ContentHash;
                        msg.SetCustomFlag(customFlag, false);
                        client.MailboxManager.SetMessageFlag(msg, customFlag, true);

                        // now, since GMail uses IMAP folders for tags and a message can have multiple tags,
                        // we have to see if the changed flag affected not just this IMAP message but
                        // others in other IMAP folders as well. If it has, those are the same message
                        // and we have to add those folder names to the tag list of this note.
                        List<Note> sameTitleNotes = notesIMAP.FindAll(delegate(Note findNote) { return findNote.Title == n.Title; });
                        foreach (Note same in sameTitleNotes)
                        {
                            IMessage m = client.MailboxManager.GetMessageByUID(same.IMAPMessageUID, same.IMAPFolder.ID);
                            client.RequestManager.SubmitAndWait(new MessageFlagRequest(m, null), false);
                            string hash = null;
                            List<string> flags = m.GetCustomFlags();
                            foreach (string flag in flags)
                            {
                                if (flag.StartsWith("XEveIm"))
                                {
                                    hash = flag.Substring(6);
                                    break;
                                }
                            }
                            if ((hash != null) && (hash == n.ContentHash))
                            {
                                // yes, this is the same message!
                                // remove it from the list and add its folder name as a tag
                                // to this note
                                if (n != same)
                                {
                                    n.Tags.Add(m.Folder.FullPath);
                                    notesIMAP.Remove(same);
                                }
                            }
                        }
                        AddInfoLine(string.Format("importing note\"{0}\"", msg.Subject));

                        // generate the Evernote export file
                        string path = Path.GetTempFileName();
                        n.SaveEvernoteExportData(path);
                        // import the export file into Evernote
                        ENScriptWrapper ENScript = new ENScriptWrapper();
                        // TODO: replace fixed path
                        ENScript.ENScriptPath = @"C:\Program Files (x86)\Evernote\Evernote3.5\ENScript.exe";
                        if (!ENScript.ImportNotes(path, notebook))
                        {
                            AddInfoLine(string.Format("failed to import note \"{0}\"", msg.Subject));
                        }
                        File.Delete(path);
                    }
                }
                notesIMAP.Remove(n);
            }
        }

        private void UploadNotesAsMails(string folder, List<Note> notesEvernote, string exportFile)
        {
            int notecounter = 0;
            foreach (Note n in notesEvernote)
            {

                XmlTextReader xtrInput;
                XmlDocument xdItem;

                xtrInput = new XmlTextReader(exportFile);

                while (xtrInput.Read())
                {
                    while ((xtrInput.NodeType == XmlNodeType.Element) && (xtrInput.Name.ToLower() == "note"))
                    {
                        xdItem = new XmlDocument();
                        xdItem.LoadXml(xtrInput.ReadOuterXml());
                        XmlNode node = xdItem.FirstChild;

                        // node is <note> element
                        // node.FirstChild.InnerText is <title>
                        node = node.FirstChild;

                        if (node.InnerText == n.Title)
                        {
                            node = node.NextSibling;
                            if (n.Content == node.InnerXml.Replace("\r", ""))
                            {
                                XmlNodeList atts = xdItem.GetElementsByTagName("resource");
                                foreach (XmlNode xmln in atts)
                                {
                                    Attachment attachment = new Attachment();
                                    attachment.Base64Data = xmln.FirstChild.InnerText;
                                    byte[] data = Convert.FromBase64String(xmln.FirstChild.InnerText);
                                    byte[] hash = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(data);
                                    string hashHex = BitConverter.ToString(hash).Replace("-", "").ToLower();

                                    attachment.Hash = hashHex;

                                    XmlNodeList fns = xdItem.GetElementsByTagName("file-name");
                                    if (fns.Count > n.Attachments.Count)
                                    {
                                        attachment.FileName = fns.Item(n.Attachments.Count).InnerText;
                                    }
                                    XmlNodeList mimes = xdItem.GetElementsByTagName("mime");
                                    if (mimes.Count > n.Attachments.Count)
                                    {
                                        attachment.ContentType = mimes.Item(n.Attachments.Count).InnerText;
                                    }
                                    n.Attachments.Add(attachment);
                                }
                            }
                        }
                    }
                }
                xtrInput.Close();

                string htmlBody = n.Content;

                List<LinkedResource> linkedResources = new List<LinkedResource>();
                List<System.Net.Mail.Attachment> attachedResources = new List<System.Net.Mail.Attachment>();
                foreach (Attachment attachment in n.Attachments)
                {
                    Regex rx = new Regex(@"<en-media\b[^>]*?hash=""" + attachment.Hash + @"""[^>]*/>", RegexOptions.IgnoreCase);
                    if ((attachment.ContentType.Contains("image")) && (rx.Match(htmlBody).Success))
                    {
                        // replace the <en-media /> tag with an <img /> tag
                        htmlBody = rx.Replace(htmlBody, @"<img src=""cid:" + attachment.Hash + @"""/>");
                        byte[] data = Convert.FromBase64String(attachment.Base64Data);
                        Stream s = new MemoryStream(data);
                        ContentType ct = new ContentType();
                        ct.Name = attachment.FileName;
                        ct.MediaType = attachment.ContentType;
                        LinkedResource lr = new LinkedResource(s, ct);
                        lr.ContentId = attachment.Hash;
                        linkedResources.Add(lr);
                    }
                    else
                    {
                        byte[] data = Convert.FromBase64String(attachment.Base64Data);
                        Stream s = new MemoryStream(data);
                        ContentType ct = new ContentType();
                        ct.Name = attachment.FileName;
                        ct.MediaType = attachment.ContentType;
                        System.Net.Mail.Attachment a = new System.Net.Mail.Attachment(s, ct);
                        attachedResources.Add(a);
                    }
                }
                htmlBody = htmlBody.Replace(@"<![CDATA[<?xml version=""1.0"" encoding=""UTF-8""?>", "");
                htmlBody = htmlBody.Replace(@"<!DOCTYPE en-note SYSTEM ""http://xml.evernote.com/pub/enml2.dtd"">", "");
                htmlBody = htmlBody.Replace("<en-note>", "<body>");
                htmlBody = htmlBody.Replace("</en-note>]]>", "</body>");
                htmlBody = htmlBody.Trim();
                htmlBody = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN""><head></head>" + htmlBody;
                MailMessage mailMsg = new MailMessage();

                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);
                foreach (LinkedResource lr in linkedResources)
                {
                    avHtml.LinkedResources.Add(lr);
                }

                // Add the alternate views instead of using MailMessage.Body
                mailMsg.AlternateViews.Add(avHtml);
                foreach (System.Net.Mail.Attachment a in attachedResources)
                {
                    mailMsg.Attachments.Add(a);
                }
                mailMsg.From = new MailAddress("EveImSync <eveimsync@tortoisesvn.net>");
                mailMsg.To.Add(new MailAddress("EveImSync <eveimsync@tortoisesvn.net>"));
                mailMsg.Subject = n.Title;
                string eml = mailMsg.GetEmailAsString();

                //using (StreamWriter outfile = new StreamWriter(@"D:\Development\EveImSync\" + notecounter.ToString() + ".eml"))
                //{
                //    outfile.Write(eml);
                //}
                //notecounter++;

                // find the folder to upload to
                IFolder currentFolder = client.MailboxManager.GetFolderByPath(folder);
                string customFlag = "XEveIm" + n.ContentHash;

                // now upload the new note
                client.RequestManager.SubmitAndWait(new AppendRequest(eml, customFlag, currentFolder, null), false);
            }
        }
    }
}
