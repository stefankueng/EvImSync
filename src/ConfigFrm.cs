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
using System.IO;
using System.Windows.Forms;

namespace EveImSync
{
    public partial class ConfigFrm : Form
    {
        Configuration config = null;

        public ConfigFrm()
        {
            InitializeComponent();
        }

        private void ConfigFrm_Load(object sender, EventArgs e)
        {
            config = Configuration.Create();
            if (config.ENScriptPath.Length > 0)
            {
                enscriptPath.Text = config.ENScriptPath;
            }
            else
            {
                // try to find the ENScript path ourselves
                string path = System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                path = Path.Combine(path, "Evernote");
                if (Directory.Exists(path))
                {
                    foreach (string d in Directory.GetDirectories(path))
                    {
                        string everPath = Path.Combine(path, d);
                        string enscrPath = Path.Combine(everPath, "ENScript.exe");
                        if (File.Exists(enscrPath))
                        {
                            enscriptPath.Text = enscrPath;
                            break;
                        }
                    }
                }
            }

            // now fill all the pairs in the list control
            foreach (SyncPairSettings sps in config.SyncPairs)
            {
                pairList.Items.Add(sps.EvernoteNotebook);
            }

            deletePairButton.Enabled = pairList.SelectedItems.Count != 0;
        }

        private void BrowseForEnscript_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ENScript.exe|ENScript.exe";
            openFileDialog.Title = "Select the ENScript.exe file in the Evernote Installation folder";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                enscriptPath.Text = openFileDialog.FileName;
            }
        }

        private void ApplyPairButton_Click(object sender, EventArgs e)
        {
            if (evernoteNotebook.Text.Length == 0)
            {
                errorProvider.SetError(evernoteNotebook, "Please enter a valid notebook name");
                return;
            }

            if ((imapNotebook.Text.Length == 0) || (imapNotebook.Text == "/"))
            {
                errorProvider.SetError(imapNotebook, "Please set a subfolder for syncing the notes");
                return;
            }

            SyncPairSettings sps = config.SyncPairs.Find(findPair => { return findPair.EvernoteNotebook == evernoteNotebook.Text; });
            if (sps == null)
            {
                sps = new SyncPairSettings();
            }
            else
            {
                config.SyncPairs.Remove(sps);
                sps.LastSyncTime = new DateTime(0);
            }

            sps.IMAPServer = imapServer.Text;
            sps.IMAPUsername = imapUsername.Text;
            sps.IMAPPassword = imapPassword.Text;
            sps.IMAPNotesFolder = imapNotebook.Text;
            sps.EvernoteNotebook = evernoteNotebook.Text;
            config.SyncPairs.Add(sps);

            pairList.Items.Clear();
            foreach (SyncPairSettings sp in config.SyncPairs)
            {
                pairList.Items.Add(sp.EvernoteNotebook);
            }

            pairList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            pairList.Update();
        }

        private void PairList_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in pairList.SelectedItems)
            {
                SyncPairSettings sps = config.SyncPairs.Find(findPair => { return findPair.EvernoteNotebook == item.Text; });
                if (sps != null)
                {
                    imapServer.Text = sps.IMAPServer;
                    imapUsername.Text = sps.IMAPUsername;
                    imapPassword.Text = sps.IMAPPassword;
                    imapNotebook.Text = sps.IMAPNotesFolder;
                    evernoteNotebook.Text = sps.EvernoteNotebook;
                }
            }

            deletePairButton.Enabled = pairList.SelectedItems.Count != 0;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            config.ENScriptPath = enscriptPath.Text;
            config.Save();
            this.Close();
        }

        private void DeletePairButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in pairList.SelectedItems)
            {
                SyncPairSettings sps = config.SyncPairs.Find(findPair => { return findPair.EvernoteNotebook == item.Text; });
                if (sps != null)
                {
                    config.SyncPairs.Remove(sps);
                }
            }

            pairList.Items.Clear();
            foreach (SyncPairSettings sp in config.SyncPairs)
            {
                pairList.Items.Add(sp.EvernoteNotebook);
            }

            pairList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            pairList.Update();
        }
    }
}
