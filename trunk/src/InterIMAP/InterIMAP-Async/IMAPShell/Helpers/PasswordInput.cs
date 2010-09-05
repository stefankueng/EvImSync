using System;
using System.Collections.Generic;
using System.Text;

namespace IMAPShell.Helpers
{
    public class PasswordInput
    {
        public static string ReadPassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return sb.ToString();
                }
                else
                {
                    Console.Write("*");
                    sb.Append(key.KeyChar);
                }
            }
        }
    }
}
