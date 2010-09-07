using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace EveImSync
{
    [Serializable]
    public class SyncPairSettings
    {
        public SyncPairSettings()
        {

        }
        public string EvernoteNotebook = string.Empty;
        public string IMAPUsername = string.Empty;
        public string IMAPPassword = string.Empty;
        public string IMAPServer = string.Empty;
        public string IMAPNotesFolder = string.Empty;
    }
}
