using System;
using System.Linq;

namespace HtmlAgilityPack
{
    public static class SilverlightExtensions
    {
        public static string[] Split(this string @this, char[] chars, int count)
        {
            var items = @this.Split(chars);
            return items.Length > 2 ? items.Take(2).ToArray() : items;
        }

        public static string[] Split(this string @this, string[] chars, int count)
        {
            var items = @this.Split(chars, StringSplitOptions.None);
            return items.Length > 2 ? items.Take(2).ToArray() : items;
        }

        
    }
}
