using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using IMAPShell.Helpers;

namespace IMAPShell.Shell
{
    public class CommandResult
    {
        #region Private Fields

        private bool _exit;
        private bool _success;
        private bool _invalid;
        private string _command;
        private string[] _args;
        private readonly List<string> _results;
        private string _successMessage;
        private string _errorMessage;
        private ResultType _type;
        #endregion

        #region CTOR
        public CommandResult(string cmd, string[] args)
        {
            _results = new List<string>();
            _command = cmd;
            _args = args;
            _type = ResultType.Success;
        }
        #endregion

        #region Public Properties
        public ResultType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string SuccessMessage
        {
            get { return _successMessage; }
            set { _successMessage = value; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        public string Command
        {
            get { return _command; }
        }

        public string[] Args
        {
            get { return _args; }
        }

        public bool Invalid
        {
            get { return _invalid; }
            set { _invalid = value; }
        }

        public bool Exit
        {
            get { return _exit; }
            set { _exit = value; }
        }

        public bool Success
        {
            get { return _success; }
            set { _success = value; }
        }

        public List<string> Results
        {
            get { return _results; }
        }
        #endregion

        public static CommandResult CreateError(string cmd, string[] args, string msg)
        {
            CommandResult result = new CommandResult(cmd, args);
            result.Type = ResultType.Error;
            result.ErrorMessage = msg;
            return result;
        }
    }
}
