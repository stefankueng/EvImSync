using System;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Interfaces;
using IMAPShell.Shell;
using InterIMAP.Asynchronous.Client;

namespace IMAPShell.Commands
{
    public abstract class BaseCommand : ICommand
    {
        private string[] _args;
        private IMAPShell.Shell.IMAPShell _shell;
        private string _command;

        protected BaseCommand(IMAPShell.Shell.IMAPShell shell, string cmd, string[] args)
        {
            _args = args;
            _shell = shell;
            _command = cmd;
        }

        protected string[] Args
        {
            get { return _args ?? new string[] {}; }
        }

        protected string Command
        {
            get { return _command; }
        }

        protected IMAPShell.Shell.IMAPShell Shell
        {
            get { return _shell; }
        }
        
        public abstract CommandResult Execute();
    }
}
