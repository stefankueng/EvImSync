using System;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Requests
{

    /// <summary>
    /// Request to move a message to another folder.
    /// </summary>
    public class MoveMessageRequest
    {
        //--------------------------------------------------
        #region Constructor/Fields

        private readonly IMAPAsyncClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveMessageRequest"/> class.
        /// </summary>
        /// <param name="client">The imap client.</param>
        public MoveMessageRequest(IMAPAsyncClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            this._client = client;
        }

        public delegate void MoveMessageRequestCompletedHandler();
        /// <summary>
        /// Raised when the message is moved.
        /// </summary>
        public event MoveMessageRequestCompletedHandler MoveMessageCompleted;

        #endregion
        //--------------------------------------------------

        /// <summary>
        /// Starts moving the message to the destination folder.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="destFolder">The destination folder.</param>
        public void Start(IMessage message, IFolder destFolder)
        {
            // Copy the message to the dest folder and delete original message
            this._client.RequestManager.SubmitRequest(new CopyMessageRequest(message, destFolder, delegate { this.DeleteMessage(message); }), false);
        }

        /// <summary>
        /// Deletes the original message.
        /// </summary>
        /// <param name="message">The original message.</param>
        private void DeleteMessage(IMessage message)
        {
            this._client.RequestManager.SubmitRequest(new DeleteMessageRequest(message, delegate
            {
                // Rais event when delete completed
                if (this.MoveMessageCompleted != null)
                {
                    this.MoveMessageCompleted();
                }
            }), false);
        }
    }
}
