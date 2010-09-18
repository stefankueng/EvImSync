
namespace InterIMAP.Common.Processors
{

    /// <summary>
    /// Processor for copy message response.
    /// </summary>
    public class CopyMessageProcessor : BaseProcessor
    {
        /// <summary>
        /// Result of the copy operation.
        /// </summary>
        public bool MessageCopied { get; private set; }

        /// <summary>
        /// Processes the response.
        /// </summary>
        public override void ProcessResult()
        {
            base.ProcessResult();

            if (CmdResult.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE)
            {
                this.MessageCopied = true;
            }
        }
    }
}
