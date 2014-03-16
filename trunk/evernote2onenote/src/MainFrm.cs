// Evernote2Onenote - imports Evernote notes to Onenote
// Copyright (C) 2014 - Stefan Kueng

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

using Evernote2Onenote.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using OneNote = Microsoft.Office.Interop.OneNote;

namespace Evernote2Onenote
{
    /// <summary>
    /// main dialog
    /// </summary>
    public partial class MainFrm : Form
    {
        private delegate void StringDelegate(string foo);
        private string enscriptpath;
        private string m_EvernoteNotebookPath;
        private SynchronizationContext synchronizationContext;
        private bool cancelled = false;
        private SyncStep syncStep = SyncStep.Start;
        private Microsoft.Office.Interop.OneNote.Application onApp = null;
        private string m_PageID;
        private string m_xmlNewOutlineContent =
            "<one:Meta name=\"{2}\" content=\"{1}\"/>" +
            "<one:OEChildren><one:HTMLBlock><one:Data><![CDATA[{0}]]></one:Data></one:HTMLBlock>{3}</one:OEChildren>";

        private string m_xmlSourceUrl = "<one:OE alignment=\"left\" quickStyleIndex=\"2\"><one:T><![CDATA[From &lt;<a href=\"{0}\">{0}</a>&gt; ]]></one:T></one:OE>";

        private string m_xmlNewOutline =
            "<?xml version=\"1.0\"?>" +
            "<one:Page xmlns:one=\"{2}\" ID=\"{1}\" dateTime=\"{5}\">" +
            "<one:Title selected=\"partial\" lang=\"en-US\">" +
                        "<one:OE creationTime=\"{5}\" lastModifiedTime=\"{5}\" style=\"font-family:Calibri;font-size:17.0pt\">" +
                            "<one:T><![CDATA[{3}]]></one:T> " +
                        "</one:OE>" +
                        "</one:Title>{4}" +
            "<one:Outline>{0}</one:Outline></one:Page>";
        private string m_xmlns = "http://schemas.microsoft.com/office/onenote/2010/onenote";
        private string ENNotebookName = "";

        public MainFrm()
        {
            InitializeComponent();
            this.synchronizationContext = SynchronizationContext.Current;
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            versionLabel.Text = string.Format("Version: {0}", version);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
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
                case SyncStep.CalculateWhatToDo: // 30-35%
                    fullpos = max != 0 ? (int)(pos * 100000.0 / max * 0.05) + 30000 : 30000;
                    break;
                case SyncStep.ImportNotes:       // 35-100%
                    fullpos = max != 0 ? (int)(pos * 100000.0 / max * 0.65) + 35000 : 35000;
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

        static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        private void Startsync_Click(object sender, EventArgs e)
        {
            ENNotebookName = this.textBoxENNotebookName.Text;
            if (ENNotebookName.Length == 0)
            {
                MessageBox.Show("Please enter a notebook in EverNote to import the notes from", "Evernote2Onenote");
                return;
            }
            enscriptpath = ProgramFilesx86() + "\\Evernote\\Evernote\\ENScript.exe";
            if (!File.Exists(enscriptpath))
            {
                MessageBox.Show("Could not find the ENScript.exe file from Evernote!\nPlease select this file in the next dialog.", "Evernote2Onenote");
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Applications|*.exe";
                openFileDialog1.Title = "Select the ENScript.exe file";
                openFileDialog1.CheckPathExists = true;

                // Show the Dialog.
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    enscriptpath = openFileDialog1.FileName;
                }
                if (!File.Exists(enscriptpath))
                    return;
            }

            onApp = new OneNote.Application();
            if (onApp == null)
            {
                MessageBox.Show("Could not start Onenote!", "Evernote2Onenote");
                return;
            }
            // create a new notebook named "EverNote"
            try
            {
                string xmlHierarchy;
                onApp.GetHierarchy("", OneNote.HierarchyScope.hsNotebooks, out xmlHierarchy);

                // Get the hierarchy for the default notebook folder
                onApp.GetSpecialLocation(OneNote.SpecialLocation.slDefaultNotebookFolder, out m_EvernoteNotebookPath);
                m_EvernoteNotebookPath += "\\" + ENNotebookName;
                string newnbID;
                onApp.OpenHierarchy(m_EvernoteNotebookPath, "", out newnbID, OneNote.CreateFileType.cftNotebook);
                string xmlUnfiledNotes;
                onApp.GetHierarchy(newnbID, OneNote.HierarchyScope.hsPages, out xmlUnfiledNotes);

                // Load and process the hierarchy
                XmlDocument docHierarchy = new XmlDocument();
                docHierarchy.LoadXml(xmlHierarchy);
                StringBuilder Hierarchy = new StringBuilder();
                AppendHierarchy(docHierarchy.DocumentElement, Hierarchy, 0);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not create the target notebook in Onenote!", "Evernote2Onenote");
                return;
            }

            if (startsync.Text == "Start Import")
            {
                startsync.Text = "Cancel";
                MethodInvoker syncDelegate = new MethodInvoker(ImportNotesToOnenote);
                syncDelegate.BeginInvoke(null, null);
            }
            else
            {
                cancelled = true;
            }
        }

