// EvImSync - A tool to sync Evernote notes to IMAP mails and vice versa
// Copyright (C) 2010 - Stefan Kueng

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using EveImSync.Enums;
using InterIMAP;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Requests;

namespace EveImSync
{
    /// <summary>
    /// main dialog
    /// </summary>
    public partial class MainFrm : Form
    {
        private IMAPAsyncClient client;
        private delegate void StringDelegate(string foo);
        private string enscriptpath;
        private SynchronizationContext synchronizationContext;
        private bool cancelled = false;
        private SyncStep syncStep = SyncStep.Start;

        public MainFrm()
        {
            InitializeComponent();
            this.synchronizationContext = SynchronizationContext.Current;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigFrm cf = new ConfigFrm();
            cf.ShowDialog();
        }

        private void SetInfo(string line1, string line2, int pos, int max)
        {
            int fullpos = 0;

            switch (this.syncStep)
            {
                // full progress is from 0 - 100'000
                case SyncStep.ExtractNotes:      // 0- 10%
                    fullpos = max != 0 ? (int)(pos * 100000.0 / max * 0.1) : 0;
                    break;
                case SyncStep.ParseNotes:        // 10-20%
                    fullpos = max != 0 ? (int)(pos * 100000.0 / max * 0.1) + 10000 : 10000;
                    break;
                case SyncStep.GettingImapList:   // 20-30%
                    fullpos = max != 0 ? (int)(pos * 100000.0 / max * 0.1) + 20000 : 20000;
                    break;
                case SyncStep.CalculateWhatToDo: // 30-35%
                    fullpos = max != 0 ? (int)(pos * 100000.0 / max * 0.05) + 30000 : 30000;
                    break;
                case SyncStep.AdjustTags:        // 35-40%
                    fullpos = max != 0 ? (int)(pos * 100000.0 / max * 0.05) + 35000 : 35000;
                    break;
                case SyncStep.DownloadNotes:     // 40-70%
                    fullpos = max != 0 ? (int)(pos * 100000.0 / max * 0.3) + 40000 : 40000;
                    break;
                case SyncStep.UploadNotes:       // 70-100%
                    fullpos = max != 0 ? (int)(pos * 100000.0 / max * 0.3) + 70000 : 70000;
                    break;
            }

            synchronizationContext.Send(new SendOrPostCallback(delegate(object state)
            {
                if (line1 != null)
                    this.infoText1.Text = line1;
                if (line2 != null)
                    this.infoText2.Text = line2;
                this.progressIndicator.Minimum = 0;
                this.progressIndicator.Maximum = 100000;
                this.progressIndicator.Value = fullpos;
            }), null);

            if (max == 0)
                syncStep++;
        }

        private void Startsync_Click(object sender, EventArgs e)
        {
            if (startsync.Text == "Start Sync")
            {
                Configuration config = Configuration.Create();
                if (config.SyncPairs.Count == 0)
                {
                    ConfigFrm cf = new ConfigFrm();
                    cf.ShowDialog();
                    return;
                }

                startsync.Text = "Cancel";
                MethodInvoker syncDelegate = new MethodInvoker(SyncEvernoteWithIMAP);
                syncDelegate.BeginInvoke(null, null);
            }
            else
            {
                cancelled = true;
            }
        }

