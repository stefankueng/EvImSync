using System;
using System.Collections.Generic;
using System.Text;

namespace IMAPShell.Attributes
{
    public class CommandInfoAttribute : Attribute
    {
        private readonly string _cmdName;
        private readonly string _cmdHelp;
        private readonly string _cmdUsage;


        public string CommandName
        {
            get { return _cmdName; }
        }

        public string CommandHelp
        {
            get { return _cmdHelp; }
        }

        public string CommandUsage
        {
            get { return _cmdUsage; }
        }

        public CommandInfoAttribute(string cmdName, string cmdHelp, string cmdUsage)
        {
            _cmdName = cmdName;
            _cmdHelp = cmdHelp;
            _cmdUsage = cmdUsage;
        }
    }
}