        private void ImportNotesToOnenote()
        {
            syncStep = SyncStep.Start;
            SetInfo("Extracting notes from Evernote", "", 0, 0);
            string exportFile = ExtractNotes(ENNotebookName);
            if (exportFile != null)
            {
                List<Note> notesEvernote = new List<Note>();
                if (exportFile != string.Empty)
                {
                    SetInfo("Parsing notes from Evernote", "", 0, 0);
                    notesEvernote = ParseNotes(exportFile);
                }
                if (exportFile != string.Empty)
                {
                    SetInfo("importing notes to Onenote", "", 0, 0);
                    ImportNotesToOnenote(ENNotebookName, notesEvernote, exportFile);
                }
            }
            else
            {
                MessageBox.Show(string.Format("The notebook \"{0}\" either does not exist or isn't accessible!", ENNotebookName));
            }

            if (cancelled)
            {
                SetInfo(null, "Operation cancelled", 0, 0);
            }
            synchronizationContext.Send(new SendOrPostCallback(delegate(object state)
            {
                startsync.Text = "Start Import";
                this.infoText1.Text = "Finished";
                this.progressIndicator.Minimum = 0;
                this.progressIndicator.Maximum = 100000;
                this.progressIndicator.Value = 0;
            }), null);
        }

        private string ExtractNotes(string notebook)
        {
            if (cancelled)
            {
                return null;
            }
            syncStep = SyncStep.ExtractNotes;

            ENScriptWrapper enscript = new ENScriptWrapper();
            enscript.ENScriptPath = enscriptpath;

            string exportFile = Path.GetTempFileName();
#if DEBUG
            exportFile = @"D:\Development\evimsync\" + notebook + ".xml";
#endif
            if (enscript.ExportNotebook(notebook, exportFile))
            {
                return exportFile;
            }

            // in case the selected notebook is empty, we don't get
            // an exportFile. But just to make sure the notebook
            // exists anyway, we check that here before giving up
            if (enscript.GetNotebooks().Contains(notebook))
                return string.Empty;

            return null;
        }

        private List<Note> ParseNotes(string exportFile)
        {
            syncStep = SyncStep.ParseNotes;
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

                        noteList.Add(note);
                    }
                }

