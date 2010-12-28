// EvImSync - A tool to sync Evernote notes to IMAP mails and vice versa
// Copyright (C) 2010 - Stefan Kueng

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace EveImSync
{
    [Serializable]
    [XmlRootAttribute("Configuration", Namespace = "", IsNullable = false)]
    public class Configuration
    {
        private Configuration()
        {
        }

        public static Configuration Create()
        {
            Configuration c = new Configuration();
            XmlSerializer xs = new XmlSerializer(typeof(Configuration));
            bool portable = File.Exists(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "portable");
            string configPath = portable
                ? Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath)
                : Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "EvImSync");
            configPath = Path.Combine(configPath, "EvImSync.xml");
            if (File.Exists(configPath))
            {
                using (FileStream fs = File.OpenRead(configPath))
                {
                    c = (Configuration)xs.Deserialize(fs);
                }

                SimpleAES simpleAES = new SimpleAES();
                foreach (SyncPairSettings sps in c.SyncPairs)
                {
                    sps.IMAPPassword = simpleAES.DecryptString(sps.IMAPPassword);
                }
            }

            return c;
        }

        public void Save()
        {
            SimpleAES simpleAES = new SimpleAES();
            foreach (SyncPairSettings sps in SyncPairs)
            {
                sps.IMAPPassword = simpleAES.EncryptToString(sps.IMAPPassword);
            }

            bool portable = File.Exists(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "portable");
            string configPath = portable 
                ? Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) 
                : Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "EvImSync");
            configPath = Path.Combine(configPath, "EvImSync.xml");

            XmlSerializer xs = new XmlSerializer(typeof(Configuration));
            if (!Directory.Exists(Path.GetDirectoryName(configPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            }

            using (FileStream fs = File.Create(configPath))
            {
                xs.Serialize(fs, this);
            }

            foreach (SyncPairSettings sps in SyncPairs)
            {
                sps.IMAPPassword = simpleAES.DecryptString(sps.IMAPPassword);
            }
        }

        public List<SyncPairSettings> SyncPairs = new List<SyncPairSettings>();

        public string ENScriptPath = string.Empty;
    }
}
