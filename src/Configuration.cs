using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace EveImSync
{
    [Serializable]
    [XmlRootAttribute("Configuration", Namespace = "", IsNullable = false)]
    public class Configuration
    {
        private Configuration()
        {
        }

        static public Configuration Create()
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

        public List<SyncPairSettings> syncPairs = new List<SyncPairSettings>();

        public string ENScriptPath = string.Empty;
    }
}
