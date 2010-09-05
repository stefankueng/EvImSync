using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{
    /// <summary>
    /// Checks every folder for new messages
    /// </summary>
    public class NewMessagesRequest
    {
        #region Private Fields

        private readonly IMAPAsyncClient _client;
        private readonly SearchCriteria _criteria;
        #endregion

        #region CTOR
        /// <summary>
        /// Create a new NewMessageRequest
        /// </summary>
        /// <param name="client"></param>
        public NewMessagesRequest(IMAPAsyncClient client)
        {
            _client = client;
            _criteria = new SearchCriteria(true);
        }
        #endregion

        public delegate void NewMessageRequestCompletedHandler(IMessage[] messages);
        public event NewMessageRequestCompletedHandler NewMessageRequestCompleted;

        #region Public Methods

        /// <summary>
        /// Search for new messages within the specified folder
        /// </summary>
        /// <param name="folder"></param>
        public void Start(IFolder folder)
        {
            _client.RequestManager.SubmitRequest(new SearchRequest(folder, _criteria, delegate(IRequest req)
            {
                SearchProcessor sp = req.GetProcessorAsType<SearchProcessor>();
                IMessage[] msgs = sp.Messages;
                if (NewMessageRequestCompleted != null)
                    NewMessageRequestCompleted(msgs);
            }), true);
        }

        /// <summary>
        /// Search for messages within the specified folders. The NewMessageRequestCompleted event will be fired once for each folder.
        /// </summary>
        /// <param name="folders"></param>
        public void Start(IFolder[] folders)
        {
            foreach (IFolder folder in folders)
            {
                Start(folder);
            }
        }

        /// <summary>
        /// Starts the search for new messages
        /// </summary>
        public void Start()
        {
            IFolder[] folders = _client.MailboxManager.GetAllFolders();
            List<IMessage> newMessages = new List<IMessage>();
            int folderCount = 0;
            foreach (IFolder folder in folders)
            {
                _client.RequestManager.SubmitRequest(new SearchRequest(folder, _criteria, 
                    delegate(IRequest req)
                        {
                            SearchProcessor sp = req.GetProcessorAsType<SearchProcessor>();
                            newMessages.AddRange(sp.Messages);
                            folderCount++;
                        }), false);
            }

            while (folderCount < folders.Length) { Thread.Sleep(1); }
        }
        #endregion
    }
}
