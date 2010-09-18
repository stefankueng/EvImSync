using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using InterIMAP;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;
using System.Collections;
using InterIMAP.Common.Requests;

namespace InterIMAPGUITest
{
    public partial class Form1 : Form
    {
        private readonly ArrayList _treeResults;
        private IFolder folder;
        private ProgressWindow progWin = new ProgressWindow();
        private bool progWindowVisible = false;
        private IMAPAsyncClient client;
        
        public Form1()
        {
            InitializeComponent();
            _treeResults = new ArrayList();
            textBox1.DataBindings.Add("Text", _treeResults, null);
            progWin.Shown += new EventHandler(progWin_Shown);
            progWin.ShowInTaskbar = false;
            button2.Enabled = false;
            progressBar1.Visible = false;
        }

        void progWin_Shown(object sender, EventArgs e)
        {
            progWindowVisible = true;
        }

        private void UpdateTextBox(ArrayList text)
        {
            if (textBox1.InvokeRequired)
                textBox1.Invoke(new UpdateTextCallback(UpdateTextBox), new object[] {text});
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (string s in text)
                {
                    sb.AppendLine(s);
                }
                textBox1.Text = sb.ToString();
            }
            
            
        }

        private void UpdateFolderList(string folderName)
        {
            folderListBox.Items.Add(folderName);
        }

        private void UpdateMessageList(int[] uids)
        {
            if (messageListBox.InvokeRequired)
                messageListBox.Invoke(new UpdateMessageListCallback(UpdateMessageList), new object[] {uids});
            else
            {
                messageListBox.Items.Clear();                
                
                List<int> temp = new List<int>(uids);
                temp.Reverse();
                foreach (int uid in temp)
                    messageListBox.Items.Add(uid);
            }
        }

        private void UpdateLabel(string text)
        {
            if (label1.InvokeRequired)
                label1.Invoke(new UpdateLabelCallback(UpdateLabel), new object[] {text});
            else
                label1.Text = text;
        }

        public delegate void UpdateTextCallback(ArrayList text);

        public delegate void UpdateFolderListCallback(string folderName);

        public delegate void UpdateLabelCallback(string text);

        public delegate void UpdateMessageListCallback(int[] uids);

        public delegate void UpdateProgressWindowCallback(double percent, bool visible);

        public delegate void UpdateFolderListStateCallback(bool enabled);

        public delegate void UpdateProgressBarCallback(long current, long max);

        public void UpdateFolderListState(bool enabled)
        {
            if (folderListBox.InvokeRequired)
                folderListBox.Invoke(new UpdateFolderListStateCallback(UpdateFolderListState), new object[] {enabled});
            else
            {
                folderListBox.Enabled = enabled;
            }
        }


        private void FolderTreeCallback(IRequest req)
        {
            Console.WriteLine();
            FolderListProcessor flp = req.GetProcessorAsType<FolderListProcessor>();
            
            
            foreach (IFolder f in client.MailboxManager.Folders)
            {                
                folderListBox.Invoke(new UpdateFolderListCallback(UpdateFolderList), new object[] {f.FullPath});
            }

            //label1.Invoke(new UpdateLabelCallback(UpdateLabel), new object[] {""});
            UpdateFolderListState(false);
            UpdateLabel("Populating folder data...");
            client.MailboxManager.PopulateFolderData(FolderDataProgress, FolderDataCompleted);
        }

        private void UpdateProgressBar(long current, long max)
        {
            if (progressBar1.InvokeRequired)
                progressBar1.Invoke(new UpdateProgressBarCallback(UpdateProgressBar), new object[] {current, max});
            else
            {
                if (current == -1 && max == -1)
                {
                    progressBar1.Visible = false;
                    return;
                }
                
                if (current == 0 || max == 0) return;
                progressBar1.Visible = true;
                double percent = ((double)current / (double)max) * 100.00;
                if ((int)percent <= 100)
                    progressBar1.Value = (int)percent;
            }
        }
        
        private void UpdateProgressWindow(double percent, bool visible)
        {
            if (progWin.InvokeRequired)
                progWin.Invoke(new UpdateProgressWindowCallback(UpdateProgressWindow), new object[] {percent});
            else
            {
                if (!progWindowVisible)
                    progWin.ShowDialog();

                progWin.Percent = percent;

                if (!visible)
                    progWin.Hide();
            }
        }

        private void FolderDataProgress(double percent)
        {
            //UpdateProgressWindow(percent, true);
                
        }

