using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveImSync
{
    class Attachment
    {
        public string Base64Data { get; set; }
        public string ContentID { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string Hash { get; set; }
    }
}