        private void SyncEvernoteWithIMAP()
        {
            Configuration config = Configuration.Create();
            enscriptpath = config.ENScriptPath;
            foreach (SyncPairSettings syncPair in config.SyncPairs)
            {
                if (cancelled)
                {
                    break;
                }
                synchronizationContext.Send(new SendOrPostCallback(delegate(object state)
                {
                    this.infoText0.Text = string.Format("Syncing notebook {0}", syncPair.EvernoteNotebook);
                }), null);

                syncStep = SyncStep.Start;
                SetInfo("Extracting notes from Evernote", "", 0, 0);
                string exportFile = ExtractNotes(syncPair.EvernoteNotebook);
                if (exportFile != null && exportFile != string.Empty)
                {
                    SetInfo("Parsing notes from Evernote", "", 0, 0);
                    List<Note> notesEvernote = ParseNotes(exportFile);
                    SetInfo("Fetching list of emails", "", 0, 0);
                    List<Note> notesIMAP = GetMailList(syncPair.IMAPServer, syncPair.IMAPUsername, syncPair.IMAPPassword, syncPair.IMAPNotesFolder);
                    SetInfo("Figuring out what needs to be synced", "", 0, 0);
                    DiffNotesAndMails(ref notesEvernote, ref notesIMAP, syncPair.LastSyncTime);
                    SetInfo("Adjusting tags in the GMail account", "", 0, 0);
                    AdjustIMAPTags(syncPair.IMAPNotesFolder, notesIMAP);
                    SetInfo("Downloading emails", "", 0, 0);
                    DownloadAndImportMailsToEvernote(notesIMAP, syncPair.EvernoteNotebook);
                    SetInfo("Uploading emails", "", 0, 0);
                    UploadNotesAsMails(syncPair.IMAPNotesFolder, notesEvernote, exportFile);
                    syncPair.LastSyncTime = DateTime.Now;
                    if (client != null)
                        client.Stop();
                }
            }

            if (!cancelled)
            {
                config.Save();
            }
            else
            {
                SetInfo(null, "Operation cancelled", 0, 0);
            }
            synchronizationContext.Send(new SendOrPostCallback(delegate(object state)
            {
                startsync.Text = "Start Sync";
                this.progressIndicator.Minimum = 0;
                this.progressIndicator.Maximum = 100000;
                this.progressIndicator.Value = 100000;
            }), null);
        }

        private string ExtractNotes(string notebook)
        {
            if (cancelled)
            {
                return null;
            }

            ENScriptWrapper enscript = new ENScriptWrapper();
            enscript.ENScriptPath = enscriptpath;

            string exportFile = Path.GetTempFileName();

            if (enscript.ExportNotebook(notebook, exportFile))
            {
                return exportFile;
            }

            return null;
        }

        private List<Note> ParseNotes(string exportFile)
        {
            List<Note> noteList = new List<Note>();
            if (cancelled)
            {
                return noteList;
            }

            XmlTextReader xtrInput;
            XmlDocument xmlDocItem;

            xtrInput = new XmlTextReader(exportFile);

            try
            {
                while (xtrInput.Read())
                {
                    while ((xtrInput.NodeType == XmlNodeType.Element) && (xtrInput.Name.ToLower() == "note"))
                    {
                        if (cancelled)
                        {
                            break;
                        }

                        xmlDocItem = new XmlDocument();
                        xmlDocItem.LoadXml(xtrInput.ReadOuterXml());
                        XmlNode node = xmlDocItem.FirstChild;

                        // node is <note> element
                        // node.FirstChild.InnerText is <title>
                        node = node.FirstChild;

                        Note note = new Note();
                        note.Title = node.InnerText;
                        node = node.NextSibling;
                        note.Content = node.InnerXml;
                        XmlNodeList tagslist = xmlDocItem.GetElementsByTagName("tag");
                        foreach (XmlNode n in tagslist)
                        {
                            note.Tags.Add(n.InnerText);
                        }

                        XmlNodeList datelist = xmlDocItem.GetElementsByTagName("created");
                        foreach (XmlNode n in datelist)
                        {
                            try
                            {
                                note.Date = DateTime.ParseExact(n.InnerText, "yyyyMMddTHHmmssZ", null);
                            }
                            catch (System.FormatException)
                            {
                            }
                        }

                        XmlNodeList datelist2 = xmlDocItem.GetElementsByTagName("updated");
                        foreach (XmlNode n in datelist2)
                        {
                            try
                            {
                                note.Date = DateTime.ParseExact(n.InnerText, "yyyyMMddTHHmmssZ", null);
                            }
                            catch (System.FormatException)
                            {
                            }
                        }

                        noteList.Add(note);
                    }
                }

                xtrInput.Close();
            }
            catch (System.Xml.XmlException)
            {
                // happens if the notebook was empty or does not exist.
            }

            return noteList;
        }

