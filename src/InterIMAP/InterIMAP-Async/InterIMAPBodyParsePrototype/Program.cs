using System;
using System.Collections.Generic;
using System.Text;

namespace InterIMAPBodyParsePrototype
{
    class Program
    {

        private static void ParseBodyStructure(string input)
        {
            
        }

        

        
        
        static void Main(string[] args)
        {
            int somenum = 23423532;
            string s = somenum.ToString("0,000");

            Console.WriteLine(s);
            Console.ReadLine();
            return;

            //List<string> testinput = new List<string>();
            //testinput.Add("* 1 FETCH (BODYSTRUCTURE ((\"TEXT\" \"PLAIN\" (\"charset\" \"iso-8859-15\") NIL NIL \"QUOTED-PRINTABLE\" 527 9 NIL NIL NIL)(\"TEXT\" \"HTML\" (\"charset\" \"iso-8859-15\") NIL NIL \"QUOTED-PRINTABLE\" 3081 52 NIL NIL NIL) \"alternative\" (\"boundary\" \"----_=_NextPart_001_01C9984F.AB447B00\") NIL NIL) UID 15065)");
            //testinput.Add("* 4 FETCH (BODYSTRUCTURE (((\"TEXT\" \"PLAIN\" (\"charset\" \"Windows-1251\") NIL NIL \"QUOTED-PRINTABLE\" 10196 170 NIL NIL NIL)(\"TEXT\" \"HTML\" (\"charset\" \"Windows-1251\") NIL NIL \"QUOTED-PRINTABLE\" 23316 389 NIL NIL NIL) \"alternative\" (\"boundary\" \"----_=_NextPart_002_01C99876.CB3B6500\") NIL NIL)(\"IMAGE\" \"GIF\" (\"name\" \"apologized.gif\") \"apologized.gif\" \"apologized.gif\" \"BASE64\" 4226 NIL (\"attachment\" (\"filename\" \"apologized.gif\")) NIL)(\"IMAGE\" \"GIF\" (\"name\" \"Fichte.gif\") \"Fichte.gif\" \"Fichte.gif\" \"BASE64\" 2168 NIL (\"attachment\" (\"filename\" \"Fichte.gif\")) NIL) \"related\" (\"boundary\" \"----_=_NextPart_001_01C99876.CB3B6500\" \"type\" \"multipart/alternative\") NIL NIL) UID 7616)");
            ////testinput.Add("");
            ////testinput.Add("");
            ////testinput.Add("");
            ////testinput.Add("");
            ////testinput.Add("");
            ////testinput.Add("");

            //foreach (string s in testinput)
            //{
            //    string s2 = s.Substring(s.IndexOf(" (", s.IndexOf("BODYSTRUCTURE")));
            //    ParseBodyStructure(s2.Trim().Substring(0, s2.Trim().Length - 1));
            //}
        }
    }
}
