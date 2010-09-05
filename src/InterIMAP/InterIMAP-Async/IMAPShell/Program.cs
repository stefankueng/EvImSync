using System;
using System.Collections.Generic;

using System.Runtime.InteropServices;
using System.Text;
using IMAPShell.Helpers;
using IMAPShell;
using InterIMAP;

namespace IMAPShell
{
    class Program
    {
        private static IMAPShell.Shell.IMAPShell _shell;
        private static Dictionary<string, string> argValues = new Dictionary<string, string>();
        private static IMAPConfig _config;

        private static void PrintWelcome()
        {
            ColorConsole.WriteLine("^11:00IMAPShell ^15:00Interactive IMAP Environment");
            ColorConsole.WriteLine("Version 0.01. Copyright (C) 2009 Jason Miesionczek");
            ColorConsole.WriteLine("\n^10:00Type 'help' for a list of available commands\n");
        }
        
        private static void PrintHelp()
        {
            Console.WriteLine("USAGE: IMAPShell -c <config file>");
            Console.WriteLine("       IMAPShell -s <server> -u <username> -p <password> [-ssl]\n\n");
            Console.WriteLine("Optional Arguments:");
            Console.WriteLine("       -auto\tAutomatically connects using specified configuration");
        }
        

        private static string NextArg(string[] args, int idx)
        {
            if (idx + 1 < args.Length)
                return args[idx + 1];
            else
            {
                return null;
            }
        }
        
        public static void Main(string[] args)
        {
            PrintWelcome();
            Arguments argParser = new Arguments(args);
            if (argParser["c"] != null)
                _config = new IMAPConfig(argParser["c"]);
            else if (argParser.ArgsDefined(new string[] { "s","p","u"}))
            {
                string server = argParser["s"];
                string username = argParser["u"];
                string password = argParser["p"];
                bool useSSL = argParser["ssl"] != null ? true : false;
                _config = new IMAPConfig(server, username, password, useSSL, false, "");
            }
            else
            {
                ColorConsole.WriteLine("\n\n^13:00Invalid parameters specified.\n");
                PrintHelp();
                return;
            }

            bool autoConnect = argParser["auto"] != null;
            
            _shell = new Shell.IMAPShell(_config, autoConnect);
            _shell.Start();                                    
        }
    }
}