        private List<Note> GetMailList(string server, string username, string password, string notefolder)
        {
            List<Note> noteList = new List<Note>();
            if (cancelled)
            {
                return noteList;
            }

            IMAPConfig config = new IMAPConfig(server, username, password, true, true, "/");
            client = new IMAPAsyncClient(config, 2);
            if (client.Start() == false)
            {
                cancelled = true;
                return noteList;
            }

            GetMailsListRecursive(notefolder, ref noteList);

            return noteList;
        }

        private void GetMailsListRecursive(string folder, ref List<Note> noteList)
        {
            if (cancelled)
            {
                return;
            }

            SetInfo(null, string.Format("scanning folder \"{0}\"", folder), 0, 1);
            client.RequestManager.SubmitAndWait(new FolderTreeRequest(folder, null), false);
            IFolder currentFolder = client.MailboxManager.GetFolderByPath(folder);
            if (currentFolder == null)
            {
                // folder does not exist
                cancelled = true;
                return;
            }
            client.RequestManager.SubmitAndWait(new MessageListRequest(currentFolder, null), false);

            IMessage[] msgList = client.MailboxManager.GetMessagesByFolder(currentFolder);
            int msgCount = 0;
            int msgCounter = 0;
            foreach (IMessage cm in msgList)
            {
                msgCount++;
            }

            foreach (IMessage msg in msgList)
            {
                if (cancelled)
                {
                    break;
                }

                SetInfo(null, string.Format("scanning folder \"{0}\", message {1} of {2}", folder, ++msgCounter, msgCount), 0, 1);

                client.RequestManager.SubmitAndWait(new MessageFlagRequest(msg, delegate
                {
                    client.RequestManager.SubmitAndWait(new MessageHeaderRequest(msg, null), false);
                }), false);

                Note note = new Note();
                note.Title = msg.Subject;
                note.Date = msg.DateReceived;

                if (folder.IndexOf('/') >= 0)
                {
                    note.Tags.Add(folder.Substring(folder.IndexOf('/') + 1));
                }
                else
                {
                    note.Tags.Add(folder);
                }

                note.IMAPMessages.Add(msg);

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

                bool toAdd = true;
                if ((hash != null) && (hash != string.Empty))
                {
                    // does this note already exist?
                    note.ContentHash = hash;
                    foreach (Note n in noteList)
                    {
                        if (n.ContentHash == note.ContentHash)
                        {
                            if (folder.IndexOf('/') >= 0)
                            {
                                n.Tags.Add(folder.Substring(folder.IndexOf('/') + 1));
                            }
                            else
                            {
                                n.Tags.Add(folder);
                            }

                            n.IMAPMessages.Add(note.IMAPMessages[0]);
                            toAdd = false;
                            break;
                        }
                    }
                }

                if (toAdd)
                {
                    noteList.Add(note);
                }
            }

            IFolder[] subFolders = client.MailboxManager.GetSubFolders(currentFolder);
            foreach (IFolder f in subFolders)
            {
                if (cancelled)
                {
                    break;
                }

                GetMailsListRecursive(f.FullPath, ref noteList);
            }
        }

