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
            using (FileStream fs = File.OpenRead(@"D:\Development\EvImSync\config.xml"))
            {
                c = (Configuration)xs.Deserialize(fs);
            }

            return c;
        }

        public void Save()
        {
            XmlSerializer xs = new XmlSerializer(typeof(Configuration));
            using (FileStream fs = File.Create(@"D:\Development\EvImSync\config.xml"))
            {
                xs.Serialize(fs, this);
            }
        }

        public List<SyncPairSettings> SyncPairs = new List<SyncPairSettings>();

        public string ENScriptPath = string.Empty;
    }
}
