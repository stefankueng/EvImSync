namespace ConfigGeneratorWin
{
    partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.passwordBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.passwordBox = new System.Windows.Forms.TextBox();
            this.usernameBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.hostName = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.logFileBrowse = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.logFileBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.binaryFormat = new System.Windows.Forms.RadioButton();
            this.xmlFormat = new System.Windows.Forms.RadioButton();
            this.browseButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.localCacheBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.rootFolderBox = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.autoSyncCacheBox = new System.Windows.Forms.CheckBox();
            this.autoLoad = new System.Windows.Forms.CheckBox();
            this.debugMode = new System.Windows.Forms.CheckBox();
            this.autoLogon = new System.Windows.Forms.CheckBox();
            this.useSSL = new System.Windows.Forms.CheckBox();
            this.saveBtn = new System.Windows.Forms.Button();
            this.closeBtn = new System.Windows.Forms.Button();
            this.debugDetailBox = new System.Windows.Forms.ComboBox();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(543, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newConfigToolStripMenuItem,
            this.openConfigToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newConfigToolStripMenuItem
            // 
            this.newConfigToolStripMenuItem.Name = "newConfigToolStripMenuItem";
            this.newConfigToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newConfigToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.newConfigToolStripMenuItem.Text = "New Config...";
            this.newConfigToolStripMenuItem.Click += new System.EventHandler(this.newConfigToolStripMenuItem_Click);
            // 
            // openConfigToolStripMenuItem
            // 
            this.openConfigToolStripMenuItem.Name = "openConfigToolStripMenuItem";
            this.openConfigToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openConfigToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.openConfigToolStripMenuItem.Text = "Open Config...";
            this.openConfigToolStripMenuItem.Click += new System.EventHandler(this.openConfigToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.passwordBox2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.passwordBox);
            this.groupBox1.Controls.Add(this.usernameBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.hostName);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(256, 205);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 152);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Password Verify";
            // 
            // passwordBox2
            // 
            this.passwordBox2.Location = new System.Drawing.Point(6, 168);
            this.passwordBox2.Name = "passwordBox2";
            this.passwordBox2.PasswordChar = '*';
            this.passwordBox2.Size = new System.Drawing.Size(243, 20);
            this.passwordBox2.TabIndex = 6;
            this.passwordBox2.TextChanged += new System.EventHandler(this.passwordBox2_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Password";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Username";
            // 
            // passwordBox
            // 
            this.passwordBox.Location = new System.Drawing.Point(7, 128);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.PasswordChar = '*';
            this.passwordBox.Size = new System.Drawing.Size(243, 20);
            this.passwordBox.TabIndex = 3;
            this.passwordBox.TextChanged += new System.EventHandler(this.passwordBox_TextChanged);
            // 
            // usernameBox
            // 
            this.usernameBox.Location = new System.Drawing.Point(7, 88);
            this.usernameBox.Name = "usernameBox";
            this.usernameBox.Size = new System.Drawing.Size(243, 20);
            this.usernameBox.TabIndex = 2;
            this.usernameBox.TextChanged += new System.EventHandler(this.usernameBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Host";
            // 
            // hostName
            // 
            this.hostName.Location = new System.Drawing.Point(7, 46);
            this.hostName.Name = "hostName";
            this.hostName.Size = new System.Drawing.Size(243, 20);
            this.hostName.TabIndex = 0;
            this.hostName.TextChanged += new System.EventHandler(this.hostName_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.logFileBrowse);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.logFileBox);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.binaryFormat);
            this.groupBox2.Controls.Add(this.xmlFormat);
            this.groupBox2.Controls.Add(this.browseButton);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.localCacheBox);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.rootFolderBox);
            this.groupBox2.Location = new System.Drawing.Point(275, 28);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(256, 204);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Defaults";
            // 
            // logFileBrowse
            // 
            this.logFileBrowse.Location = new System.Drawing.Point(171, 165);
            this.logFileBrowse.Name = "logFileBrowse";
            this.logFileBrowse.Size = new System.Drawing.Size(75, 23);
            this.logFileBrowse.TabIndex = 11;
            this.logFileBrowse.Text = "Browse";
            this.logFileBrowse.UseVisualStyleBackColor = true;
            this.logFileBrowse.Click += new System.EventHandler(this.logFileBrowse_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 151);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(44, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Log File";
            // 
            // logFileBox
            // 
            this.logFileBox.Location = new System.Drawing.Point(6, 167);
            this.logFileBox.Name = "logFileBox";
            this.logFileBox.Size = new System.Drawing.Size(159, 20);
            this.logFileBox.TabIndex = 9;
            this.logFileBox.TextChanged += new System.EventHandler(this.logFileBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 110);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Local Cache Format";
            // 
            // binaryFormat
            // 
            this.binaryFormat.AutoSize = true;
            this.binaryFormat.Location = new System.Drawing.Point(100, 128);
            this.binaryFormat.Name = "binaryFormat";
            this.binaryFormat.Size = new System.Drawing.Size(54, 17);
            this.binaryFormat.TabIndex = 7;
            this.binaryFormat.TabStop = true;
            this.binaryFormat.Text = "Binary";
            this.binaryFormat.UseVisualStyleBackColor = true;
            this.binaryFormat.CheckedChanged += new System.EventHandler(this.binaryFormat_CheckedChanged);
            // 
            // xmlFormat
            // 
            this.xmlFormat.AutoSize = true;
            this.xmlFormat.Location = new System.Drawing.Point(9, 128);
            this.xmlFormat.Name = "xmlFormat";
            this.xmlFormat.Size = new System.Drawing.Size(47, 17);
            this.xmlFormat.TabIndex = 6;
            this.xmlFormat.TabStop = true;
            this.xmlFormat.Text = "XML";
            this.xmlFormat.UseVisualStyleBackColor = true;
            this.xmlFormat.CheckedChanged += new System.EventHandler(this.xmlFormat_CheckedChanged);
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(171, 85);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 5;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Local Cache File";
            // 
            // localCacheBox
            // 
            this.localCacheBox.Location = new System.Drawing.Point(6, 87);
            this.localCacheBox.Name = "localCacheBox";
            this.localCacheBox.Size = new System.Drawing.Size(159, 20);
            this.localCacheBox.TabIndex = 3;
            this.localCacheBox.TextChanged += new System.EventHandler(this.localCacheBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Root Folder";
            // 
            // rootFolderBox
            // 
            this.rootFolderBox.Location = new System.Drawing.Point(6, 45);
            this.rootFolderBox.Name = "rootFolderBox";
            this.rootFolderBox.Size = new System.Drawing.Size(243, 20);
            this.rootFolderBox.TabIndex = 1;
            this.rootFolderBox.TextChanged += new System.EventHandler(this.rootFolderBox_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.debugDetailBox);
            this.groupBox3.Controls.Add(this.autoSyncCacheBox);
            this.groupBox3.Controls.Add(this.autoLoad);
            this.groupBox3.Controls.Add(this.debugMode);
            this.groupBox3.Controls.Add(this.autoLogon);
            this.groupBox3.Controls.Add(this.useSSL);
            this.groupBox3.Location = new System.Drawing.Point(12, 239);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(519, 131);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Options";
            // 
            // autoSyncCacheBox
            // 
            this.autoSyncCacheBox.AutoSize = true;
            this.autoSyncCacheBox.Location = new System.Drawing.Point(205, 27);
            this.autoSyncCacheBox.Name = "autoSyncCacheBox";
            this.autoSyncCacheBox.Size = new System.Drawing.Size(109, 17);
            this.autoSyncCacheBox.TabIndex = 4;
            this.autoSyncCacheBox.Text = "Auto Sync Cache";
            this.autoSyncCacheBox.UseVisualStyleBackColor = true;
            this.autoSyncCacheBox.CheckedChanged += new System.EventHandler(this.autoSyncCacheBox_CheckedChanged);
            // 
            // autoLoad
            // 
            this.autoLoad.AutoSize = true;
            this.autoLoad.Location = new System.Drawing.Point(25, 98);
            this.autoLoad.Name = "autoLoad";
            this.autoLoad.Size = new System.Drawing.Size(148, 17);
            this.autoLoad.TabIndex = 3;
            this.autoLoad.Text = "Auto Load Message UIDs";
            this.autoLoad.UseVisualStyleBackColor = true;
            this.autoLoad.CheckedChanged += new System.EventHandler(this.autoLoad_CheckedChanged);
            // 
            // debugMode
            // 
            this.debugMode.AutoSize = true;
            this.debugMode.Location = new System.Drawing.Point(25, 75);
            this.debugMode.Name = "debugMode";
            this.debugMode.Size = new System.Drawing.Size(88, 17);
            this.debugMode.TabIndex = 2;
            this.debugMode.Text = "Debug Mode";
            this.debugMode.UseVisualStyleBackColor = true;
            this.debugMode.CheckedChanged += new System.EventHandler(this.debugMode_CheckedChanged);
            // 
            // autoLogon
            // 
            this.autoLogon.AutoSize = true;
            this.autoLogon.Location = new System.Drawing.Point(25, 52);
            this.autoLogon.Name = "autoLogon";
            this.autoLogon.Size = new System.Drawing.Size(81, 17);
            this.autoLogon.TabIndex = 1;
            this.autoLogon.Text = "Auto Logon";
            this.autoLogon.UseVisualStyleBackColor = true;
            this.autoLogon.CheckedChanged += new System.EventHandler(this.autoLogon_CheckedChanged);
            // 
            // useSSL
            // 
            this.useSSL.AutoSize = true;
            this.useSSL.Location = new System.Drawing.Point(25, 29);
            this.useSSL.Name = "useSSL";
            this.useSSL.Size = new System.Drawing.Size(68, 17);
            this.useSSL.TabIndex = 0;
            this.useSSL.Text = "Use SSL";
            this.useSSL.UseVisualStyleBackColor = true;
            this.useSSL.CheckedChanged += new System.EventHandler(this.useSSL_CheckedChanged);
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(12, 381);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(75, 23);
            this.saveBtn.TabIndex = 4;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // closeBtn
            // 
            this.closeBtn.Location = new System.Drawing.Point(456, 381);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(75, 23);
            this.closeBtn.TabIndex = 5;
            this.closeBtn.Text = "Close";
            this.closeBtn.UseVisualStyleBackColor = true;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // debugDetailBox
            // 
            this.debugDetailBox.FormattingEnabled = true;
            this.debugDetailBox.Items.AddRange(new object[] {
            "<Select Debug Detail Level>",
            "All Events",
            "InterIMAP Events Only",
            "Errors Only"});
            this.debugDetailBox.Location = new System.Drawing.Point(205, 73);
            this.debugDetailBox.Name = "debugDetailBox";
            this.debugDetailBox.Size = new System.Drawing.Size(150, 21);
            this.debugDetailBox.TabIndex = 5;
            this.debugDetailBox.SelectedIndexChanged += new System.EventHandler(this.debugDetailBox_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 416);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "InterIMAP Configuration Editor";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openConfigToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox passwordBox;
        private System.Windows.Forms.TextBox usernameBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox hostName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox localCacheBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox rootFolderBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox useSSL;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton binaryFormat;
        private System.Windows.Forms.RadioButton xmlFormat;
        private System.Windows.Forms.CheckBox autoLogon;
        private System.Windows.Forms.CheckBox debugMode;
        private System.Windows.Forms.CheckBox autoLoad;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox passwordBox2;
        private System.Windows.Forms.CheckBox autoSyncCacheBox;
        private System.Windows.Forms.Button logFileBrowse;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox logFileBox;
        private System.Windows.Forms.ComboBox debugDetailBox;
    }
}

