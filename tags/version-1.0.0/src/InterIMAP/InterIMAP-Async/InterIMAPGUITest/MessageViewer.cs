using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Interfaces;

namespace InterIMAPGUITest
{
    public partial class MessageViewer : Form
    {
        private IMessage _msg;
        private IMAPAsyncClient _client;
        private Dictionary<string, IMessageContent> fileContent = new Dictionary<string, IMessageContent>();
        
        public MessageViewer(IMessage msg, IMAPAsyncClient client)
        {
            InitializeComponent();
            _client = client;
            _msg = msg;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            setToField();
            setFromField();
            setCCField();
            setBCCField();
            setSubject();
            setDate();
            setText();
            setFlags();
            populateAttachments();
        }

        private void setFlags()
        {
            this.seenBox.Checked = _msg.Seen;
            this.draftBox.Checked = _msg.Draft;
            this.answeredBox.Checked = _msg.Answered;
            this.flaggedBox.Checked = _msg.Flagged;
            this.recentBox.Checked = _msg.Recent;
            this.deletedBox.Checked = _msg.Deleted;
        }

        private void populateAttachments()
        {
            attachmentList.Items.Clear();
            
            foreach (IMessageContent content in _msg.MessageContent)
            {
                if (content.IsAttachment)
                {
                    attachmentList.Items.Add(content.ContentFilename);
                    fileContent.Add(content.ContentFilename, content);
                }
            }
        }

        private void setText()
        {
            this.textDataBox.Text = _msg.TextData ?? _msg.HTMLData;
        }

        private void setDate()
        {
            this.dateBox.Text = _msg.DateReceived.ToString();
        }

        private void setSubject()
        {
            this.subjectBox.Text = _msg.Subject;
        }

        private void setBCCField()
        {
            List<string> names = new List<string>();
            foreach (IContact contact in _msg.BccContacts)
                names.Add(contact.ToString());

            this.bccList.Text = string.Join(", ", names.ToArray());
        }

        private void setCCField()
        {
            List<string> names = new List<string>();
            foreach (IContact contact in _msg.CcContacts)
                names.Add(contact.ToString());

            this.ccList.Text = string.Join(", ", names.ToArray());
        }

        private void setToField()
        {
            List<string> names = new List<string>();
            foreach (IContact contact in _msg.ToContacts)
                names.Add(contact.ToString());

            this.toList.Text = string.Join(", ", names.ToArray());

        }

        private void setFromField()
        {
            List<string> names = new List<string>();
            foreach (IContact contact in _msg.FromContacts)
                names.Add(contact.ToString());

            this.fromList.Text = string.Join(", ", names.ToArray());
        }

        private void attachmentList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int idx = attachmentList.IndexFromPoint(e.Location);
            string name = attachmentList.GetItemText(attachmentList.Items[idx]);
            IMessageContent content = fileContent[name];
            _client.MailboxManager.SaveAttachment(content,@"C:\");
            MessageBox.Show(String.Format("{0} Saved.", name));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
