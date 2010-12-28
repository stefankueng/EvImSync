using System;
using System.Collections.Generic;
using System.Text;

namespace InterIMAP.Common.Processors
{
    public class AppendProcessor : BaseProcessor
    {
        public override void ProcessResult()
        {
            base.ProcessResult();

            if (CmdResult.Response == IMAPResponse.IMAP_SUCCESS_RESPONSE)
            {
            }
        }
    }
}