        private void FolderDataCompleted(IBatchRequest req)
        {
            //UpdateProgressWindow(0, false);
            UpdateFolderListState(true);
            UpdateLabel("Folder data loaded.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Getting folder list...";
            IMAPConfig config = new IMAPConfig(@"c:\test1.cfg");
            client = new IMAPAsyncClient(config, 5);
            //IMAPConnectionPool.GetInstance().StartUp(config, 5);
            client.Start();
            client.RequestManager.SubmitRequest(new FolderTreeRequest("\"\"", FolderTreeCallback), false);
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateLabel("Shutting down...");
            folderListBox.Items.Clear();
            messageListBox.Items.Clear();
            //IMAPConnectionPool.GetInstance().Shutdown();
            client.ConnectionPool.Shutdown();
            UpdateLabel("Disconnected");
            button2.Enabled = false;
            button1.Enabled = true;
            progressBar1.Visible = false;
        }

        private void folderListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string name = folderListBox.SelectedItem.ToString();
            UpdateLabel(String.Format("Getting message list for '{0}'...", name));
            folder = client.MailboxManager.GetFolderByPath(name);
            client.RequestManager.SubmitRequest(new MessageListRequest(folder, MessageListComplete), false);
            //UpdateMessageList(folder.Messages);
        }

        private void MessageListComplete(IRequest req)
        {
            MessageListProcessor mlp = req.GetProcessorAsType<MessageListProcessor>();
            //mlp.Request.Command.
            //textBox1.Invoke(new UpdateTextCallback(UpdateTextBox), new object[] {mlp.CmdResult.Results});
            UpdateTextBox(mlp.CmdResult.Results);
            UpdateMessageList(mlp.UIDs);
            UpdateLabel("");
        }

        private void messageListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int uid = Int32.Parse(messageListBox.SelectedItem.ToString());
            UpdateLabel("Getting header for message id "+uid);
            IMessage msg = client.MailboxManager.GetMessageByUID(uid, folder.ID);

            FullMessageRequest fmr = new FullMessageRequest(client, msg);
            fmr.MessageComplete += new FullMessageCompleteCallback(fmr_MessageComplete);
            fmr.MessageProgress += new FullMessageProgressCallback(fmr_MessageProgress);
            fmr.Start();
            //IMAPRequestManager.SubmitRequest(new MessageHeaderRequest(msg, MessageHeaderComplete), false);
            //IMAPRequestManager.SubmitRequest(new MessageStructureRequest(msg, MessageStructureComplete), false);
            //client.RequestManager.SubmitBatchRequest(
            //    new SimpleBatchRequest(new IRequest[]
            //        {
            //            new MessageHeaderRequest(msg, MessageHeaderComplete),
            //            new MessageStructureRequest(msg, MessageStructureComplete)
            //        }), false);


        }

        void fmr_MessageProgress(IMessage msg, long bytesReceived, long totalBytes)
        {
            UpdateProgressBar(bytesReceived, totalBytes);
        }

        void fmr_MessageComplete(IMessage msg, TimeSpan time)
        {
            MessageViewer mv = new MessageViewer(msg,client);
            string txt = String.Format("Message {0} took {1} seconds to download", msg.UID, time.TotalSeconds);
            UpdateProgressBar(-1,-1);
            UpdateLabel(txt);
            Application.Run(mv);
        }

        private void MessageStructureComplete(IRequest req)
        {
            UpdateTextBox(req.Result.Results);
            SimpleBatchRequest sbr = new SimpleBatchRequest();
            IMessage msg = req.Command.ParameterObjects[0] as IMessage;
            foreach (IMessageContent content in msg.MessageContent)
                sbr.Requests.Add(new MessagePartRequest(content, MessagePartComplete, null));

            client.RequestManager.SubmitAsyncBatchRequest(sbr, false);
        }

        private void MessagePartComplete(IRequest req)
        {
            IMessageContent content = req.Command.ParameterObjects[0] as IMessageContent;

        }
        
        private void MessageHeaderComplete(IRequest req)
        {
            UpdateTextBox(req.Result.Results);
            UpdateLabel("");
            MessageHeaderProcessor mhp = req.GetProcessorAsType<MessageHeaderProcessor>();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("To Count: {0}{1}", mhp.Message.ToContacts.Length, Environment.NewLine);
            sb.AppendFormat("From Count: {0}{1}", mhp.Message.FromContacts.Length, Environment.NewLine);

            //MessageBox.Show(sb.ToString());
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            client.MailboxManager.SaveNewMailbox(@"c:\mailbox.mbx");
        }
    }
}
