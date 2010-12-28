using InterIMAP.Common.Commands;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Processors;

namespace InterIMAP.Common.Requests
{

    /// <summary>
    /// Request to copy a message.
    /// </summary>
    public class CopyMessageRequest : BaseRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoveMessageRequest"/> class.
        /// </summary>
        /// <param name="message">The messagy to copy.</param>
        /// <param name="destFolder">The destination folder.</param>
        /// <param name="callback">The callback to call when copy operation is completed.</param>
        public CopyMessageRequest(IMessage message, IFolder destFolder, RequestCompletedCallback callback)
            : base(callback)
        {
            PreCommand = new SelectFolderCommand(message.Folder, null);
            Command = new CopyMessageCommand(message, destFolder, null);
            ProcessorType = typeof(CopyMessageProcessor);
        }
    }
}
