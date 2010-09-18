using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using IMAPShell.Attributes;
using IMAPShell.Commands;
using IMAPShell.Helpers;
using InterIMAP;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Interfaces;
using ICommand=IMAPShell.Interfaces.ICommand;

namespace IMAPShell.Shell
{
    public class IMAPShell
    {
        #region Private Fields

        private IMAPConfig _config;
        private readonly IMAPAsyncClient _client;
        private readonly Dictionary<string, Type> _commandMap = new Dictionary<string, Type>();
        private readonly Dictionary<string, string> _commandHelpMap = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _commandUsageMap = new Dictionary<string, string>();
        private readonly List<Type> _requiresConnection = new List<Type>();
        private readonly List<string> _cmdHistory = new List<string>();
        private IFolder _currentFolder;
        private readonly bool _autoConnect;
        
        LineEditor le = new LineEditor(null);
        #endregion

        #region CTOR
        public IMAPShell(IMAPConfig config, bool autoConnect)
        {
            _config = config;
            _client = new IMAPAsyncClient(config, 5);
            _currentFolder = null;
            _autoConnect = autoConnect;
            
            InitCommandMap();
        }

        private void InitCommandMap()
        {
            _commandMap.Clear();
            _commandHelpMap.Clear();
            _commandUsageMap.Clear();
            _requiresConnection.Clear();
                        
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                object[] customAttribs = t.GetCustomAttributes(typeof (CommandInfoAttribute), true);

                if (customAttribs.Length <= 0) continue;
                                
                foreach (object obj in customAttribs)
                {
                    string name = ((CommandInfoAttribute) obj).CommandName;
                    string help = ((CommandInfoAttribute) obj).CommandHelp;
                    string usage = ((CommandInfoAttribute) obj).CommandUsage;
                    _commandMap.Add(name, t);
                    _commandHelpMap.Add(name, help);
                    _commandUsageMap.Add(name, usage);
                }

                object[] requireAttribs = t.GetCustomAttributes(typeof (RequiresConnectionAttribute), true);

                if (requireAttribs.Length == 0) continue;
                _requiresConnection.Add(t);
                
                
            }
        }

        public IMAPConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        public IMAPAsyncClient Client
        {
            get { return _client; }
        }

        public List<String> AvailableCommands
        {
            get
            {
                List<string> names = new List<string>();
                foreach (string name in _commandMap.Keys)
                    names.Add(name);

                return names;
            }
        }

        public IFolder CurrentFolder
        {
            get { return _currentFolder; }
            set { _currentFolder = value; }
        }
        #endregion

        #region Public Methods
                       
        public void Start()
        {
            PrintConfig();

            if (_autoConnect)
                ProcessResult(ProcessCommand("connect"));

            bool quittinTime = false;
            while (true)
            {
                if (quittinTime) break;
                                
                CommandResult result = ProcessCommand(ProcessInput());
                quittinTime = ProcessResult(result);                                                
            }

            if (_client.IsAlive)
                _client.Stop();
        }

        
        internal IFolder FindFolder(string name)
        {
            if (CurrentFolder != null && CurrentFolder.SubFolders.Length >0)
            {
                foreach (IFolder f in CurrentFolder.SubFolders)
                    if (f.Name.Equals(name))
                        return f;
            }

            foreach (IFolder f in Client.MailboxManager.GetAllFolders())
                if (f.FullPath.Equals(name))
                    return f;

            return null;

        }
        #endregion

        #region Private Methods
        private string ProcessInput()
        {
            return le.Edit(GetPromptString(), "");
        } 