        private void DiffNotesAndMails(ref List<Note> notesEvernote, ref List<Note> notesIMAP, DateTime lastSync)
        {
            int counter = 0;
            foreach (Note n in notesIMAP)
            {
                SetInfo(null, "", counter++, notesIMAP.Count);
                if (cancelled)
                {
                    break;
                }

                if (n.ContentHash == string.Empty)
                {
                    // Notes with no hashs haven't been downloaded and processed yet, so they're new
                    // and must be imported into Evernote
                    n.Action = NoteAction.ImportToEvernote;
                }
                else
                {
                    // Notes with a hash that doesn't exist in Evernote have been removed from
                    // Evernote and should be removed on IMAP
                    Note noteInEvernote = notesEvernote.Find(delegate(Note findNote) { return findNote.ContentHash == n.ContentHash; });
                    bool existsInEvernote = noteInEvernote != null;
                    if (!existsInEvernote)
                    {
                        n.Action = NoteAction.DeleteOnIMAP;
                        bool force = false;
                        synchronizationContext.Send(new SendOrPostCallback(delegate(object state)
                        {
                            force = this.forceDownload.Checked;
                        }), null);
                        if (force)
                            n.Action = NoteAction.ImportToEvernote;

                        if (n.Date > lastSync)
                            n.Action = NoteAction.ImportToEvernote;
                    }
                    else
                    {
                        // if the note already exists in Evernote, we have to check whether tags have changed
                        foreach (string tag in noteInEvernote.Tags)
                        {
                            if (n.Tags.Find(findTag => { return findTag == tag; }) == null)
                            {
                                // tag does not exist in the email note
                                n.NewTags.Add(tag);
                                n.Action = NoteAction.AdjustTagsOnIMAP;
                            }
                        }

                        foreach (string tag in n.Tags)
                        {
                            if (noteInEvernote.Tags.Find(findTag => { return findTag == tag; }) == null)
                            {
                                // tag does not exist in the evernote note
                                n.ObsoleteTags.Add(tag);
                                n.Action = NoteAction.AdjustTagsOnIMAP;
                            }
                        }
                    }
                }
            }

            if (notesIMAP.Count > 0)
                SetInfo(null, "", notesIMAP.Count, notesIMAP.Count);
            foreach (Note n in notesEvernote)
            {
                bool existsInIMAP = notesIMAP.Find(delegate(Note findNote) { return findNote.ContentHash == n.ContentHash; }) != null;
                if (!existsInIMAP)
                {
                    n.Action = NoteAction.UploadToIMAP;
                }
            }
        }

        private void AdjustIMAPTags(string folder, List<Note> notesIMAP)
        {
            int counter = 0;
            foreach (Note note in notesIMAP)
            {
                if (note.Action == NoteAction.AdjustTagsOnIMAP)
                {
                    if (cancelled)
                    {
                        break;
                    }

                    SetInfo(null, string.Format("adjusting tags for email\"{0}\"", note.Title), counter, notesIMAP.Count);

                    foreach (string tag in note.NewTags)
                    {
                        if (cancelled)
                        {
                            break;
                        }

                        string tagfolder = folder + "/" + tag;
                        IFolder tagFolder = GetOrCreateFolderByPath(tagfolder);

                        client.RequestManager.SubmitAndWait(new CopyMessageRequest(note.IMAPMessages[0], tagFolder, null), true);
                        client.RequestManager.SubmitAndWait(new MessageListRequest(tagFolder, null), true);
                    }

                    foreach (IMessage msg in note.IMAPMessages)
                    {
                        if (cancelled)
                        {
                            break;
                        }

                        string tag = msg.Folder.FullPath;
                        if (tag.IndexOf('/') >= 0)
                        {
                            tag = tag.Substring(tag.IndexOf('/') + 1);
                        }

                        if (note.ObsoleteTags.Find(findTag => { return findTag == tag; }) != null)
                        {
                            client.RequestManager.SubmitAndWait(new DeleteMessageRequest(msg, null), true);
                        }
                    }
                }
                counter++;
            }
        }

