// Evernote2Onenote - imports Evernote notes to Onenote
// Copyright (C) 2014, 2023 - Stefan Kueng

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
        private string _evernoteNotebookPath;
        private readonly SynchronizationContext _synchronizationContext;
        private bool _cancelled;
        private SyncStep _syncStep = SyncStep.Start;
        private Microsoft.Office.Interop.OneNote.Application _onApp;
        private readonly string _xmlNewOutlineContent =
            "<one:Meta name=\"{2}\" content=\"{1}\"/>" +
            "<one:OEChildren><one:HTMLBlock><one:Data><![CDATA[{0}]]></one:Data></one:HTMLBlock>{3}</one:OEChildren>";

        private const string XmlSourceUrl = "<one:OE alignment=\"left\" quickStyleIndex=\"2\"><one:T><![CDATA[From &lt;<a href=\"{0}\">{0}</a>&gt; ]]></one:T></one:OE>";

        private const string XmlNewOutline = "<?xml version=\"1.0\"?>" + "<one:Page xmlns:one=\"{2}\" ID=\"{1}\" dateTime=\"{5}\">" + "<one:Title selected=\"partial\" lang=\"en-US\">" + "<one:OE creationTime=\"{5}\" lastModifiedTime=\"{5}\">" + "<one:T><![CDATA[{3}]]></one:T> " + "</one:OE>" + "</one:Title>{4}" + "<one:Outline>{0}</one:Outline></one:Page>";

        private const string Xmlns = "http://schemas.microsoft.com/office/onenote/2013/onenote";
        private string _enNotebookName = "";
        private bool _useUnfiledSection;
        private string _enexfile = "";
        string _newnbId = "";

        private readonly string _cmdNoteBook = "";
        private DateTime _cmdDate = new DateTime(0);

        private readonly Regex _rxStyle = new Regex("(?<text>\\<div.)style=\\\"[^\\\"]*\\\"", RegexOptions.IgnoreCase);
        private readonly Regex _rxCdata = new Regex(@"<!\[CDATA\[<\?xml version=[""']1.0[""'][^?]*\?>", RegexOptions.IgnoreCase);
        private readonly Regex _rxCdataInner = new Regex(@"\<\!\[CDATA\[(?<text>.*)\]\]\>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private readonly Regex _rxBodyStart = new Regex(@"<en-note[^>/]*>", RegexOptions.IgnoreCase);
        private readonly Regex _rxBodyEnd = new Regex(@"</en-note\s*>\s*]]>", RegexOptions.IgnoreCase);
        private readonly Regex _rxBodyEmpty = new Regex(@"<en-note[^>/]*/>\s*]]>", RegexOptions.IgnoreCase);
        private readonly Regex _rxDate = new Regex(@"^date:(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private readonly Regex _rxNote = new Regex("<title>(.+)</title>", RegexOptions.IgnoreCase);
        private readonly Regex _rxComment = new Regex("<!--(.+)-->", RegexOptions.IgnoreCase);
        private static readonly Regex RxDtd = new Regex(@"<!DOCTYPE en-note SYSTEM \""http:\/\/xml\.evernote\.com\/pub\/enml\d*\.dtd\"">", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public MainFrm(string cmdNotebook, string cmdDate)
        {
            InitializeComponent();
            _synchronizationContext = SynchronizationContext.Current;
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            versionLabel.Text = $@"Version: {version}";

            if (cmdNotebook.Length > 0)
                _cmdNoteBook = cmdNotebook;
            if (cmdDate.Length > 0)
            {
                try
                {
                    _cmdDate = DateTime.Parse(cmdDate);
                }
                catch (Exception)
                {
                    MessageBox.Show($"The Datestring\n{cmdDate}\nis not valid!");
                }
            }
            try
            {
                importDatePicker.Value = _cmdDate;
            }
            catch (Exception)
            {
                importDatePicker.Value = importDatePicker.MinDate;
            }

            if (cmdNotebook.Length > 0)
            {
                StartSync();
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void SetInfo(string line1, string line2, int pos, int max)
        {
            int fullpos = 0;

            switch (_syncStep)
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

            _synchronizationContext.Send(delegate
            {
                if (line1 != null)
                    infoText1.Text = line1;
                if (line2 != null)
                    infoText2.Text = line2;
                progressIndicator.Minimum = 0;
                progressIndicator.Maximum = 100000;
                progressIndicator.Value = fullpos;
            }, null);

            if (max == 0)
                _syncStep++;
        }

        private void btnENEXImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = @"Evernote exports|*.enex";
            openFileDialog1.Title = @"Select the ENEX file";
            openFileDialog1.CheckPathExists = true;

            // Show the Dialog.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _enexfile = openFileDialog1.FileName;
                StartSync();
            }
        }

        private void StartSync()
        {
            if (!string.IsNullOrEmpty(_enexfile))
            {
                _enNotebookName = Path.GetFileNameWithoutExtension(_enexfile);
            }
            if (_cmdNoteBook.Length > 0)
                _enNotebookName = _cmdNoteBook;

            try
            {
                _onApp = new OneNote.Application();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not connect to Onenote!\nReasons for this might be:\n* The desktop version of onenote is not installed\n* Onenote is not installed properly\n* Onenote is already running but with a different user account\n\n{ex}");
                return;
            }
            if (_onApp == null)
            {
                MessageBox.Show("Could not connect to Onenote!\nReasons for this might be:\n* The desktop version of onenote is not installed\n* Onenote is not installed properly\n* Onenote is already running but with a different user account\n");
                return;
            }
            // create a new notebook named "EverNote"
            try
            {
                string xmlHierarchy;
                _onApp.GetHierarchy("", OneNote.HierarchyScope.hsNotebooks, out xmlHierarchy);

                // Get the hierarchy for the default notebook folder
                _onApp.GetSpecialLocation(OneNote.SpecialLocation.slDefaultNotebookFolder, out _evernoteNotebookPath);
                _evernoteNotebookPath += "\\" + _enNotebookName;
                string newnbId;
                _onApp.OpenHierarchy(_evernoteNotebookPath, "", out newnbId, OneNote.CreateFileType.cftNotebook);
                _onApp.GetHierarchy(newnbId, OneNote.HierarchyScope.hsPages, out _);

                // Load and process the hierarchy
                XmlDocument docHierarchy = new XmlDocument();
                docHierarchy.LoadXml(xmlHierarchy);
                StringBuilder hierarchy = new StringBuilder();
                AppendHierarchy(docHierarchy.DocumentElement, hierarchy, 0);
            }
            catch (Exception)
            {
                try
                {
                    _onApp.GetHierarchy("", OneNote.HierarchyScope.hsPages, out _);

                    // Get the hierarchy for the default notebook folder
                    _onApp.GetSpecialLocation(OneNote.SpecialLocation.slUnfiledNotesSection, out _evernoteNotebookPath);
                    _onApp.OpenHierarchy(_evernoteNotebookPath, "", out _newnbId);
                    _onApp.GetHierarchy(_newnbId, OneNote.HierarchyScope.hsPages, out _);
                    _useUnfiledSection = true;
                }
                catch (Exception ex2)
                {
                    MessageBox.Show($"Could not create the target notebook in Onenote!\n{ex2}");
                    return;
                }
            }

            if (importDatePicker.Value > _cmdDate)
                _cmdDate = importDatePicker.Value;

            if (btnENEXImport.Text == "Import ENEX File")
            {
                btnENEXImport.Text = "Cancel";
                MethodInvoker syncDelegate = ImportNotesToOnenote;
                syncDelegate.BeginInvoke(null, null);
            }
            else
            {
                _cancelled = true;
            }
        }

        private void ImportNotesToOnenote()
        {
            _syncStep = SyncStep.Start;
            if (!string.IsNullOrEmpty(_enexfile))
            {
                List<Note> notesEvernote = new List<Note>();
                if (_enexfile != string.Empty)
                {
                    SetInfo("Parsing notes from Evernote", "", 0, 0);
                    notesEvernote = ParseNotes(_enexfile);
                }
                if (_enexfile != string.Empty)
                {
                    SetInfo("importing notes to Onenote", "", 0, 0);
                    ImportNotesToOnenote(notesEvernote, _enexfile);
                }
            }

            _enexfile = "";
            if (_cancelled)
            {
                SetInfo(null, "Operation cancelled", 0, 0);
            }
            else
                SetInfo("", "", 0, 0);

            _synchronizationContext.Send(delegate
            {
                btnENEXImport.Text = @"Import ENEX File";
                infoText1.Text = @"Finished";
                progressIndicator.Minimum = 0;
                progressIndicator.Maximum = 100000;
                progressIndicator.Value = 0;
            }, null);
            if (_cmdNoteBook.Length > 0)
            {
                _synchronizationContext.Send(delegate
                {
                    Close();
                }, null);
            }
        }

        private List<Note> ParseNotes(string exportFile)
        {
            _syncStep = SyncStep.ParseNotes;
            List<Note> noteList = new List<Note>();
            if (_cancelled)
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
                        if (_cancelled)
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
            catch (XmlException ex)
            {
                // happens if the notebook was empty or does not exist.
                // Or due to a parsing error if a note isn't properly xml encoded
                // try to find the name of the note that's causing the problems
                string notename = "";
                if (xmltext.Length > 0)
                {
                    var notematch = _rxNote.Match(xmltext);
                    if (notematch.Groups.Count == 2)
                    {
                        notename = notematch.Groups[1].ToString();
                    }
                }
                string temppath = Path.GetTempPath() + "\\ev2on";
                string tempfilepathDir = temppath + "\\failedNotes";
                try
                {
                    Directory.CreateDirectory(tempfilepathDir);
                    string tempfilepath = tempfilepathDir + "\\note-";
                    tempfilepath += Guid.NewGuid().ToString();
                    tempfilepath += ".xml";
                    File.WriteAllText(tempfilepath, xmltext);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (notename.Length > 0)
                    MessageBox.Show($"Error parsing the note \"{notename}\" in notebook \"{_enNotebookName}\",\n{ex}\\n\\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues");
                else
                    MessageBox.Show($"Error parsing the notebook \"{_enNotebookName}\"\n{ex}\\n\\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues");
            }

            return noteList;
        }

        private void ImportNotesToOnenote(List<Note> notesEvernote, string exportFile)
        {
            _syncStep = SyncStep.CalculateWhatToDo;
            int uploadcount = notesEvernote.Count;

            string temppath = Path.GetTempPath() + "\\ev2on";
            Directory.CreateDirectory(temppath);

            _syncStep = SyncStep.ImportNotes;
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
                            if (_cancelled)
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
                            if (note.Title.StartsWith("=?"))
                                note.Title = Rfc2047Decoder.Parse(note.Title);
                            var contentElements = xmlDocItem.GetElementsByTagName("content");
                            if (contentElements.Count > 0)
                            {
                                node = contentElements[0];
                            }
                            note.Content = HttpUtility.HtmlDecode(node.InnerXml);
                            if (note.Content.StartsWith("=?"))
                                note.Content = Rfc2047Decoder.Parse(note.Content);

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
                                    if (attachment.FileName.StartsWith("=?"))
                                        attachment.FileName = Rfc2047Decoder.Parse(attachment.FileName);
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
                                if (DateTime.TryParseExact(n.InnerText, "yyyyMMddTHHmmssZ", CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out var dateCreated))
                                {
                                    note.Date = dateCreated;
                                }
                            }
                            if (modifiedDateCheckbox.Checked)
                            {
                                XmlNodeList datelist2 = xmlDocItem.GetElementsByTagName("updated");
                                foreach (XmlNode n in datelist2)
                                {
                                    if (DateTime.TryParseExact(n.InnerText, "yyyyMMddTHHmmssZ", CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out var dateUpdated))
                                    {
                                        note.Date = dateUpdated;
                                    }
                                }
                            }

                            XmlNodeList sourceurl = xmlDocItem.GetElementsByTagName("source-url");
                            note.SourceUrl = "";
                            foreach (XmlNode n in sourceurl)
                            {
                                try
                                {
                                    if (n.InnerText.StartsWith("file://"))
                                        continue;
                                    if (n.InnerText.StartsWith("en-cache://"))
                                        continue;
                                    note.SourceUrl = n.InnerText;
                                }
                                catch (FormatException)
                                {
                                }
                            }

                            if (_cmdDate > note.Date)
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

                            htmlBody = _rxStyle.Replace(htmlBody, "${text}");
                            htmlBody = _rxComment.Replace(htmlBody, string.Empty);
                            htmlBody = _rxCdata.Replace(htmlBody, string.Empty);
                            htmlBody = RxDtd.Replace(htmlBody, string.Empty);
                            htmlBody = _rxBodyStart.Replace(htmlBody, "<body>");
                            htmlBody = _rxBodyEnd.Replace(htmlBody, "</body>");
                            htmlBody = _rxBodyEmpty.Replace(htmlBody, "<body></body>");
                            htmlBody = htmlBody.Trim();
                            htmlBody = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN""><head></head>" + htmlBody;

                            string emailBody = htmlBody;
                            emailBody = _rxDate.Replace(emailBody, "Date: " + note.Date.ToString("ddd, dd MMM yyyy HH:mm:ss K"));
                            emailBody = emailBody.Replace("&apos;", "'");
                            emailBody = emailBody.Replace("’", "'");
                            emailBody = _rxCdataInner.Replace(emailBody, "&lt;![CDATA[${text}]]&gt;");
                            emailBody = emailBody.Replace("‘", "'");

                            try
                            {
                                string pageId = string.Empty;

                                // Get the hierarchy for all the notebooks
                                if ((note.Tags.Count > 0) && (!_useUnfiledSection))
                                {
                                    foreach (string tag in note.Tags)
                                    {
                                        string sectionId = GetSection(tag);
                                        _onApp.CreateNewPage(sectionId, out pageId, OneNote.NewPageStyle.npsBlankPageWithTitle);
                                        //_onApp.GetPageContent(pageId, out _);
                                        //OneNote uses HTML for the xml string to pass to the UpdatePageContent, so use the
                                        //Outlook HTMLBody property.  It coerces rtf and plain text to HTML.
                                        int outlineId = new Random().Next();
                                        //string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, m_outlineIDMetaName);
                                        string xmlSource = string.Format(XmlSourceUrl, note.SourceUrl);
                                        string outlineContent = string.Format(_xmlNewOutlineContent, emailBody, outlineId, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), note.SourceUrl.Length > 0 ? xmlSource : "");
                                        string xml = string.Format(XmlNewOutline, outlineContent, pageId, Xmlns, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), xmlAttachments, note.Date.ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'"));
                                        _onApp.UpdatePageContent(xml, DateTime.MinValue, OneNote.XMLSchema.xs2013, true);
                                    }
                                }
                                else
                                {
                                    string sectionId = _useUnfiledSection ? _newnbId : GetSection("not specified");
                                    _onApp.CreateNewPage(sectionId, out pageId, OneNote.NewPageStyle.npsBlankPageWithTitle);
                                    //_onApp.GetPageContent(pageId, out _);
                                    //OneNote uses HTML for the xml string to pass to the UpdatePageContent, so use the
                                    //Outlook HTMLBody property.  It coerces rtf and plain text to HTML.
                                    int outlineId = new Random().Next();
                                    //string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, m_outlineIDMetaName);
                                    string xmlSource = string.Format(XmlSourceUrl, note.SourceUrl);
                                    string outlineContent = string.Format(_xmlNewOutlineContent, emailBody, outlineId, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), note.SourceUrl.Length > 0 ? xmlSource : "");
                                    string xml = string.Format(XmlNewOutline, outlineContent, pageId, Xmlns, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), xmlAttachments, note.Date.ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'"));
                                    _onApp.UpdatePageContent(xml, DateTime.MinValue, OneNote.XMLSchema.xs2013, true);
                                }
                                _onApp.SyncHierarchy(pageId);
                            }
                            catch (Exception ex)
                            {
                                string tempfilepathDir = temppath + "\\failedNotes";
                                try
                                {
                                    Directory.CreateDirectory(tempfilepathDir);
                                    string tempfilepath = tempfilepathDir + "\\note-";
                                    tempfilepath += Guid.NewGuid().ToString();
                                    tempfilepath += ".xml";
                                    File.WriteAllText(tempfilepath, xmltext);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                                MessageBox.Show($"Note:{note.Title}\n{ex}\n\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues");
                            }

                            foreach (string p in tempfiles)
                            {
                                File.Delete(p);
                            }
                        }
                    }

                    xtrInput.Close();
                }
                catch (XmlException ex)
                {
                    // happens if the notebook was empty or does not exist.
                    // Or due to a parsing error if a note isn't properly xml encoded
                    // try to find the name of the note that's causing the problems
                    string notename = "";
                    if (xmltext.Length > 0)
                    {
                        var notematch = _rxNote.Match(xmltext);
                        if (notematch.Groups.Count == 2)
                        {
                            notename = notematch.Groups[1].ToString();
                        }
                    }
                    string tempfilepathDir = temppath + "\\failedNotes";
                    try
                    {
                        Directory.CreateDirectory(tempfilepathDir);
                        string tempfilepath = tempfilepathDir + "\\note-";
                        tempfilepath += Guid.NewGuid().ToString();
                        tempfilepath += ".xml";
                        File.WriteAllText(tempfilepath, xmltext);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (notename.Length > 0)
                        MessageBox.Show($"Error parsing the note \"{notename}\" in notebook \"{_enNotebookName}\",\n{ex}\\n\\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues");
                    else
                        MessageBox.Show($"Error parsing the notebook \"{_enNotebookName}\"\n{ex}\\n\\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Exception importing notes:\n{ex}");
                }
            }
            _onApp = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private void AppendHierarchy(XmlNode xml, StringBuilder str, int level)
        {
            // The set of elements that are themselves meaningful to export:
            if (xml.Name == "one:Notebook" || xml.Name == "one:SectionGroup" || xml.Name == "one:Section" || xml.Name == "one:Page")
            {
                if (xml.Attributes != null)
                {
                    string id = xml.Attributes != null && xml.LocalName == "Section" && xml.Attributes["path"].Value == _evernoteNotebookPath
                        ? "UnfiledNotes"
                        : xml.Attributes["ID"].Value;
                    string name = HttpUtility.HtmlEncode(xml.Attributes["name"].Value);
                    if (str.Length > 0)
                        str.Append("\n");
                    str.Append(string.Format("{0} {1} {2} {3}", level.ToString(), xml.LocalName, id, name));
                }
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
            System.Diagnostics.Process.Start("https://tools.stefankueng.com/Evernote2Onenote.html");
        }
        private string GetSection(string sectionName)
        {
            string newnbId = "";
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
                _onApp.GetHierarchy("", OneNote.HierarchyScope.hsNotebooks, out xmlHierarchy);

                _onApp.OpenHierarchy(_evernoteNotebookPath + "\\" + sectionName + ".one", "", out newnbId, OneNote.CreateFileType.cftSection);
                _onApp.GetHierarchy(newnbId, OneNote.HierarchyScope.hsSections, out _);

                // Load and process the hierarchy
                XmlDocument docHierarchy = new XmlDocument();
                docHierarchy.LoadXml(xmlHierarchy);
                StringBuilder hierarchy = new StringBuilder(sectionName);
                AppendHierarchy(docHierarchy.DocumentElement, hierarchy, 0);
            }
            catch (Exception /*ex*/)
            {
                //MessageBox.Show(string.Format("Exception creating section \"{0}\":\n{1}", sectionName, ex.ToString()));
            }
            return newnbId;
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
                title = title.Replace("’", "&apos;");
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
                author = author.Replace("’", "&apos;");
                author = author.Replace("<", "&lt;");
                author = author.Replace(">", "&gt;");
                author = author.Replace("@", "&#64;");
                text = rxauthor.Replace(text, "<author>" + author + "</author>");
            }

            Regex rxfilename = new Regex("<file-name>(.+)</file-name>", RegexOptions.IgnoreCase);
            if (match.Groups.Count == 2)
            {
                MatchEvaluator myEvaluator = FilenameMatchEvaluator;
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

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_onApp != null)
            {
                _cancelled = true;
            }
            _onApp = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
