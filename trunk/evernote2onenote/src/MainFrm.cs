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
using System.Globalization;
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
                        "<one:OE creationTime=\"{5}\" lastModifiedTime=\"{5}\">" +
                            "<one:T><![CDATA[{3}]]></one:T> " +
                        "</one:OE>" +
                        "</one:Title>{4}" +
            "<one:Outline>{0}</one:Outline></one:Page>";
        private string m_xmlns = "http://schemas.microsoft.com/office/onenote/2010/onenote";
        private string ENNotebookName = "";
        private bool m_bUseUnfiledSection = false;
        private string m_enexfile = "";
        string newnbID = "";

        private string cmdNoteBook = "";
        private DateTime cmdDate = new DateTime(0);

        private Regex rxCDATA = new Regex(@"<!\[CDATA\[<\?xml version=""1.0""[^?]*\?>", RegexOptions.IgnoreCase);
        private Regex rxBodyStart = new Regex(@"<en-note[^>/]*>", RegexOptions.IgnoreCase);
        private Regex rxBodyEnd = new Regex(@"</en-note\s*>\s*]]>", RegexOptions.IgnoreCase);
        private Regex rxBodyEmpty = new Regex(@"<en-note[^>/]*/>\s*]]>", RegexOptions.IgnoreCase);
        private Regex rxDate = new Regex(@"^date:(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private Regex rxNote = new Regex("<title>(.+)</title>", RegexOptions.IgnoreCase);
        private static readonly Regex rxDtd = new Regex(@"<!DOCTYPE en-note SYSTEM \""http:\/\/xml\.evernote\.com\/pub\/enml\d*\.dtd\"">", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public MainFrm(string cmdNotebook, string cmdDate)
        {
            InitializeComponent();
            this.synchronizationContext = SynchronizationContext.Current;
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            versionLabel.Text = string.Format("Version: {0}", version);

            if (cmdNotebook.Length > 0)
                cmdNoteBook = cmdNotebook;
            if (cmdDate.Length > 0)
            {
                try
                {
                    this.cmdDate = DateTime.Parse(cmdDate);
                }
                catch (Exception)
                {
                    MessageBox.Show(string.Format("The Datestring\n{0}\nis not valid!", cmdDate));
                }
            }
            try
            {
                importDatePicker.Value = this.cmdDate;
            }
            catch (Exception)
            {
                importDatePicker.Value = importDatePicker.MinDate;
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
                    this.Close();
            }
            ENScriptWrapper enscript = new ENScriptWrapper();
            enscript.ENScriptPath = enscriptpath;
            var notebooklist = enscript.GetNotebooks();
            foreach (string s in notebooklist)
                this.notebookCombo.Items.Add(s);
            if (notebooklist.Count == 0)
            {
                MessageBox.Show("No Notebooks found in Evernote!\nMake sure you have at least one locally synched notebook.", "Evernote2Onenote");
                startsync.Enabled = false;
            }
            else
                this.notebookCombo.SelectedIndex = 0;

            if (cmdNotebook.Length > 0)
            {
                Startsync_Click(null, null);
            }
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
        private void btnENEXImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Evernote exports|*.enex";
            openFileDialog1.Title = "Select the ENEX file";
            openFileDialog1.CheckPathExists = true;

            // Show the Dialog.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                m_enexfile = openFileDialog1.FileName;
                Startsync_Click(sender, e);
            }
        }

        private void Startsync_Click(object sender, EventArgs e)
        {
            ENNotebookName = this.notebookCombo.SelectedItem as string;
            if (m_enexfile != null && m_enexfile.Length > 0)
            {
                ENNotebookName = Path.GetFileNameWithoutExtension(m_enexfile);
            }
            if (cmdNoteBook.Length > 0)
                ENNotebookName = cmdNoteBook;
            if (ENNotebookName.Length == 0)
            {
                MessageBox.Show("Please enter a notebook in EverNote to import the notes from", "Evernote2Onenote");
                return;
            }
            if (m_enexfile != null && m_enexfile.Length > 0)
            {
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
            }

            try
            {
                onApp = new OneNote.Application();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Could connect to Onenote!\nReasons for this might be:\n* The desktop version of onenote is not installed\n* Onenote is not installed properly\n* Onenote is already running but with a different user account\n\n{0}", ex.ToString()));
                return;
            }
            if (onApp == null)
            {
                MessageBox.Show(string.Format("Could connect to Onenote!\nReasons for this might be:\n* The desktop version of onenote is not installed\n* Onenote is not installed properly\n* Onenote is already running but with a different user account\n\n{0}"));
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
                try
                {
                    string xmlHierarchy;
                    onApp.GetHierarchy("", OneNote.HierarchyScope.hsPages, out xmlHierarchy);

                    // Get the hierarchy for the default notebook folder
                    onApp.GetSpecialLocation(OneNote.SpecialLocation.slUnfiledNotesSection, out m_EvernoteNotebookPath);
                    onApp.OpenHierarchy(m_EvernoteNotebookPath, "", out newnbID, OneNote.CreateFileType.cftNone);
                    string xmlUnfiledNotes;
                    onApp.GetHierarchy(newnbID, OneNote.HierarchyScope.hsPages, out xmlUnfiledNotes);
                    m_bUseUnfiledSection = true;
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(string.Format("Could not create the target notebook in Onenote!\n{0}", ex2.ToString()));
                    return;
                }
            }

            if (importDatePicker.Value > cmdDate)
                cmdDate = importDatePicker.Value;

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
            if (m_enexfile != null && m_enexfile.Length > 0)
            {
                List<Note> notesEvernote = new List<Note>();
                if (m_enexfile != string.Empty)
                {
                    SetInfo("Parsing notes from Evernote", "", 0, 0);
                    notesEvernote = ParseNotes(m_enexfile);
                }
                if (m_enexfile != string.Empty)
                {
                    SetInfo("importing notes to Onenote", "", 0, 0);
                    ImportNotesToOnenote(ENNotebookName, notesEvernote, m_enexfile);
                }
            }
            else
            {
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
            }

            m_enexfile = "";
            if (cancelled)
            {
                SetInfo(null, "Operation cancelled", 0, 0);
            }
            else
                SetInfo("", "", 0, 0);

            synchronizationContext.Send(new SendOrPostCallback(delegate(object state)
            {
                startsync.Text = "Start Import";
                this.infoText1.Text = "Finished";
                this.progressIndicator.Minimum = 0;
                this.progressIndicator.Maximum = 100000;
                this.progressIndicator.Value = 0;
            }), null);
            if (cmdNoteBook.Length > 0)
            {
                synchronizationContext.Send(new SendOrPostCallback(delegate(object state)
                {
                    this.Close();
                }), null);
            }
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
            string xmltext = "";
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
                        xmltext = SanitizeXml(xtrInput.ReadOuterXml());
                        xmlDocItem.LoadXml(xmltext);
                        XmlNode node = xmlDocItem.FirstChild;

                        // node is <note> element
                        // node.FirstChild.InnerText is <title>
                        node = node.FirstChild;

                        Note note = new Note();
                        note.Title = HttpUtility.HtmlDecode(node.InnerText);

                        noteList.Add(note);
                    }
                }

                xtrInput.Close();
            }
            catch (System.Xml.XmlException ex)
            {
                // happens if the notebook was empty or does not exist.
                // Or due to a parsing error if a note isn't properly xml encoded
                // 
                // try to find the name of the note that's causing the problems
                string notename = "";
                if (xmltext.Length > 0)
                {
                    Regex rxnote = new Regex("<title>(.+)</title>", RegexOptions.IgnoreCase);
                    var notematch = rxnote.Match(xmltext);
                    if (notematch.Groups.Count == 2)
                    {
                        notename = notematch.Groups[1].ToString();
                    }
                }
                if (notename.Length > 0)
                    MessageBox.Show(string.Format("Error parsing the note \"{2}\" in notebook \"{0}\",\n{1}", ENNotebookName, ex.ToString(), notename));
                else
                    MessageBox.Show(string.Format("Error parsing the notebook \"{0}\"\n{1}", ENNotebookName, ex.ToString()));
            }

            return noteList;
        }

        private void ImportNotesToOnenote(string folder, List<Note> notesEvernote, string exportFile)
        {
            syncStep = SyncStep.CalculateWhatToDo;
            int uploadcount = notesEvernote.Count;

            string temppath = Path.GetTempPath() + "\\ev2on";
            Directory.CreateDirectory(temppath);

            syncStep = SyncStep.ImportNotes;
            int counter = 0;


            {
                XmlTextReader xtrInput;
                XmlDocument xmlDocItem;
                string xmltext = "";
                try
                {
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
                            xmltext = SanitizeXml(xtrInput.ReadOuterXml());
                            xmlDocItem.LoadXml(xmltext);
                            XmlNode node = xmlDocItem.FirstChild;

                            // node is <note> element
                            // node.FirstChild.InnerText is <title>
                            node = node.FirstChild;

                            Note note = new Note();
                            note.Title = HttpUtility.HtmlDecode(node.InnerText);
                            node = node.NextSibling;
                            note.Content = HttpUtility.HtmlDecode(node.InnerXml);

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
                                    attachment.FileName = HttpUtility.HtmlDecode(fns.Item(note.Attachments.Count).InnerText);
                                    string invalid = new string(Path.GetInvalidFileNameChars());
                                    foreach (char c in invalid)
                                    {
                                        attachment.FileName = attachment.FileName.Replace(c.ToString(), "");
                                    }
                                    attachment.FileName = System.Security.SecurityElement.Escape(attachment.FileName);
                                }

                                XmlNodeList mimes = xmlDocItem.GetElementsByTagName("mime");
                                if (mimes.Count > note.Attachments.Count)
                                {
                                    attachment.ContentType = HttpUtility.HtmlDecode(mimes.Item(note.Attachments.Count).InnerText);
                                }

                                note.Attachments.Add(attachment);
                            }

                            XmlNodeList tagslist = xmlDocItem.GetElementsByTagName("tag");
                            foreach (XmlNode n in tagslist)
                            {
                                note.Tags.Add(HttpUtility.HtmlDecode(n.InnerText));
                            }

                            XmlNodeList datelist = xmlDocItem.GetElementsByTagName("created");
                            foreach (XmlNode n in datelist)
                            {
                                DateTime dateCreated;

                                if (DateTime.TryParseExact(n.InnerText, "yyyyMMddTHHmmssZ", CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out dateCreated))
                                {
                                    note.Date = dateCreated;
                                }
                            }
                            XmlNodeList datelist2 = xmlDocItem.GetElementsByTagName("updated");
                            foreach (XmlNode n in datelist2)
                            {
                                DateTime dateUpdated;

                                if (DateTime.TryParseExact(n.InnerText, "yyyyMMddTHHmmssZ", CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out dateUpdated))
                                {
                                    note.Date = dateUpdated;
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

                            if (cmdDate > note.Date)
                                continue;

                            SetInfo(null, string.Format("importing note ({0} of {1}) : \"{2}\"", counter + 1, uploadcount, note.Title), counter++, uploadcount);

                            string htmlBody = note.Content;

                            List<string> tempfiles = new List<string>();
                            string xmlAttachments = "";
                            foreach (Attachment attachment in note.Attachments)
                            {
                                // save the attached file
                                string tempfilepath = temppath + "\\";
                                byte[] data = Convert.FromBase64String(attachment.Base64Data);
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
                                    rx = new Regex(@"<en-media\b[^>]*?hash=""" + attachment.Hash + @"""[^>]*></en-media>", RegexOptions.IgnoreCase);
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
                            }
                            note.Attachments.Clear();

                            htmlBody = rxCDATA.Replace(htmlBody, string.Empty);
                            htmlBody = rxDtd.Replace(htmlBody, string.Empty);
                            htmlBody = rxBodyStart.Replace(htmlBody, "<body>");
                            htmlBody = rxBodyEnd.Replace(htmlBody, "</body>");
                            htmlBody = rxBodyEmpty.Replace(htmlBody, "<body></body>");
                            htmlBody = htmlBody.Trim();
                            htmlBody = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN""><head></head>" + htmlBody;

                            string emailBody = htmlBody;
                            emailBody = rxDate.Replace(emailBody, "Date: " + note.Date.ToString("ddd, dd MMM yyyy HH:mm:ss K"));
                            emailBody = emailBody.Replace("&apos;", "'");
                            emailBody = emailBody.Replace("’", "&rsquo;");
                            emailBody = emailBody.Replace("‘", "&lsquo;");

                            try
                            {
                                // Get the hierarchy for all the notebooks
                                if ((note.Tags.Count > 0) && (!m_bUseUnfiledSection))
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
                                        string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), note.SourceUrl.Length > 0 ? xmlSource : "");
                                        string xml = string.Format(m_xmlNewOutline, outlineContent, m_PageID, m_xmlns, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), xmlAttachments, note.Date.ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'"));
                                        onApp.UpdatePageContent(xml, DateTime.MinValue, OneNote.XMLSchema.xs2010, true);
                                    }
                                }
                                else
                                {
                                    string sectionId = m_bUseUnfiledSection ? newnbID : GetSection("unspecified");
                                    onApp.CreateNewPage(sectionId, out m_PageID, Microsoft.Office.Interop.OneNote.NewPageStyle.npsBlankPageWithTitle);
                                    string textToSave;
                                    onApp.GetPageContent(m_PageID, out textToSave, Microsoft.Office.Interop.OneNote.PageInfo.piBasic);
                                    //OneNote uses HTML for the xml string to pass to the UpdatePageContent, so use the
                                    //Outlook HTMLBody property.  It coerces rtf and plain text to HTML.
                                    int outlineID = new System.Random().Next();
                                    //string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, m_outlineIDMetaName);
                                    string xmlSource = string.Format(m_xmlSourceUrl, note.SourceUrl);
                                    string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), note.SourceUrl.Length > 0 ? xmlSource : "");
                                    string xml = string.Format(m_xmlNewOutline, outlineContent, m_PageID, m_xmlns, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), xmlAttachments, note.Date.ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'"));
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
                catch (System.Xml.XmlException ex)
                {
                    // happens if the notebook was empty or does not exist.
                    // Or due to a parsing error if a note isn't properly xml encoded
                    // try to find the name of the note that's causing the problems
                    string notename = "";
                    if (xmltext.Length > 0)
                    {
                        var notematch = rxNote.Match(xmltext);
                        if (notematch.Groups.Count == 2)
                        {
                            notename = notematch.Groups[1].ToString();
                        }
                    }
                    if (notename.Length > 0)
                        MessageBox.Show(string.Format("Error parsing the note \"{2}\" in notebook \"{0}\",\n{1}", ENNotebookName, ex.ToString(), notename));
                    else
                        MessageBox.Show(string.Format("Error parsing the notebook \"{0}\"\n{1}", ENNotebookName, ex.ToString()));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception importing notes:\n{0}", ex.ToString()));
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
                // remove and/or replace characters that are not allowed in Onenote section names
                sectionName = sectionName.Replace("?", "");
                sectionName = sectionName.Replace("*", "");
                sectionName = sectionName.Replace("/", "");
                sectionName = sectionName.Replace("\\", "");
                sectionName = sectionName.Replace(":", "");
                sectionName = sectionName.Replace("<", "");
                sectionName = sectionName.Replace(">", "");
                sectionName = sectionName.Replace("|", "");
                sectionName = sectionName.Replace("&", "");
                sectionName = sectionName.Replace("#", "");
                sectionName = sectionName.Replace("\"", "'");
                sectionName = sectionName.Replace("%", "");

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

        private string SanitizeXml(string text)
        {
            //text = HttpUtility.HtmlDecode(text);
            Regex rxtitle = new Regex("<note><title>(.+)</title>", RegexOptions.IgnoreCase);
            var match = rxtitle.Match(text);
            if (match.Groups.Count == 2)
            {
                string title = match.Groups[1].ToString();
                title = title.Replace("&", "&amp;");
                title = title.Replace("\"", "&quot;");
                title = title.Replace("'", "&apos;");
                title = title.Replace("’", "&rsquo;");
                title = title.Replace("<", "&lt;");
                title = title.Replace(">", "&gt;");
                title = title.Replace("@", "&#64;");
                text = rxtitle.Replace(text, "<note><title>" + title + "</title>");
            }

            Regex rxauthor = new Regex("<author>(.+)</author>", RegexOptions.IgnoreCase);
            var authormatch = rxauthor.Match(text);
            if (match.Groups.Count == 2)
            {
                string author = authormatch.Groups[1].ToString();
                author = author.Replace("&", "&amp;");
                author = author.Replace("\"", "&quot;");
                author = author.Replace("'", "&apos;");
                author = author.Replace("’", "&rsquo;");
                author = author.Replace("<", "&lt;");
                author = author.Replace(">", "&gt;");
                author = author.Replace("@", "&#64;");
                text = rxauthor.Replace(text, "<author>" + author + "</author>");
            }

            Regex rxfilename = new Regex("<file-name>(.+)</file-name>", RegexOptions.IgnoreCase);
            var filenamematch = rxfilename.Match(text);
            if (match.Groups.Count == 2)
            {
                MatchEvaluator myEvaluator = new MatchEvaluator(FilenameMatchEvaluator);
                text = rxfilename.Replace(text, myEvaluator);
            }

            return text;
        }
        public string FilenameMatchEvaluator(Match m)
        {
            string filename = m.Groups[1].ToString();
            filename = filename.Replace("&nbsp;", " ");
            // remove illegal path chars
            string invalid = new string(Path.GetInvalidFileNameChars());
            foreach (char c in invalid)
            {
                filename = filename.Replace(c.ToString(), "");
            }
            filename = System.Security.SecurityElement.Escape(filename);
            return "<file-name>" + filename + "</file-name>";
        }
    }
}