        private void DownloadAndImportMailsToEvernote(List<Note> notesIMAP, string notebook)
        {
            int counter = 0;
            int numNotesToUpload = 0;
            foreach (Note ntu in notesIMAP)
            {
                if (ntu.Action == NoteAction.ImportToEvernote)
                    numNotesToUpload++;
            }

            while (notesIMAP.Count > 0)
            {
                if (cancelled)
                {
                    break;
                }

                Note n = notesIMAP[0];
                if (n.Action == NoteAction.ImportToEvernote)
                {
                    IMessage msg = n.IMAPMessages[0];
                    SetInfo(null, string.Format("getting email\"{0}\"", msg.Subject), counter++, numNotesToUpload);

                    FullMessageRequest fmr = new FullMessageRequest(client, msg);

                    // fmr.MessageProgress += new FullMessageProgressCallback(fmr_MessageProgress);
                    fmr.SubmitAndWait();
                    if (msg.ContentLoaded)
                    {
                        IMessageContent[] content = msg.MessageContent;
                        foreach (IMessageContent msgcontent in content)
                        {
                            if (!msgcontent.IsAttachment)
                            {
                                if ((msgcontent.TextData != null) && (msgcontent.TextData.Length > 0) && ((n.ContentHash == string.Empty) || (n.Content == null)))
                                {
                                    n.SetTextContent(msgcontent.TextData);
                                }
                                else if ((msgcontent.HTMLData != null) && ((msgcontent.HTMLData.Length > 0) || (n.Content == null)))
                                {
                                    n.SetHtmlContent(msgcontent.HTMLData);
                                }

                                Debug.Assert(n.ContentHash != string.Empty, "Hash is empty!");
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

                        // add the date
                        n.Date = msg.DateReceived;

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
                            IMessage m = same.IMAPMessages[0];
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
                                    n.IMAPMessages.Add(m);
                                    notesIMAP.Remove(same);
                                }
                            }
                        }

                        // generate the Evernote export file
                        string path = Path.GetTempFileName();
                        n.SaveEvernoteExportData(path);

                        // import the export file into Evernote
                        ENScriptWrapper enscript = new ENScriptWrapper();
                        enscript.ENScriptPath = enscriptpath;
                        if (!enscript.ImportNotes(path, notebook))
                        {
                            // failed to import note
                        }

                        File.Delete(path);
                    }
                }

                notesIMAP.Remove(n);
            }
        }

