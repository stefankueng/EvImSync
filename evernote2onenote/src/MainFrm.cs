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
using System.IO.Compression;
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

        private readonly Regex _rxStyle = new Regex("(?<text>\\<(?:pre|code|div|span|li|ul|ol|p|td|tr|table|tbody|h1|h2|a\\s+href=\\\"[^\\\"]*\\\").)\\s*style=\\\"[^\\\"]*\\\"", RegexOptions.IgnoreCase);
        private readonly Regex _rxFontFamily = new Regex(@"font-family: \""[^\""]*\""", RegexOptions.IgnoreCase);
        private readonly Regex _rxCdata = new Regex(@"<!\[CDATA\[<\?xml version=[""']1.0[""'][^?]*\?>", RegexOptions.IgnoreCase);
        private readonly Regex _rxCdata2 = new Regex(@"<!\[CDATA\[<!DOCTYPE en-note \w+ ""https?://xml.evernote.com/pub/enml2.dtd"">", RegexOptions.IgnoreCase);
        private readonly Regex _rxCdataInner = new Regex(@"\<\!\[CDATA\[(?<text>.*)\]\]\>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private readonly Regex _rxEmptyCdata = new Regex(@"<!\[CDATA\[<\?xml version=[""']1.0[""'][^?]*\?>[\s\n]+\]\]>", RegexOptions.IgnoreCase);
        private readonly Regex _rxEmptyCdata2 = new Regex(@"<!\[CDATA\[[\s\n]+\]\]>", RegexOptions.IgnoreCase);
        private readonly Regex _rxEmptyCdata3 = new Regex(@"<!\[CDATA\[>[\s\n]+\]\]>", RegexOptions.IgnoreCase);
        private readonly Regex _rxBodyStart = new Regex(@"<en-note[^>/]*>", RegexOptions.IgnoreCase);
        private readonly Regex _rxBodyEnd = new Regex(@"</en-note\s*>\s*]]>", RegexOptions.IgnoreCase);
        private readonly Regex _rxBodyEmpty = new Regex(@"<en-note[^>/]*/>\s*]]>", RegexOptions.IgnoreCase);
        private readonly Regex _rxDate = new Regex(@"^date:(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private readonly Regex _rxNote = new Regex("<title>(.+)</title>", RegexOptions.IgnoreCase);
        private readonly Regex _rxComment = new Regex("<!--(.+)-->", RegexOptions.IgnoreCase);
        private readonly Regex _rxDtd = new Regex(@"<!DOCTYPE en-note SYSTEM \""http:\/\/xml\.evernote\.com\/pub\/enml\d*\.dtd\"">", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public MainFrm(string cmdNotebook, string cmdDate)
        {
            InitializeComponent();
            _synchronizationContext = SynchronizationContext.Current;
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
            var fullpos = 0;

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
            if (btnENEXImport.Text == "Cancel")
            {
                _cancelled = true;
                return;
            }

            var openFileDialog1 = new OpenFileDialog();
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
                _onApp.GetHierarchy("", OneNote.HierarchyScope.hsNotebooks, out var xmlHierarchy);

                // Get the hierarchy for the default notebook folder
                try
                {
                    _onApp.GetSpecialLocation(OneNote.SpecialLocation.slDefaultNotebookFolder, out _evernoteNotebookPath);
                }
                catch (Exception)
                {
                    _onApp.GetSpecialLocation(OneNote.SpecialLocation.slUnfiledNotesSection, out _evernoteNotebookPath);
                }
                var nbName = _enNotebookName.Substring(0, Math.Min(30, _enNotebookName.Length)); // only allow 30 chars for the notebook name
                nbName = nbName.Replace(".", "");
                _evernoteNotebookPath += "\\" + nbName;
                _onApp.OpenHierarchy(_evernoteNotebookPath, "", out var newnbId, OneNote.CreateFileType.cftNotebook);
                _onApp.GetHierarchy(newnbId, OneNote.HierarchyScope.hsPages, out _);

                // Load and process the hierarchy
                var docHierarchy = new XmlDocument();
                docHierarchy.LoadXml(xmlHierarchy);
                var hierarchy = new StringBuilder();
                AppendHierarchy(docHierarchy.DocumentElement, hierarchy, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not create the target notebook in Onenote!\nTrying to use the \"unfiled notes\" section instead.\nThe error was:\n\n{ex}");

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
                    MessageBox.Show($"Could not create the target notebook in Onenote!\nMake sure you have the desktop version of OneNote installed, not the one from the Windows store!\n\n{ex2}");
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

            // shut down onedrive sync so it doesn't interfere with the import
            // get program files folder
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            // start the onedrive.exe process with the /shutdown parameter
            var onedrive = Path.Combine(programFiles, "Microsoft OneDrive", "OneDrive.exe");
            if (!File.Exists(onedrive))
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                onedrive = Path.Combine(localAppData, "Microsoft", "OneDrive", "OneDrive.exe");
            }
            if (File.Exists(onedrive))
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = onedrive;
                p.StartInfo.Arguments = "/shutdown";
                p.Start();
                p.WaitForExit();
            }

            if (!string.IsNullOrEmpty(_enexfile))
            {
                var notesEvernote = new List<Note>();
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

            if (File.Exists(onedrive))
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = onedrive;
                p.StartInfo.Arguments = "/background";
                p.Start();
            }

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
            var noteList = new List<Note>();
            if (_cancelled)
            {
                return noteList;
            }

            var xtrInput = new XmlTextReader(exportFile);
            var xmltext = "";
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

                        var xmlDocItem = new XmlDocument();
                        xmltext = SanitizeXml(xtrInput.ReadOuterXml());
                        xmlDocItem.LoadXml(xmltext);
                        var node = xmlDocItem.FirstChild;

                        // node is <note> element
                        // node.FirstChild.InnerText is <title>
                        node = node.FirstChild;

                        var note = new Note
                        {
                            Title = HttpUtility.HtmlDecode(node.InnerText)
                        };

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
                var notename = "";
                if (xmltext.Length > 0)
                {
                    var notematch = _rxNote.Match(xmltext);
                    if (notematch.Groups.Count == 2)
                    {
                        notename = notematch.Groups[1].ToString();
                    }
                }
                string tempfilepathDir = string.Empty;
                if (xmltext.Length > 0)
                    tempfilepathDir = ZipFailedNote(xmltext);

                MessageBox.Show(notename.Length > 0
                    ? $"Error parsing the note \"{notename}\" in notebook \"{_enNotebookName}\",\n{ex}\\n\\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues"
                    : $"Error parsing the notebook \"{_enNotebookName}\"\n{ex}\\n\\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues");
            }

            return noteList;
        }

        private static string ZipFailedNote(string xmltext)
        {
            var temppath = Path.GetTempPath() + "\\ev2on";
            var tempfilepathDir = temppath + "\\failedNotes";
            try
            {
                Directory.CreateDirectory(tempfilepathDir);
                var noteName = "note-" + Guid.NewGuid().ToString();
                var tempfilepath = tempfilepathDir + "\\";
                tempfilepath += noteName;
                tempfilepath += ".enex";
                xmltext = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE en-export SYSTEM \"http://xml.evernote.com/pub/evernote-export4.dtd\"><en-export>" + xmltext;
                xmltext = xmltext + "</en-export>";
                File.WriteAllText(tempfilepath, xmltext);
                var zipFilePath = tempfilepath + ".zip";
                using (ZipArchive zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(tempfilepath, noteName + ".enex");
                }
                File.Delete(tempfilepath);
            }
            catch (Exception)
            {
                // ignored
            }

            return tempfilepathDir;
        }

        private void ImportNotesToOnenote(List<Note> notesEvernote, string exportFile)
        {
            _syncStep = SyncStep.CalculateWhatToDo;
            var uploadcount = notesEvernote.Count;

            var temppath = Path.GetTempPath() + "\\ev2on";
            Directory.CreateDirectory(temppath);

            _syncStep = SyncStep.ImportNotes;
            var counter = 0;


            {
                var xmltext = "";
                try
                {
                    var xtrInput = new XmlTextReader(exportFile);
                    while (xtrInput.Read())
                    {
                        while ((xtrInput.NodeType == XmlNodeType.Element) && (xtrInput.Name.ToLower() == "note"))
                        {
                            if (_cancelled)
                            {
                                break;
                            }

                            var xmlDocItem = new XmlDocument();
                            xmltext = SanitizeXml(xtrInput.ReadOuterXml());
                            xmlDocItem.LoadXml(xmltext);
                            var node = xmlDocItem.FirstChild;

                            // node is <note> element
                            // node.FirstChild.InnerText is <title>
                            node = node.FirstChild;

                            var note = new Note
                            {
                                Title = HttpUtility.HtmlDecode(node.InnerText).Replace("&nbsp;", " ")
                            };
                            if (note.Title.StartsWith("=?"))
                                note.Title = Rfc2047Decoder.Parse(note.Title);
                            var contentElements = xmlDocItem.GetElementsByTagName("content");
                            if (contentElements.Count > 0)
                            {
                                node = contentElements[0];
                            }
                            note.Content = node.InnerXml;//HttpUtility.HtmlDecode(node.InnerXml);
                            if (note.Content.StartsWith("=?"))
                                note.Content = Rfc2047Decoder.Parse(note.Content);

                            var atts = xmlDocItem.GetElementsByTagName("resource");
                            foreach (XmlNode xmln in atts)
                            {
                                var attachment = new Attachment
                                {
                                    Base64Data = xmln.FirstChild.InnerText
                                };
                                var data = Convert.FromBase64String(xmln.FirstChild.InnerText);
                                var hash = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(data);
                                var hashHex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();

                                attachment.Hash = hashHex;

                                var fns = xmlDocItem.GetElementsByTagName("file-name");
                                if (fns.Count > note.Attachments.Count)
                                {
                                    attachment.FileName = HttpUtility.HtmlDecode(fns.Item(note.Attachments.Count).InnerText);
                                    if (attachment.FileName.StartsWith("=?"))
                                        attachment.FileName = Rfc2047Decoder.Parse(attachment.FileName);
                                    var invalid = new string(Path.GetInvalidFileNameChars());
                                    foreach (var c in invalid)
                                    {
                                        attachment.FileName = attachment.FileName.Replace(c.ToString(), "");
                                    }
                                    attachment.FileName = System.Security.SecurityElement.Escape(attachment.FileName);
                                }

                                var mimes = xmlDocItem.GetElementsByTagName("mime");
                                if (mimes.Count > note.Attachments.Count)
                                {
                                    attachment.ContentType = HttpUtility.HtmlDecode(mimes.Item(note.Attachments.Count).InnerText);
                                }

                                note.Attachments.Add(attachment);
                            }

                            var tagslist = xmlDocItem.GetElementsByTagName("tag");
                            foreach (XmlNode n in tagslist)
                            {
                                note.Tags.Add(HttpUtility.HtmlDecode(n.InnerText));
                            }

                            var datelist = xmlDocItem.GetElementsByTagName("created");
                            foreach (XmlNode n in datelist)
                            {
                                if (DateTime.TryParseExact(n.InnerText, "yyyyMMddTHHmmssZ", CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out var dateCreated))
                                {
                                    note.Date = dateCreated;
                                }
                            }
                            if (modifiedDateCheckbox.Checked)
                            {
                                var datelist2 = xmlDocItem.GetElementsByTagName("updated");
                                foreach (XmlNode n in datelist2)
                                {
                                    if (DateTime.TryParseExact(n.InnerText, "yyyyMMddTHHmmssZ", CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out var dateUpdated))
                                    {
                                        note.Date = dateUpdated;
                                    }
                                }
                            }

                            var sourceurl = xmlDocItem.GetElementsByTagName("source-url");
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

                            SetInfo(null, $"importing note ({counter + 1} of {uploadcount}) : \"{note.Title}\"", counter++, uploadcount);

                            var htmlBody = note.Content;

                            var tempfiles = new List<string>();
                            var xmlAttachments = "";
                            foreach (var attachment in note.Attachments)
                            {
                                // save the attached file
                                var tempfilepath = temppath + "\\";
                                var data = Convert.FromBase64String(attachment.Base64Data);
                                tempfilepath += attachment.Hash;
                                Stream fs = new FileStream(tempfilepath, FileMode.Create);
                                fs.Write(data, 0, data.Length);
                                fs.Close();
                                tempfiles.Add(tempfilepath);

                                var rx = new Regex(@"<en-media\b[^>]*?hash=""" + attachment.Hash + @"""[^>]*/>", RegexOptions.IgnoreCase);
                                var rxMatch = rx.Match(htmlBody);
                                if ((attachment.ContentType != null) && (attachment.ContentType.Contains("image") && rxMatch.Success))
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
                                        if (!string.IsNullOrEmpty(attachment.FileName))
                                        {
                                            // do not attach proxy.php image files: those are overlay images created by evernote to search text in images
                                            if (!attachment.ContentType.Contains("image") || attachment.FileName != "proxy.php")
                                                xmlAttachments +=
                                                    $"<one:InsertedFile pathSource=\"{tempfilepath}\" preferredName=\"{attachment.FileName}\" />";
                                        }
                                        else
                                            xmlAttachments +=
                                                $"<one:InsertedFile pathSource=\"{tempfilepath}\" preferredName=\"{attachment.Hash}\" />";
                                    }
                                }
                            }
                            note.Attachments.Clear();

                            htmlBody = _rxFontFamily.Replace(htmlBody, string.Empty);
                            htmlBody = _rxStyle.Replace(htmlBody, delegate (Match m)
                            {
                                if (m.Value.Contains("--en-codeblock:true;"))
                                    return m.Result("<br><br>${text}") + "style=\"background-color:#B0B0B0; font-family: Consolas, Courier New, monospace; font-size: 15px;\"";
                                return m.Result("${text}");
                            });
                            htmlBody = htmlBody.Replace("<pre>", "<br><br><pre style=\"font-family: Consolas, Courier New, monospace; font-size: 15px; background-color:#B0B0B0;\">");
                            htmlBody = htmlBody.Replace("</pre>", "</pre><br><br>");
                            htmlBody = _rxComment.Replace(htmlBody, string.Empty);
                            htmlBody = _rxEmptyCdata.Replace(htmlBody, string.Empty);
                            htmlBody = _rxEmptyCdata2.Replace(htmlBody, string.Empty);
                            htmlBody = _rxEmptyCdata3.Replace(htmlBody, string.Empty);
                            htmlBody = _rxCdata.Replace(htmlBody, string.Empty);
                            htmlBody = _rxCdata2.Replace(htmlBody, string.Empty);
                            htmlBody = _rxDtd.Replace(htmlBody, string.Empty);
                            htmlBody = _rxBodyStart.Replace(htmlBody, "<body>");
                            htmlBody = _rxBodyEnd.Replace(htmlBody, "</body>");
                            htmlBody = _rxBodyEmpty.Replace(htmlBody, "<body></body>");
                            htmlBody = htmlBody.Trim();
                            htmlBody = @"<!DOCTYPE html><head></head>" + htmlBody;

                            // Evernote does not escape < and > chars in <pre> sections!
                            // do that here so we don't get malformed xml
                            var rxPre = new Regex(@"<pre\b[^>]*?>(.+)</pre>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            foreach (Match match in rxPre.Matches(htmlBody))
                            {
                                var fullPreSection = match.ToString();
                                var innerSection = match.Groups[1].ToString();
                                var innserSectionCorrect = innerSection.Replace("<", "&lt;").Replace(">", "&gt;");
                                fullPreSection = fullPreSection.Replace(innerSection, innserSectionCorrect);
                                htmlBody = rxPre.Replace(htmlBody, fullPreSection);
                            }
                            var emailBody = htmlBody;
                            emailBody = _rxDate.Replace(emailBody, "Date: " + note.Date.ToString("ddd, dd MMM yyyy HH:mm:ss K"));
                            emailBody = emailBody.Replace("&apos;", "'");
                            emailBody = emailBody.Replace("’", "'");
                            emailBody = _rxCdataInner.Replace(emailBody, "&lt;![CDATA[${text}]]&gt;");
                            emailBody = emailBody.Replace("‘", "'");

                            try
                            {
                                var pageId = string.Empty;

                                // Get the hierarchy for all the notebooks
                                if ((note.Tags.Count > 0) && (!_useUnfiledSection))
                                {
                                    foreach (var tag in note.Tags)
                                    {
                                        var sectionId = GetSection(tag);
                                        try
                                        {
                                            _onApp.CreateNewPage(sectionId, out pageId, OneNote.NewPageStyle.npsBlankPageWithTitle);
                                        }
                                        catch (Exception)
                                        {
                                            sectionId = _useUnfiledSection ? _newnbId : GetSection("not specified");
                                            _onApp.CreateNewPage(sectionId, out pageId, OneNote.NewPageStyle.npsBlankPageWithTitle);
                                        }
                                        //_onApp.GetPageContent(pageId, out _);
                                        //OneNote uses HTML for the xml string to pass to the UpdatePageContent, so use the
                                        //Outlook HTMLBody property.  It coerces rtf and plain text to HTML.
                                        var outlineId = new Random().Next();
                                        //string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, m_outlineIDMetaName);
                                        var xmlSource = string.Format(XmlSourceUrl, note.SourceUrl);
                                        var outlineContent = string.Format(_xmlNewOutlineContent, emailBody, outlineId, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), note.SourceUrl.Length > 0 ? xmlSource : "");
                                        var xml = string.Format(XmlNewOutline, outlineContent, pageId, Xmlns, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), xmlAttachments, note.Date.ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'"));
                                        _onApp.UpdatePageContent(xml, DateTime.MinValue, OneNote.XMLSchema.xs2013, true);
                                    }
                                }
                                else
                                {
                                    var sectionId = _useUnfiledSection ? _newnbId : GetSection("not specified");
                                    _onApp.CreateNewPage(sectionId, out pageId, OneNote.NewPageStyle.npsBlankPageWithTitle);
                                    //string pages = string.Empty;
                                    //_onApp.GetHierarchy(sectionId, OneNote.HierarchyScope.hsPages, out pages);
                                    //string pageContent = string.Empty;
                                    //string testPageId = pageId;
                                    //_onApp.GetPageContent(testPageId, out pageContent);

                                    //OneNote uses HTML for the xml string to pass to the UpdatePageContent, so use the
                                    //Outlook HTMLBody property.  It coerces rtf and plain text to HTML.
                                    var outlineId = new Random().Next();
                                    //string outlineContent = string.Format(m_xmlNewOutlineContent, emailBody, outlineID, m_outlineIDMetaName);
                                    var xmlSource = string.Format(XmlSourceUrl, note.SourceUrl);
                                    var outlineContent = string.Format(_xmlNewOutlineContent, emailBody, outlineId, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), note.SourceUrl.Length > 0 ? xmlSource : "");
                                    var xml = string.Format(XmlNewOutline, outlineContent, pageId, Xmlns, System.Security.SecurityElement.Escape(note.Title).Replace("&apos;", "'"), xmlAttachments, note.Date.ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'Z'"));
                                    _onApp.UpdatePageContent(xml, DateTime.MinValue, OneNote.XMLSchema.xs2013, true);
                                }
                                _onApp.SyncHierarchy(pageId);
                            }
                            catch (Exception ex)
                            {
                                var tempfilepathDir = ZipFailedNote(xmltext);

                                MessageBox.Show($"Note:{note.Title}\n{ex}\n\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues");
                            }

                            foreach (var p in tempfiles)
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
                    var notename = "";
                    if (xmltext.Length > 0)
                    {
                        var notematch = _rxNote.Match(xmltext);
                        if (notematch.Groups.Count == 2)
                        {
                            notename = notematch.Groups[1].ToString();
                        }
                    }
                    string tempfilepathDir = string.Empty;
                    if (xmltext.Length > 0)
                        tempfilepathDir = ZipFailedNote(xmltext);

                    MessageBox.Show(notename.Length > 0
                        ? $"Error parsing the note \"{notename}\" in notebook \"{_enNotebookName}\",\n{ex}\\n\\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues"
                        : $"Error parsing the notebook \"{_enNotebookName}\"\n{ex}\\n\\nA copy of the note is left in {tempfilepathDir}. If you want to help fix the problem, please consider creating an issue and attaching that note to it: https://github.com/stefankueng/EvImSync/issues");
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
                    var id = xml.Attributes != null && xml.LocalName == "Section" && xml.Attributes["path"].Value == _evernoteNotebookPath
                        ? "UnfiledNotes"
                        : xml.Attributes["ID"].Value;
                    var name = HttpUtility.HtmlEncode(xml.Attributes["name"].Value);
                    if (str.Length > 0)
                        str.Append("\n");
                    str.Append($"{level.ToString()} {xml.LocalName} {id} {name}");
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
            var newnbId = "";
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
                sectionName = sectionName.Trim('.');
                if (sectionName.Length > 80)
                    sectionName = sectionName.Substring(0, 80);

                _onApp.GetHierarchy("", OneNote.HierarchyScope.hsNotebooks, out var xmlHierarchy);

                _onApp.OpenHierarchy(_evernoteNotebookPath + "\\" + sectionName + ".one", "", out newnbId, OneNote.CreateFileType.cftSection);
                _onApp.GetHierarchy(newnbId, OneNote.HierarchyScope.hsSections, out _);

                // Load and process the hierarchy
                var docHierarchy = new XmlDocument();
                docHierarchy.LoadXml(xmlHierarchy);
                var hierarchy = new StringBuilder(sectionName);
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
            var rxtitle = new Regex("<note><title>(.+)</title>", RegexOptions.IgnoreCase);
            var match = rxtitle.Match(text);
            if (match.Groups.Count == 2)
            {
                var title = match.Groups[1].ToString();
                title = title.Replace("&", "&amp;");
                title = title.Replace("\"", "&quot;");
                title = title.Replace("'", "&apos;");
                title = title.Replace("’", "&apos;");
                title = title.Replace("<", "&lt;");
                title = title.Replace(">", "&gt;");
                title = title.Replace("@", "&#64;");
                text = rxtitle.Replace(text, "<note><title>" + title + "</title>");
            }

            var rxauthor = new Regex("<author>(.+)</author>", RegexOptions.IgnoreCase);
            var authormatch = rxauthor.Match(text);
            if (match.Groups.Count == 2)
            {
                var author = authormatch.Groups[1].ToString();
                author = author.Replace("&", "&amp;");
                author = author.Replace("\"", "&quot;");
                author = author.Replace("'", "&apos;");
                author = author.Replace("’", "&apos;");
                author = author.Replace("<", "&lt;");
                author = author.Replace(">", "&gt;");
                author = author.Replace("@", "&#64;");
                text = rxauthor.Replace(text, "<author>" + author + "</author>");
            }

            var rxfilename = new Regex("<file-name>(.+)</file-name>", RegexOptions.IgnoreCase);
            if (match.Groups.Count == 2)
            {
                MatchEvaluator myEvaluator = FilenameMatchEvaluator;
                text = rxfilename.Replace(text, myEvaluator);
            }

            var rxSrcUrl = new Regex("<source-url>(.+)</source-url>", RegexOptions.IgnoreCase);
            if (match.Groups.Count == 2)
            {
                MatchEvaluator myEvaluator = FilenameMatchEvaluator;
                text = rxSrcUrl.Replace(text, myEvaluator);
            }

            return text;
        }

        private string FilenameMatchEvaluator(Match m)
        {
            var filename = m.Groups[1].ToString();
            filename = filename.Replace("&nbsp;", " ");
            // remove illegal path chars
            var invalid = new string(Path.GetInvalidFileNameChars());
            foreach (var c in invalid)
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
