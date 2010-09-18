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

namespace EveImSync
{
    partial class ConfigFrm
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
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.browseForEnscript = new System.Windows.Forms.Button();
            this.enscriptPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.deletePairButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okayButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.applyPairButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.imapNotebook = new System.Windows.Forms.TextBox();
            this.imapPassword = new System.Windows.Forms.TextBox();
            this.imapUsername = new System.Windows.Forms.TextBox();
            this.imapServer = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.evernoteNotebook = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pairList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.browseForEnscript);
            this.groupBox1.Controls.Add(this.enscriptPath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(437, 50);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Evernote";
            // 
            // browseForEnscript
            // 
            this.browseForEnscript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseForEnscript.Location = new System.Drawing.Point(391, 11);
            this.browseForEnscript.Name = "browseForEnscript";
            this.browseForEnscript.Size = new System.Drawing.Size(40, 23);
            this.browseForEnscript.TabIndex = 2;
            this.browseForEnscript.Text = "...";
            this.browseForEnscript.UseVisualStyleBackColor = true;
            this.browseForEnscript.Click += new System.EventHandler(this.BrowseForEnscript_Click);
            // 
            // enscriptPath
            // 
            this.enscriptPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.enscriptPath.Location = new System.Drawing.Point(118, 13);
            this.enscriptPath.Name = "enscriptPath";
            this.enscriptPath.Size = new System.Drawing.Size(267, 20);
            this.enscriptPath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path to ENScript.exe";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.deletePairButton);
            this.groupBox2.Controls.Add(this.cancelButton);
            this.groupBox2.Controls.Add(this.okayButton);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Controls.Add(this.evernoteNotebook);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.pairList);
            this.groupBox2.Location = new System.Drawing.Point(12, 68);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(437, 286);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sync Pairs";
            // 
            // deletePairButton
            // 
            this.deletePairButton.Location = new System.Drawing.Point(9, 221);
            this.deletePairButton.Name = "deletePairButton";
            this.deletePairButton.Size = new System.Drawing.Size(75, 23);
            this.deletePairButton.TabIndex = 1;
            this.deletePairButton.Text = "Delete";
            this.deletePairButton.UseVisualStyleBackColor = true;
            this.deletePairButton.Click += new System.EventHandler(this.DeletePairButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(356, 256);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okayButton
            // 
            this.okayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okayButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okayButton.Location = new System.Drawing.Point(275, 256);
            this.okayButton.Name = "okayButton";
            this.okayButton.Size = new System.Drawing.Size(75, 23);
            this.okayButton.TabIndex = 5;
            this.okayButton.Text = "&Ok";
            this.okayButton.UseVisualStyleBackColor = true;
            this.okayButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.applyPairButton);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.imapNotebook);
            this.groupBox3.Controls.Add(this.imapPassword);
            this.groupBox3.Controls.Add(this.imapUsername);
            this.groupBox3.Controls.Add(this.imapServer);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(151, 42);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(278, 202);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "IMAP";
            // 
            // applyPairButton
            // 
            this.applyPairButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.applyPairButton.Location = new System.Drawing.Point(180, 121);
            this.applyPairButton.Name = "applyPairButton";
            this.applyPairButton.Size = new System.Drawing.Size(75, 23);
            this.applyPairButton.TabIndex = 8;
            this.applyPairButton.Text = "Set Pair";
            this.applyPairButton.UseVisualStyleBackColor = true;
            this.applyPairButton.Click += new System.EventHandler(this.ApplyPairButton_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 98);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Base Folder:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Password:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Username:";
            // 
            // imapNotebook
            // 
            this.imapNotebook.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.imapNotebook.Location = new System.Drawing.Point(73, 95);
            this.imapNotebook.Name = "imapNotebook";
            this.imapNotebook.Size = new System.Drawing.Size(182, 20);
            this.imapNotebook.TabIndex = 7;
            // 
            // imapPassword
            // 
            this.imapPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.imapPassword.Location = new System.Drawing.Point(73, 69);
            this.imapPassword.Name = "imapPassword";
            this.imapPassword.PasswordChar = '*';
            this.imapPassword.Size = new System.Drawing.Size(182, 20);
            this.imapPassword.TabIndex = 5;
            // 
            // imapUsername
            // 
            this.imapUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.imapUsername.Location = new System.Drawing.Point(73, 43);
            this.imapUsername.Name = "imapUsername";
            this.imapUsername.Size = new System.Drawing.Size(182, 20);
            this.imapUsername.TabIndex = 3;
            // 
            // imapServer
            // 
            this.imapServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.imapServer.Location = new System.Drawing.Point(73, 17);
            this.imapServer.Name = "imapServer";
            this.imapServer.Size = new System.Drawing.Size(182, 20);
            this.imapServer.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Server:";
            // 
            // evernoteNotebook
            // 
            this.evernoteNotebook.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.evernoteNotebook.Location = new System.Drawing.Point(257, 16);
            this.evernoteNotebook.Name = "evernoteNotebook";
            this.evernoteNotebook.Size = new System.Drawing.Size(172, 20);
            this.evernoteNotebook.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(148, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Evernote Notebook:";
            // 
            // pairList
            // 
            this.pairList.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.pairList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.pairList.GridLines = true;
            this.pairList.HideSelection = false;
            this.pairList.Location = new System.Drawing.Point(9, 19);
            this.pairList.MultiSelect = false;
            this.pairList.Name = "pairList";
            this.pairList.Size = new System.Drawing.Size(133, 196);
            this.pairList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.pairList.TabIndex = 0;
            this.pairList.UseCompatibleStateImageBehavior = false;
            this.pairList.View = System.Windows.Forms.View.Details;
            this.pairList.SelectedIndexChanged += new System.EventHandler(this.PairList_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // ConfigFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 366);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MinimumSize = new System.Drawing.Size(473, 404);
            this.Name = "ConfigFrm";
            this.Text = "ConfigFrm";
            this.Load += new System.EventHandler(this.ConfigFrm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button browseForEnscript;
        private System.Windows.Forms.TextBox enscriptPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox imapNotebook;
        private System.Windows.Forms.TextBox imapPassword;
        private System.Windows.Forms.TextBox imapUsername;
        private System.Windows.Forms.TextBox imapServer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox evernoteNotebook;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView pairList;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okayButton;
        private System.Windows.Forms.Button applyPairButton;
        private System.Windows.Forms.Button deletePairButton;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}