        private void UploadNotesAsMails(string folder, List<Note> notesEvernote, string exportFile)
        {
            int uploadcount = 0;
            foreach (Note n in notesEvernote)
            {
                if (n.Action == NoteAction.UploadToIMAP)
                {
                    uploadcount++;
                }
            }

            int counter = 0;
            foreach (Note n in notesEvernote)
            {
                if (cancelled)
                {
                    break;
                }

                if (n.Action == NoteAction.UploadToIMAP)
                {
                    SetInfo(null, string.Format("uploading note \"{0}\"", n.Title), counter++, uploadcount);

                    XmlTextReader xtrInput;
                    XmlDocument xmlDocItem;

                    xtrInput = new XmlTextReader(exportFile);

                    while (xtrInput.Read())
                    {
                        while ((xtrInput.NodeType == XmlNodeType.Element) && (xtrInput.Name.ToLower() == "note"))
                        {
                            if (cancelled)
                            {
                                break;
                            }

                            xmlDocItem = new XmlDocument();
                            xmlDocItem.LoadXml(xtrInput.ReadOuterXml());
                            XmlNode node = xmlDocItem.FirstChild;

                            // node is <note> element
                            // node.FirstChild.InnerText is <title>
                            node = node.FirstChild;

                            if (node.InnerText == n.Title)
                            {
                                node = node.NextSibling;
                                if (n.Content == node.InnerXml.Replace("\r", string.Empty))
                                {
                                    XmlNodeList atts = xmlDocItem.GetElementsByTagName("resource");
                                    foreach (XmlNode xmln in atts)
                                    {
                                        Attachment attachment = new Attachment();
                                        attachment.Base64Data = xmln.FirstChild.InnerText;
                                        byte[] data = Convert.FromBase64String(xmln.FirstChild.InnerText);
                                        byte[] hash = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(data);
                                        string hashHex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();

                                        attachment.Hash = hashHex;

                                        XmlNodeList fns = xmlDocItem.GetElementsByTagName("file-name");
                                        if (fns.Count > n.Attachments.Count)
                                        {
                                            attachment.FileName = fns.Item(n.Attachments.Count).InnerText;
                                        }

                                        XmlNodeList mimes = xmlDocItem.GetElementsByTagName("mime");
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
                        if (attachment.ContentType.Contains("image") && rx.Match(htmlBody).Success)
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

                    htmlBody = htmlBody.Replace(@"<![CDATA[<?xml version=""1.0"" encoding=""UTF-8""?>", string.Empty);
                    htmlBody = htmlBody.Replace(@"<!DOCTYPE en-note SYSTEM ""http://xml.evernote.com/pub/enml2.dtd"">", string.Empty);
                    htmlBody = htmlBody.Replace("<en-note>", "<body>");
                    htmlBody = htmlBody.Replace("</en-note>]]>", "</body>");
                    htmlBody = htmlBody.Trim();
                    htmlBody = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN""><head></head>" + htmlBody;
                    MailMessage mailMsg = new MailMessage();

                    AlternateView altViewHtml = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);
                    foreach (LinkedResource lr in linkedResources)
                    {
                        altViewHtml.LinkedResources.Add(lr);
                    }

                    // Add the alternate views instead of using MailMessage.Body
                    mailMsg.AlternateViews.Add(altViewHtml);
                    foreach (System.Net.Mail.Attachment a in attachedResources)
                    {
                        mailMsg.Attachments.Add(a);
                    }

                    mailMsg.From = new MailAddress("EveImSync <eveimsync@tortoisesvn.net>");
                    mailMsg.To.Add(new MailAddress("EveImSync <eveimsync@tortoisesvn.net>"));
                    mailMsg.Subject = n.Title;
                    string eml = mailMsg.GetEmailAsString();

                    Regex rex = new Regex(@"^date:(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    eml = rex.Replace(eml, "Date: " + n.Date.ToString("ddd, dd MMM yyyy HH:mm:ss K"));

                    // find the folder to upload to
                    string tagfolder = folder;
                    if (n.Tags.Count > 0)
                    {
                        tagfolder = tagfolder + "/" + n.Tags[0];
                    }

                    IFolder currentFolder = GetOrCreateFolderByPath(tagfolder);
                    string customFlag = "XEveIm" + n.ContentHash;

                    // now upload the new note
                    int numMsg = currentFolder.Messages.Length;
                    client.RequestManager.SubmitAndWait(new AppendRequest(eml, customFlag, currentFolder, null), false);

                    if (n.Tags.Count > 1)
                    {
                        IMessage[] oldMsgs = client.MailboxManager.GetMessagesByFolder(currentFolder);
                        client.RequestManager.SubmitAndWait(new MessageListRequest(currentFolder, null), false);
                        IMessage[] newMsgs = client.MailboxManager.GetMessagesByFolder(currentFolder);
                        IMessage newMsg = null;
                        foreach (IMessage imsg in newMsgs)
                        {
                            bool found = false;
                            foreach (IMessage omsg in oldMsgs)
                            {
                                if (imsg.UID == omsg.UID)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                newMsg = client.MailboxManager.GetMessageByUID(imsg.UID, currentFolder.ID);
                                break;
                            }
                        }

                        // copy the email to all tag folders
                        for (int i = 1; i < n.Tags.Count; ++i)
                        {
                            if (cancelled)
                            {
                                break;
                            }

                            tagfolder = folder + "/" + n.Tags[i];
                            IFolder tagFolder = GetOrCreateFolderByPath(tagfolder);

                            client.RequestManager.SubmitAndWait(new CopyMessageRequest(newMsg, tagFolder, null), true);
                            client.RequestManager.SubmitAndWait(new MessageListRequest(tagFolder, null), true);
                        }
                    }
                }
            }
        }

        private IFolder GetOrCreateFolderByPath(string folderpath)
        {
            IFolder requestedFolder = client.MailboxManager.GetFolderByPath(folderpath);
            if (requestedFolder == null)
            {
                // create the missing folder
                string parent = folderpath.Substring(0, folderpath.IndexOf('/'));
                IFolder parentFolder = GetOrCreateFolderByPath(parent);
                string name = folderpath.Substring(folderpath.LastIndexOf('/') + 1);
                client.RequestManager.SubmitAndWait(new CreateFolderRequest(name, parentFolder, null), true);
                client.RequestManager.SubmitAndWait(new FolderTreeRequest(parent, null), false);

                requestedFolder = client.MailboxManager.GetFolderByPath(folderpath);
            }

            return requestedFolder;
        }
    }
}
