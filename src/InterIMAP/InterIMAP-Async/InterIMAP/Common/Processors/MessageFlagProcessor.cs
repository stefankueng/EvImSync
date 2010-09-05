using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using InterIMAP.Asynchronous.Objects;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Processors
{
    public class MessageFlagProcessor : BaseProcessor
    {
        private IMessage _msg;
        public override void ProcessResult()
        {
            base.ProcessResult();
            _msg = (IMessage) Request.Command.ParameterObjects[0];
            Message msg = (Message) _msg;

            ProcessFlags();
        }
        
        private void ProcessFlags()
        {
            // \Recent
            // \Seen
            // \Deleted
            // \Flagged
            // \Answered
            // \Draft
            //* 726 FETCH (UID 6666 FLAGS ())
            string flags = null;
            
            foreach (string firstLine in CmdResult.Results)
            {
                Match match = Regex.Match(firstLine, "^\\*\\s[\\d]*\\s\\w+\\s\\(\\w+\\s\\d+\\s[Ff][Ll][Aa][Gg][Ss]\\s\\((?<flags>(.+))\\).*$", RegexOptions.ExplicitCapture);
                if (match.Success)
                {
                    flags = match.Groups["flags"].Value;
                    break;
                }

                match = Regex.Match(firstLine,
                                    @"^\*\s+\d+\s+[Ff][Ee][Tt][Cc][Hh]\s+\([Uu][Ii][Dd]\s+\d+\s+[Ff][Ll][Aa][Gg][Ss]\s+\((?<flags>(.*\b))\)\)");
                if (match.Success)
                {
                    flags = match.Groups["flags"].Value;
                    break;
                }
            }

            if (flags == null) return;

            if (flags.Contains(MessageFlag.Seen.ToString()))
            {
                _client.MailboxManager.SetMessageFlag(_msg, MessageFlag.Seen, true, true);
            }
            else
            {
                _client.MailboxManager.SetMessageFlag(_msg, MessageFlag.Seen, false, true);
            }
            if (flags.Equals(")")) return;
            string[] separateFlag = flags.Split(new char[] {' '});
            foreach (string flag in separateFlag)
            {
                object tempFlag = Enum.Parse(typeof (MessageFlag), flag.TrimStart('\\').TrimEnd(')'));
                if (tempFlag == null) continue;
                MessageFlag f = (MessageFlag) tempFlag;
                _client.MailboxManager.SetMessageFlag(_msg, f, true, true);
            }
        }
    }
}
