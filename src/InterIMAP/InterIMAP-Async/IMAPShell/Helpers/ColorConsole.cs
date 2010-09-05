using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace IMAPShell.Helpers
{
    public static class ColorConsole
    {
        
        private const string colorMatchPattern = "(?<color>(\\^\\d{2}:\\d{2}))";
        private static bool _resetToDefault;

        /// <summary>
        /// Indicates whether the color should be reset to default after each write command
        /// </summary>
        public static bool ResetAfterWrite
        {
            get { return _resetToDefault; }
            set { _resetToDefault = value; }
        }

        static ColorConsole()
        {
            
            _resetToDefault = true;
        }

        private static void SetCurrentColor(string input)
        {
            string temp = input.Replace("^", "");
            string[] parts = temp.Split(new char[] { ':' });

            int fg = Convert.ToInt32(parts[0]);
            int bg = Convert.ToInt32(parts[1]);

            Console.ForegroundColor = GetConsoleColor(fg);
            Console.BackgroundColor = GetConsoleColor(bg);
            
        }
        
        /// <summary>
        /// Calculates the length of the string without the color code markup
        /// </summary>
        /// <param name="s"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static int Calculate(string s, params object[] objs)
        {
            if (s == null) return 0;
            int stringLen = 0;
            string input = s;
            if (objs.Length > 0)
                input = String.Format(s, objs);

            MatchCollection matches = Regex.Matches(input, colorMatchPattern);
            if (matches.Count == 0)
            {                
                return input.Length;
            }

            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                if (i == 0)
                {
                    if (m.Index > 0)
                    {
                        string substr = input.Substring(0, m.Index);
                        stringLen += substr.Length;
                        
                    }
                }
                
                if (i + 1 < matches.Count)
                {
                    // there is another color to process
                    // only write the string up to the start index of the next color
                    int start = m.Index + m.Length;
                    int len = matches[i + 1].Index - start;
                    string substr = input.Substring(start, len);
                    stringLen += substr.Length;
                    
                }
                else
                {
                    // there are no more matches, write the rest of the string
                    string substr = input.Substring(m.Index + m.Length);
                    stringLen += substr.Length;
                    
                }

            }

            

            return stringLen;
        }

        public static int Write(string s, params object[] objs)
        {
            if (s == null) return 0;
            int stringLen = 0;
            string input = s;
            if (objs.Length > 0)
                input = String.Format(s, objs);
            
            MatchCollection matches = Regex.Matches(input, colorMatchPattern);
            if (matches.Count == 0)
            {
                Console.Write(input);
                return input.Length;
            }

            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                if (i == 0)
                {
                    if (m.Index > 0)
                    {
                        string substr = input.Substring(0, m.Index);
                        stringLen += substr.Length;
                        Console.Write(substr);
                    }
                }
                
                SetCurrentColor(m.Value);
                if (i+1 < matches.Count)
                {
                    // there is another color to process
                    // only write the string up to the start index of the next color
                    int start = m.Index + m.Length;
                    int len = matches[i + 1].Index - start;
                    string substr = input.Substring(start, len);
                    stringLen += substr.Length;
                    Console.Write(substr);
                }
                else
                {
                    // there are no more matches, write the rest of the string
                    string substr = input.Substring(m.Index + m.Length);
                    stringLen += substr.Length;
                    Console.Write(substr);
                }
                
            }

            if (_resetToDefault)
                SetCurrentColor("^07:00");

            return stringLen;
            
        }

        public static void WriteLine(string input, params object[] objs)
        {
            Write(input, objs);
            Console.WriteLine();
        }
                
        private static ConsoleColor GetConsoleColor(int col)
        {
            switch (col)
            {
                case 0:
                    return ConsoleColor.Black;
                    
                case 1:
                    return ConsoleColor.DarkBlue;
                    
                case 2:
                    return ConsoleColor.DarkGreen;
                    
                case 3:
                    return ConsoleColor.DarkCyan;
                    
                case 4:
                    return ConsoleColor.DarkRed;
                    
                case 5:
                    return ConsoleColor.DarkMagenta;
                    
                case 6:
                    return ConsoleColor.DarkYellow;
                    
                case 7:
                    return ConsoleColor.Gray;
                    
                case 8:
                    return ConsoleColor.DarkGray;
                    
                case 9:
                    return ConsoleColor.Blue;
                    
                case 10:
                    return ConsoleColor.Green;
                    
                case 11:
                    return ConsoleColor.Cyan;
                    
                case 12:
                    return ConsoleColor.Red;
                    
                case 13:
                    return ConsoleColor.Magenta;
                    
                case 14:
                    return ConsoleColor.Yellow;
                    
                case 15:
                    return ConsoleColor.White;

                    default:
                    return ConsoleColor.Black;
                    
            }
        }
        
    }
}