        private bool ProcessResult(CommandResult result)
        {

            switch (result.Type)
            {
                case ResultType.Exit:
                    return true;
                case ResultType.Invalid:
                    ColorConsole.WriteLine("^14:00Command '{0}' not recognized. Type 'help' to see list of valid commands.", result.Command);
                    return false;
                case ResultType.Error:
                    ColorConsole.WriteLine("\n^04:00A problem was encountered when trying to execute '{0}'", result.Command);
                    ColorConsole.Write("^08:00{0}", new string('-', Console.BufferWidth));
                    ColorConsole.WriteLine("^12:00{0}", result.ErrorMessage);
                    ColorConsole.WriteLine("^08:00{0}", new string('-', Console.BufferWidth));
                    ProcessCommand("printresult errors");
                    ColorConsole.WriteLine("^08:00{0}", new string('-', Console.BufferWidth));
                    return false;
                case ResultType.Success:
                    if (!String.IsNullOrEmpty(result.SuccessMessage))
                    {
                        ColorConsole.Write("\n^03:00{0}", new string('-', Console.BufferWidth));
                        ColorConsole.WriteLine("^11:00{0}", result.SuccessMessage);
                        ColorConsole.Write("^03:00{0}\n", new string('-', Console.BufferWidth));
                    }
                    return false;
            }
            return false;
        }

        private CommandResult ProcessCommand(string command)
        {
            CommandResult result = new CommandResult(command, null);

            if (command.Equals(""))
            {
                result.Type = ResultType.Success;
                return result;
            }

            _cmdHistory.Add(command);

            string actualCommand = command;
            string[] args = null;
            if (command.Contains(" "))
            {
                string[] temp = command.Split(' ');
                actualCommand = temp[0];                
                List<string> tempArgs = new List<string>(temp);
                tempArgs.RemoveAt(0);
                args = tempArgs.ToArray();
            }

            if (_commandMap.ContainsKey(actualCommand))
            {
                Type cmdType = _commandMap[actualCommand];
                if (_requiresConnection.Contains(cmdType) && !Client.ReadyToGo)
                {
                    result.Type = ResultType.Error;
                    result.ErrorMessage =
                        "This command requires an active connection. Please connect to the server and then try this command again.";
                    return result;
                }
                ICommand cmd = (ICommand)Activator.CreateInstance(cmdType, this, args);
                result = cmd.Execute();
            }
            else
            {
                result.Type = ResultType.Invalid;
            }

            return result;
        }

        public void PrintConfig()
        {
            PrintFullConfig(false);
        }

        public void PrintFullConfig(bool showPassword)
        {
            Console.WriteLine("\nConfiguration Parameters:\n");
            ColorConsole.WriteLine("  Server: ^15:00{0}", _config.Host);
            ColorConsole.WriteLine("Username: ^15:00{0}", _config.UserName);
            if (showPassword)
                ColorConsole.WriteLine("Password: ^15:00{0}", _config.Password);
            ColorConsole.WriteLine("     SSL: ^15:00{0}", _config.UseSSL ? "Yes": "No");
            
            Console.WriteLine();
        }

        private int PrintPrompt()
        {
            return ColorConsole.Write("^08:00(^07:00{0}^08:00) [^07:00{1}^08:00] ^15:00>> ^07:00", _config.UserName, _currentFolder == null ? "/" : _currentFolder.Name);
        }

        private string GetPromptString()
        {
            return String.Format("^08:00(^07:00{0}^08:00) [^07:00{1}^08:00] ^15:00>> ^07:00", _config.UserName,
                                 _currentFolder == null ? "/" : _currentFolder.Name);
        }

        public void PrintHelp(string command)
        {
            if (String.IsNullOrEmpty(command))
            {
                Console.WriteLine("\nAvailable Commands:\n");
                List<String> commandNames = new List<string>();
                foreach (string key in _commandMap.Keys)
                {
                    commandNames.Add(key);
                }

                commandNames.Sort();

                foreach (string name in commandNames)
                {
                    ColorConsole.WriteLine("   ^15:00{0} ^07:00{1}", name.PadRight(16), _commandHelpMap[name]);
                }

                Console.WriteLine("\nType 'help <command name>' for usage information");
            }
            else
            {
                Console.WriteLine("\nUsage of command {0}:\n", command);
                ColorConsole.WriteLine("    ^15:00{0}", _commandUsageMap[command]);
            }

            Console.WriteLine();
        }

        public void ClearScreen()
        {
            Console.Clear();
        }
        #endregion
    }
}
