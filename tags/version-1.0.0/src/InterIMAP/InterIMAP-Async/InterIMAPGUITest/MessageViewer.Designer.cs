namespace InterIMAPGUITest
{
    partial class MessageViewer
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
            this.toList = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.fromList = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ccList = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.bccList = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.subjectBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dateBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.viewSelector = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textDataBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.attachmentList = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.seenBox = new System.Windows.Forms.CheckBox();
            this.draftBox = new System.Windows.Forms.CheckBox();
            this.deletedBox = new System.Windows.Forms.CheckBox();
            this.flaggedBox = new System.Windows.Forms.CheckBox();
            this.answeredBox = new System.Windows.Forms.CheckBox();
            this.recentBox = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toList
            // 
            this.toList.Location = new System.Drawing.Point(51, 13);
            this.toList.Name = "toList";
            this.toList.ReadOnly = true;
            this.toList.Size = new System.Drawing.Size(544, 20);
            this.toList.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "To";
            // 
            // fromList
            // 
            this.fromList.Location = new System.Drawing.Point(51, 39);
            this.fromList.Name = "fromList";
            this.fromList.ReadOnly = true;
            this.fromList.Size = new System.Drawing.Size(544, 20);
            this.fromList.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "From";
            // 
            // ccList
            // 
            this.ccList.Location = new System.Drawing.Point(51, 65);
            this.ccList.Name = "ccList";
            this.ccList.ReadOnly = true;
            this.ccList.Size = new System.Drawing.Size(544, 20);
            this.ccList.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(21, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "CC";
            // 
            // bccList
            // 
            this.bccList.Location = new System.Drawing.Point(51, 91);
            this.bccList.Name = "bccList";
            this.bccList.ReadOnly = true;
            this.bccList.Size = new System.Drawing.Size(544, 20);
            this.bccList.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "BCC";
            // 
            // subjectBox
            // 
            this.subjectBox.Location = new System.Drawing.Point(51, 117);
            this.subjectBox.Name = "subjectBox";
            this.subjectBox.ReadOnly = true;
            this.subjectBox.Size = new System.Drawing.Size(544, 20);
            this.subjectBox.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Subject";
            // 
            // dateBox
            // 
            this.dateBox.Location = new System.Drawing.Point(51, 143);
            this.dateBox.Name = "dateBox";
            this.dateBox.ReadOnly = true;
            this.dateBox.Size = new System.Drawing.Size(128, 20);
            this.dateBox.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 146);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Date";
            // 
            // viewSelector
            // 
            this.viewSelector.FormattingEnabled = true;
            this.viewSelector.Location = new System.Drawing.Point(226, 143);
            this.viewSelector.Name = "viewSelector";
            this.viewSelector.Size = new System.Drawing.Size(121, 21);
            this.viewSelector.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(185, 147);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(30, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "View";
            // 
            // textDataBox
            // 
            this.textDataBox.Location = new System.Drawing.Point(13, 169);
            this.textDataBox.Multiline = true;
            this.textDataBox.Name = "textDataBox";
            this.textDataBox.ReadOnly = true;
            this.textDataBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textDataBox.Size = new System.Drawing.Size(582, 211);
            this.textDataBox.TabIndex = 14;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.attachmentList);
            this.groupBox1.Location = new System.Drawing.Point(601, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(194, 125);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Attachments";
            // 
            // attachmentList
            // 
            this.attachmentList.FormattingEnabled = true;
            this.attachmentList.Location = new System.Drawing.Point(7, 20);
            this.attachmentList.Name = "attachmentList";
            this.attachmentList.Size = new System.Drawing.Size(168, 95);
            this.attachmentList.TabIndex = 0;
            this.attachmentList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.attachmentList_MouseDoubleClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.recentBox);
            this.groupBox2.Controls.Add(this.answeredBox);
            this.groupBox2.Controls.Add(this.flaggedBox);
            this.groupBox2.Controls.Add(this.deletedBox);
            this.groupBox2.Controls.Add(this.draftBox);
            this.groupBox2.Controls.Add(this.seenBox);
            this.groupBox2.Location = new System.Drawing.Point(601, 143);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(194, 100);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Flags";
            // 
            // seenBox
            // 
            this.seenBox.AutoSize = true;
            this.seenBox.Location = new System.Drawing.Point(7, 26);
            this.seenBox.Name = "seenBox";
            this.seenBox.Size = new System.Drawing.Size(51, 17);
            this.seenBox.TabIndex = 0;
            this.seenBox.Text = "Seen";
            this.seenBox.UseVisualStyleBackColor = true;
            // 
            // draftBox
            // 
            this.draftBox.AutoSize = true;
            this.draftBox.Location = new System.Drawing.Point(7, 50);
            this.draftBox.Name = "draftBox";
            this.draftBox.Size = new System.Drawing.Size(49, 17);
            this.draftBox.TabIndex = 1;
            this.draftBox.Text = "Draft";
            this.draftBox.UseVisualStyleBackColor = true;
            // 
            // deletedBox
            // 
            this.deletedBox.AutoSize = true;
            this.deletedBox.Location = new System.Drawing.Point(7, 74);
            this.deletedBox.Name = "deletedBox";
            this.deletedBox.Size = new System.Drawing.Size(63, 17);
            this.deletedBox.TabIndex = 2;
            this.deletedBox.Text = "Deleted";
            this.deletedBox.UseVisualStyleBackColor = true;
            // 
            // flaggedBox
            // 
            this.flaggedBox.AutoSize = true;
            this.flaggedBox.Location = new System.Drawing.Point(102, 27);
            this.flaggedBox.Name = "flaggedBox";
            this.flaggedBox.Size = new System.Drawing.Size(64, 17);
            this.flaggedBox.TabIndex = 3;
            this.flaggedBox.Text = "Flagged";
            this.flaggedBox.UseVisualStyleBackColor = true;
            // 
            // answeredBox
            // 
            this.answeredBox.AutoSize = true;
            this.answeredBox.Location = new System.Drawing.Point(102, 50);
            this.answeredBox.Name = "answeredBox";
            this.answeredBox.Size = new System.Drawing.Size(73, 17);
            this.answeredBox.TabIndex = 4;
            this.answeredBox.Text = "Answered";
            this.answeredBox.UseVisualStyleBackColor = true;
            // 
            // recentBox
            // 
            this.recentBox.AutoSize = true;
            this.recentBox.Location = new System.Drawing.Point(102, 73);
            this.recentBox.Name = "recentBox";
            this.recentBox.Size = new System.Drawing.Size(61, 17);
            this.recentBox.TabIndex = 5;
            this.recentBox.Text = "Recent";
            this.recentBox.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(719, 356);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 17;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MessageViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 392);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textDataBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.viewSelector);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.dateBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.subjectBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.bccList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ccList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.fromList);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.toList);
            this.Name = "MessageViewer";
            this.Text = "MessageViewer";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox toList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox fromList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ccList;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox bccList;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox subjectBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox dateBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox viewSelector;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textDataBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox attachmentList;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox recentBox;
        private System.Windows.Forms.CheckBox answeredBox;
        private System.Windows.Forms.CheckBox flaggedBox;
        private System.Windows.Forms.CheckBox deletedBox;
        private System.Windows.Forms.CheckBox draftBox;
        private System.Windows.Forms.CheckBox seenBox;
        private System.Windows.Forms.Button button1;
    }
}