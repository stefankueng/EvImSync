using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveImSync.Enums
{
    public enum SyncStep
    {
        Start,
        ExtractNotes,
        ParseNotes,
        GettingImapList,
        CalculateWhatToDo,
        AdjustTags,
        DownloadNotes,
        UploadNotes
    }
}