                xtrInput.Close();
            }
            catch (System.Xml.XmlException)
            {
                // happens if the notebook was empty or does not exist.
                MessageBox.Show(string.Format("The notebook \"{0}\" either does not exist or empty!", ENNotebookName));
            }

            return noteList;
        }

        private void ImportNotesToOnenote(string folder, List<Note> notesEvernote, string exportFile)
        {
            syncStep = SyncStep.CalculateWhatToDo;
            int uploadcount = 0;
            foreach (Note n in notesEvernote)
            {
                uploadcount++;
            }

            string temppath = Path.GetTempPath() + "\\ev2on";
            Directory.CreateDirectory(temppath);

            syncStep = SyncStep.ImportNotes;
            int counter = 0;


            {
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
                                if (fns.Count > note.Attachments.Count)
                                {
                                    attachment.FileName = fns.Item(note.Attachments.Count).InnerText;
                                }

                                XmlNodeList mimes = xmlDocItem.GetElementsByTagName("mime");
                                if (mimes.Count > note.Attachments.Count)
                                {
                                    attachment.ContentType = mimes.Item(note.Attachments.Count).InnerText;
                                }

                                note.Attachments.Add(attachment);
                            }

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
                            XmlNodeList sourceurl = xmlDocItem.GetElementsByTagName("source-url");
                            note.SourceUrl = "";
                            foreach (XmlNode n in sourceurl)
                            {
                                try
                                {
                                    note.SourceUrl = n.InnerText;
                                    if (n.InnerText.StartsWith("file://"))
                                        note.SourceUrl = "";
                                }
                                catch (System.FormatException)
                                {
                                }
                            }

                            SetInfo(null, string.Format("importing note ({0} of {1}) : \"{2}\"", counter + 1, uploadcount, note.Title), counter++, uploadcount);

                            string htmlBody = note.Content;

                            List<string> tempfiles = new List<string>();
                            string xmlAttachments = "";
                            foreach (Attachment attachment in note.Attachments)
                            {
                                // save the attached file
                                string tempfilepath = temppath + "\\";
                                byte[] data = Convert.FromBase64String(attachment.Base64Data);
                                if ((attachment.FileName != null) && (attachment.FileName.Length > 0))
                                {
                                    string name = attachment.FileName;
                                    string invalid = new string(Path.GetInvalidFileNameChars());
                                    foreach (char c in invalid)
                                    {
                                        name = name.Replace(c.ToString(), "");
                                    }
                                    if (name.Length >= (240 - tempfilepath.Length))
                                        name = name.Substring(name.Length - (240 - tempfilepath.Length));
                                    tempfilepath += name;
                                }
                                else
                                    tempfilepath += attachment.Hash;
                                Stream fs = new FileStream(tempfilepath, FileMode.Create);
                                fs.Write(data, 0, data.Length);
                                fs.Close();
                                tempfiles.Add(tempfilepath);

                                Regex rx = new Regex(@"<en-media\b[^>]*?hash=""" + attachment.Hash + @"""[^>]*/>", RegexOptions.IgnoreCase);
                                if ((attachment.ContentType != null) && (attachment.ContentType.Contains("image") && rx.Match(htmlBody).Success))
                                {
                                    // replace the <en-media /> tag with an <img /> tag
                                    htmlBody = rx.Replace(htmlBody, @"<img src=""file:///" + tempfilepath + @"""/>");
                                }
                                else
                                {
                                    if ((attachment.FileName != null) && (attachment.FileName.Length > 0))
                                        xmlAttachments += string.Format("<one:InsertedFile pathSource=\"{0}\" preferredName=\"{1}\" />", tempfilepath, attachment.FileName);
                                    else
                                        xmlAttachments += string.Format("<one:InsertedFile pathSource=\"{0}\" preferredName=\"{1}\" />", tempfilepath, attachment.Hash);
                                }
                            }
                            note.Attachments.Clear();

                            htmlBody = htmlBody.Replace(@"<![CDATA[<?xml version=""1.0"" encoding=""UTF-8""?>", string.Empty);
                            htmlBody = htmlBody.Replace(@"<!DOCTYPE en-note SYSTEM ""http://xml.evernote.com/pub/enml2.dtd"">", string.Empty);
                            htmlBody = htmlBody.Replace("<en-note>", "<body>");
                            htmlBody = htmlBody.Replace("</en-note>]]>", "</body>");
                            htmlBody = htmlBody.Replace("</en-note>\n]]>", "</body>");
                            htmlBody = htmlBody.Trim();
                            htmlBody = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN""><head></head>" + htmlBody;

                            string emailBody = htmlBody;
                            Regex rex = new Regex(@"^date:(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            emailBody = rex.Replace(emailBody, "Date: " + note.Date.ToString("ddd, dd MMM yyyy HH:mm:ss K"));

                            try
                            {
                                // Get the hierarchy for all the notebooks
                                if (note.Tags.Count > 0)
                                {
                                    foreach (string tag in note.Tags)
                                    {
                                        string sectionId = GetSection(tag);
                                        onApp.CreateNewPage(sectionId, out m_PageID, Microsoft.Office.Interop.OneNote.NewPageStyle.npsBlankPageWithTitle);
                                        string textToSave;
                                        onApp.GetPageContent(m_PageID, out textToSave, Microsoft.Office.Interop.OneNote.PageInfo.piBasic);
                                        //OneNote uses HTML for the xml string to pass to the UpdatePageContent, so use the
                                        //Outlook HTMLBody property.  It coerces rtf and plain text to HTML.
                                        int outlineID = new System.Random().Next();
                                        //string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, m_outlineIDMetaName);
                                        string xmlSource = string.Format(m_xmlSourceUrl, note.SourceUrl);
                                        string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, System.Security.SecurityElement.Escape(note.Title), note.SourceUrl.Length > 0 ? xmlSource : "");
                                        string xml = string.Format(m_xmlNewOutline, outlineContent, m_PageID, m_xmlns, System.Security.SecurityElement.Escape(note.Title), xmlAttachments, note.Date.ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'"));
                                        onApp.UpdatePageContent(xml, DateTime.MinValue, OneNote.XMLSchema.xs2010, true);
                                    }
                                }
                                else
                                {
                                    string sectionId = GetSection("unspecified");
                                    onApp.CreateNewPage(sectionId, out m_PageID, Microsoft.Office.Interop.OneNote.NewPageStyle.npsBlankPageWithTitle);
                                    string textToSave;
                                    onApp.GetPageContent(m_PageID, out textToSave, Microsoft.Office.Interop.OneNote.PageInfo.piBasic);
                                    //OneNote uses HTML for the xml string to pass to the UpdatePageContent, so use the
                                    //Outlook HTMLBody property.  It coerces rtf and plain text to HTML.
                                    int outlineID = new System.Random().Next();
                                    //string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, m_outlineIDMetaName);
                                    string xmlSource = string.Format(m_xmlSourceUrl, note.SourceUrl);
                                    string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, System.Security.SecurityElement.Escape(note.Title), note.SourceUrl.Length > 0 ? xmlSource : "");
                                    string xml = string.Format(m_xmlNewOutline, outlineContent, m_PageID, m_xmlns, System.Security.SecurityElement.Escape(note.Title), xmlAttachments, note.Date.ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'"));
                                    onApp.UpdatePageContent(xml, DateTime.MinValue, OneNote.XMLSchema.xs2010, true);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(string.Format("Note:{0}\n{1}", note.Title, ex.ToString()));
                            }

                            foreach (string p in tempfiles)
                            {
                                File.Delete(p);
                            }
                        }
                    }

                    xtrInput.Close();
                }
                catch (System.Xml.XmlException)
                {
                    // happens if the notebook was empty or does not exist.
                    MessageBox.Show(string.Format("The notebook \"{0}\" either does not exist or empty!", ENNotebookName));
                }

            }
        }
        private void AppendHierarchy(XmlNode xml, StringBuilder str, int level)
        {
            // The set of elements that are themselves meaningful to export:
            if (xml.Name == "one:Notebook" || xml.Name == "one:SectionGroup" || xml.Name == "one:Section" || xml.Name == "one:Page")
            {
                string ID;
                if (xml.LocalName == "Section" && xml.Attributes["path"].Value == m_EvernoteNotebookPath)
                    ID = "UnfiledNotes";
                else
                    ID = xml.Attributes["ID"].Value;
                string name = HttpUtility.HtmlEncode(xml.Attributes["name"].Value);
                if (str.Length > 0)
                    str.Append("\n");
                str.Append(string.Format("{0} {1} {2} {3}",
                    new string[] { level.ToString(), xml.LocalName, ID, name }));
            }
            // The set of elements that contain children that are meaningful to export:
            if (xml.Name == "one:Notebooks" || xml.Name == "one:Notebook" || xml.Name == "one:SectionGroup" || xml.Name == "one:Section")
            {
                foreach (XmlNode child in xml.ChildNodes)
                {
                    int nextLevel;
                    if (xml.Name == "one:Notebooks")
                        nextLevel = level;
                    else
                        nextLevel = level + 1;
                    AppendHierarchy(child, str, nextLevel);
                }
            }
        }


        private void homeLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://stefanstools.sourceforge.net/Evernote2Onenote.html");
        }
        private string GetSection(string sectionName)
        {
            string newnbID = "";
            try
            {
                string xmlHierarchy;
                onApp.GetHierarchy("", OneNote.HierarchyScope.hsNotebooks, out xmlHierarchy);

                onApp.OpenHierarchy(m_EvernoteNotebookPath + "\\" + sectionName + ".one", "", out newnbID, OneNote.CreateFileType.cftSection);
                string xmlSections;
                onApp.GetHierarchy(newnbID, OneNote.HierarchyScope.hsSections, out xmlSections);

                // Load and process the hierarchy
                XmlDocument docHierarchy = new XmlDocument();
                docHierarchy.LoadXml(xmlHierarchy);
                StringBuilder Hierarchy = new StringBuilder(sectionName);
                AppendHierarchy(docHierarchy.DocumentElement, Hierarchy, 0);
            }
            catch (Exception)
            {
                return "";
            }
            return newnbID;
        }
    }
}
