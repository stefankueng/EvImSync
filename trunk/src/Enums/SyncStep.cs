using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveImSync.Enums
{
    public enum SyncStep
    {
        ExtractNotes,
        ParseNotes,
        GettingImapList,
        CalculateWhatToDo,
        AdjustTags,
        DownloadNotes,
        UploadNotes
    }
}
