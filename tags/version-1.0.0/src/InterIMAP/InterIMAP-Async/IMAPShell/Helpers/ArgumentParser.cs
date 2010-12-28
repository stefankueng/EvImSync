using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IMAPShell.Helpers
{
    /// <summary>
    /// The Argument Parser takes an array of strings and separates them into option/value pairs.
    /// An option can begin with '-' '--' '/'.
    /// A value can contain spaces if it is surrounded with single or double quotes
    /// </summary>
    public class ArgumentParser
    {
        private readonly Dictionary<string, string> optionValues;
        
        public string this[string param]
        {
            get
            {
                if (optionValues.ContainsKey(param))
                    return optionValues[param];
                else
                {
                    return null;
                }
            }
        }

        public int Count
        {
            get { return optionValues.Keys.Count; }
        }
        
        public ArgumentParser(string[] args)
        {            
            Dictionary<int, string> optionMap = new Dictionary<int, string>();
            optionValues = new Dictionary<string, string>();
            
            if (args.Length == 1)
            {
                string option;
                if (ThisIsAnOption(args[0], out option))
                    optionValues.Add(option, "true");

                return;
            }
            
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                string option;
                if (ThisIsAnOption(arg, out option))
                {
                    if (!optionMap.ContainsKey(i))
                        optionMap.Add(i, option);
                    continue;
                }                
            }

            CollectValueForOption(optionMap, args);
        }

        private bool ThisIsAnOption(string input, out string option)
        {
            Match match = Regex.Match(input, @"^([-]+)");
            string prefix = match.Groups[0] != null ? match.Groups[0].Value : "";
            option = input.Substring(input.IndexOf(prefix)+prefix.Length);
            return match.Success;
        }

        private void CollectValueForOption(Dictionary<int, string> oMap, string[] args)
        {            
            List<int> idxs = new List<int>();
            
            foreach (int oIdx in oMap.Keys)
            {
                idxs.Add(oIdx);
            }

            int currentOption = 0;
            for (int i = 0; i < args.Length; i++)
            {
                if (idxs.Contains(i))
                {
                    if (currentOption == i-1) // if this is an option, and the previous arg was also an option
                    {
                        optionValues.Add(oMap[currentOption],"true");
                    }
                    currentOption = i;
                    continue;
                }

                if (currentOption > -1)
                {
                    string arg = string.Concat(args[i], " ");
                    
                    if (!optionValues.ContainsKey(oMap[currentOption]))
                        optionValues.Add(oMap[currentOption],arg);
                    else
                    {
                        optionValues[oMap[currentOption]] += arg;
                    }
                }
            }

            
        }
    }
}
