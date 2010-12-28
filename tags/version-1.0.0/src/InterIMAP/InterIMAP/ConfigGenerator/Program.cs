/********************************************************************************************
 * ConfigGenerator
 * Part of the InterIMAP Library
 * Copyright (C) 2004-2007 Rohit Joshi
 * Copyright (C) 2008 Jason Miesionczek
 * Original Author: Rohit Joshi
 * Based on this article on codeproject.com:
 * IMAP Client library using C#
 * http://www.codeproject.com/KB/IP/imaplibrary.aspx?msg=2498332
 * Posted: August 16th 2004
 * 
 * InterIMAP is free software; you can redistribute it and/or modify it under the terms
 * of the GNU Lesser General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * 
 * InterIMAP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with
 * InterIMAP. If not, see http://www.gnu.org/licenses/.
 * 
 * *****************************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using InterIMAP;


namespace ConfigGenerator
{
    /// <summary>
    /// This application is used to create a configuration file for use with the InterIMAP Library
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("USAGE: ConfigGenerator <configfilename>");
                return;
            }
            
            IMAPConfig config = new IMAPConfig();
            Console.WriteLine("ConfigGenerator for InterIMAP");
            Console.WriteLine("Copyright (C) 2008 Jason Miesionczek");
            Console.WriteLine();
            Console.Write("Enter Hostname: ");
            string host = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Use SSL [y/N]: ");
            string ssl = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Auto Logon [y/N]: ");
            string logon = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Debug Mode [y/N]: ");
            string debug = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Default Folder: ");
            string defaultFolder = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Local cache file: ");
            string cache = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Cache Format [xml/binary]: ");
            string format = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Auto Sync Cache [Y/n]: ");
            string sync = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Auto Retrieve All Message UIDs [Y/n]: ");
            string getids = Console.ReadLine();

            config.AutoLogon = logon.Equals("y") ? true : false;
            config.DebugMode = debug.Equals("y") ? true : false;
            config.DefaultFolderName = defaultFolder;
            config.Host = host;
            config.Password = password;
            config.UserName = username;
            config.UseSSL = ssl.Equals("y") ? true : false;
            config.CacheFile = cache;
            config.Format = format.Equals("xml") ? CacheFormat.XML : (format.Equals("binary") ? CacheFormat.Binary : CacheFormat.XML);
            config.AutoGetMsgID = getids.Equals("n") ? false : true;
            config.AutoSyncCache = sync.Equals("y") ? true : false;
            config.SaveConfig(args[0]);
            Console.WriteLine("{0} created successfully.", args[0]);
        }
    }
}
