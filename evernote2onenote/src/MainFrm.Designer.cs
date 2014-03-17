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

namespace Evernote2Onenote
{
    partial class MainFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startsync = new System.Windows.Forms.Button();
            this.infoText1 = new System.Windows.Forms.Label();
            this.infoText2 = new System.Windows.Forms.Label();
            this.progressIndicator = new System.Windows.Forms.ProgressBar();
            this.infoText0 = new System.Windows.Forms.Label();
            this.homeLink = new System.Windows.Forms.LinkLabel();
            this.versionLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxENNotebookName = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(521, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // startsync
            // 
            this.startsync.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.startsync.Location = new System.Drawing.Point(147, 51);
            this.startsync.Name = "startsync";
            this.startsync.Size = new System.Drawing.Size(146, 23);
            this.startsync.TabIndex = 1;
            this.startsync.Text = "Start Import";
            this.startsync.UseVisualStyleBackColor = true;
            this.startsync.Click += new System.EventHandler(this.Startsync_Click);
            // 
            // infoText1
            // 
            this.infoText1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoText1.AutoSize = true;
            this.infoText1.Location = new System.Drawing.Point(9, 82);
            this.infoText1.Name = "infoText1";
            this.infoText1.Size = new System.Drawing.Size(10, 13);
            this.infoText1.TabIndex = 3;
            this.infoText1.Text = " ";
            // 
            // infoText2
            // 
            this.infoText2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.infoText2.AutoSize = true;
            this.infoText2.Location = new System.Drawing.Point(12, 82);
            this.infoText2.Name = "infoText2";
            this.infoText2.Size = new System.Drawing.Size(10, 13);
            this.infoText2.TabIndex = 4;
            this.infoText2.Text = " ";
            // 
            // progressIndicator
            // 
            this.progressIndicator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressIndicator.Location = new System.Drawing.Point(12, 106);
            this.progressIndicator.Name = "progressIndicator";
            this.progressIndicator.Size = new System.Drawing.Size(497, 23);
            this.progressIndicator.TabIndex = 5;
            // 
            // infoText0
            // 
            this.infoText0.AutoSize = true;
            this.infoText0.Location = new System.Drawing.Point(12, 56);
            this.infoText0.Name = "infoText0";
            this.infoText0.Size = new System.Drawing.Size(10, 13);
            this.infoText0.TabIndex = 6;
            this.infoText0.Text = " ";
            // 
            // homeLink
            // 
            this.homeLink.AutoSize = true;
            this.homeLink.Location = new System.Drawing.Point(339, 28);
            this.homeLink.Name = "homeLink";
            this.homeLink.Size = new System.Drawing.Size(59, 13);
            this.homeLink.TabIndex = 8;
            this.homeLink.TabStop = true;
            this.homeLink.Text = "Homepage";
            this.homeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.homeLink_LinkClicked);
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(404, 28);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(45, 13);
            this.versionLabel.TabIndex = 9;
            this.versionLabel.Text = "Version:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Evernote Notebook name:";
            // 
            // textBoxENNotebookName
            // 
            this.textBoxENNotebookName.Location = new System.Drawing.Point(147, 29);
            this.textBoxENNotebookName.Name = "textBoxENNotebookName";
            this.textBoxENNotebookName.Size = new System.Drawing.Size(146, 20);
            this.textBoxENNotebookName.TabIndex = 11;
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 141);
            this.Controls.Add(this.textBoxENNotebookName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.homeLink);
            this.Controls.Add(this.infoText0);
            this.Controls.Add(this.progressIndicator);
            this.Controls.Add(this.infoText2);
            this.Controls.Add(this.infoText1);
            this.Controls.Add(this.startsync);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximumSize = new System.Drawing.Size(537, 180);
            this.MinimumSize = new System.Drawing.Size(537, 180);
            this.Name = "MainFrm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Evernote2Onenote";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button startsync;
        private System.Windows.Forms.Label infoText1;
        private System.Windows.Forms.Label infoText2;
        private System.Windows.Forms.ProgressBar progressIndicator;
        private System.Windows.Forms.Label infoText0;
        private System.Windows.Forms.LinkLabel homeLink;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxENNotebookName;
    }
}

