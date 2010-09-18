using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Processors
{
    public class SearchProcessor : BaseProcessor
    {
        private List<IMessage> _msgs = new List<IMessage>();
        private IFolder _sourceFolder;

        public IMessage[] Messages { get { return _msgs.ToArray(); } }
        
        public override void ProcessResult()
        {
            base.ProcessResult();

            _sourceFolder = _request.PreCommand.ParameterObjects[0] as IFolder;

            foreach (string line in CmdResult.Results)
            {
                if (line.StartsWith("*"))
                    GetUIDs(line.Replace("* SEARCH","").Trim());
            }
        }

        private void GetUIDs(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            string[] ids = input.Split(' ');
            foreach (string id in ids)
            {
                int i = Convert.ToInt32(id);
                IMessage msg = _request.Client.MailboxManager.AddMessage(i, _sourceFolder.ID);
                _msgs.Add(msg);
            }
        }
    }
}
