/********************************************************************************************
 * InterIMAP
 * Copyright (C) 2008-2009 Jason Miesionczek
 * Original Author: Rohit Joshi
 * Based on this article on codeproject.com:
 * IMAP Client library using C#
 * http://www.codeproject.com/KB/IP/imaplibrary.aspx?msg=2498332
 * Posted: August 16th 2004
 * 
 * ZipStorer code written by Jaime Olivares
 * http://www.codeproject.com/KB/recipes/ZipStorer.aspx
 * 
 * InterIMAP is free software; you can redistribute it and/or modify it under the terms
 * of the GNU Lesser General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * 
 * InterIMAP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with
 * InterIMAP. If not, see http://www.gnu.org/licenses/.
 * 
 * *****************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using InterIMAP.Asynchronous.Objects;
using InterIMAP.Common.Attributes;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Processors
{
    /// <summary>
    /// Processes the header of a message
    /// </summary>
    public class MessageHeaderProcessor : BaseProcessor
    {
        private IMessage _msg;
        private readonly Dictionary<string, string> headerPairs = new Dictionary<string, string>();
        
        /// <summary>
        /// The message object this processor is working on
        /// </summary>
        public IMessage Message
        {
            get { return _msg; }
        }
        
        public override void ProcessResult()
        {
            base.ProcessResult();

            _msg = (IMessage) Request.Command.ParameterObjects[0];
            Message msg = (Message) _msg;

            ProcessHeader();
            
            msg.AssociateContacts("ToContacts", ProcessContactField("to"));
            msg.AssociateContacts("FromContacts", ProcessContactField("from"));
            msg.AssociateContacts("CcContacts", ProcessContactField("cc"));
            msg.AssociateContacts("BccContacts", ProcessContactField("bcc"));
            
            PopulateMessageObject();

        }

        

        private void PopulateMessageObject()
        {
            foreach (string key in headerPairs.Keys)
            {
                PropertyInfo[] pis = FindPropertyByKey(key);
                foreach (PropertyInfo pi in pis)
                {
                    if (pi == null) continue;

                    if (pi.PropertyType == typeof (DateTime))
                    {
                        Match match = Regex.Match(headerPairs[key], "^.{4}\\s(?<date>(\\d+\\s\\w{3}\\s\\d+\\s\\d+:\\d+:\\d+)).*$",
                                                  RegexOptions.ExplicitCapture);
                        if (match.Success)
                        {
                            pi.SetValue(_msg, DateTime.Parse(match.Groups["date"].Value), null);
                        }
                        else
                        {
                            try
                            {
                                DateTime date = ParseDate(headerPairs[key]);
                                pi.SetValue(_msg, date, null);

                            }
                            catch (Exception) {}
                        }
                        
                    }
                    else
                    {
                        pi.SetValue(_msg, headerPairs[key], null);
                    }
                }
            }        
        }

        private DateTime ParseDate(string date)
        {
            //Tue,  7 Apr 2009 09:04:02 -0700 (PDT)
            string pattern = @"^\w+[,]\s+(?<day>(\d+))\s+(?<month>(\w+))\s+(?<year>(\d+))\s+(?<hour>(\d{2})):(?<min>(\d{2})):(?<sec>(\d{2}))\s+[-]\d+\s+";
            Match match = Regex.Match(date, pattern);
            if (match.Success)
            {
                int day = Int32.Parse(match.Groups["day"].Value);
                int month = GetMonth(match.Groups["month"].Value);
                if (month == 0)
                    throw new Exception("Could not identify month in date string");

                int year = Int32.Parse(match.Groups["year"].Value);
                int hour = Int32.Parse(match.Groups["hour"].Value);
                int min = Int32.Parse(match.Groups["min"].Value);
                int sec = Int32.Parse(match.Groups["sec"].Value);
                DateTime d = new DateTime(year, month, day, hour, min, sec);
                return d;

            }

            DateTime d2 = new DateTime();
            DateTime.TryParse(date, out d2);

            return d2;
        }

        private int GetMonth(string month)
        {
            string m = month.ToLower();
            switch (m)
            {
                case "jan":
                    return 1;
                case "feb":
                    return 2;
                case "mar":
                    return 3;
                case "apr":
                    return 4;
                case "may":
                    return 5;
                case "jun":
                    return 6;
                case "jul":
                    return 7;
                case "aug":
                    return 8;
                case "sep":
                    return 9;
                case "oct":
                    return 10;
                case "nov":
                    return 11;
                case "dec":
                    return 12;
                default:
                    return 0;
            }
        }

        private PropertyInfo[] FindPropertyByKey(string key)
        {
            PropertyInfo[] pis = typeof (Message).GetProperties();
            List<PropertyInfo> foundPI = new List<PropertyInfo>();

            foreach (PropertyInfo pi in pis)
            {
                object[] attribs = pi.GetCustomAttributes(false);
                foreach (object obj in attribs)
                {
                    if (obj is HeaderName)
                    {
                        if (((HeaderName)obj).Name.Equals(key))
                            foundPI.Add(pi);
                    }
                }
            }

            return foundPI.ToArray();

        }

        private void ProcessHeader()
        {
            for (int i = 0; i < CmdResult.Results.Count; i++)
            {
                string currentLine = CmdResult.Results[i].ToString();
                string nextLine = GetNextLine(i);
                if (currentLine.StartsWith("*") || 
                    currentLine.StartsWith(" ") || 
                    String.IsNullOrEmpty(currentLine) || 
                    currentLine.Equals(")") ||
                    Regex.IsMatch(currentLine, "^IMAP[0-9]+\\s[Oo][Kk].*$")
                    ) 
                    continue;

                if (nextLine == null) 
                    break;
                // check if current line continues on next line
                while (LineContinues(nextLine))
                {                    
                    // take the current line and its continuation on the next and combine them and put them back
                    // into the results array for further processing
                    currentLine = CombineLines(i);
                    ResetIndex(currentLine, i);
                    RemoveLine(i+1);
                    nextLine = GetNextLine(i);
                }

                string headerField = currentLine.Substring(0, currentLine.IndexOf(':')).ToLower();
                string headerValue = currentLine.Substring(currentLine.IndexOf(':') + 1).Trim();                
                StoreFieldValue(headerField, headerValue);
            }
        }

        private void StoreFieldValue(string field, string value)
        {
            if (headerPairs.ContainsKey(field))
            {
                //string temp = headerPairs[field];
                //temp += value;
                headerPairs[field] += value;
            }
            else
            {
                headerPairs.Add(field, value);

            }
        }

        private string GetNextLine(int i)
        {
            return (i + 1 < CmdResult.Results.Count) ? CmdResult.Results[i + 1].ToString() : null;
        }

        private IContact[] ProcessContactField(string field)
        {
            if (!headerPairs.ContainsKey(field))
                return null;

            string data = headerPairs[field];
            headerPairs.Remove(field);
            return GetContacts(data);
        }
        
        private IContact[] GetContacts(string str)
        {
            List<IContact> contacts = new List<IContact>();
            
            string[] addresses = ExtractAddresses(str);

            foreach (string addr in addresses)
            {
                Dictionary<string, string> parts = ParseAddress(addr.Trim(new char[] {','}));

                IContact contact = _client.MailboxManager.GetContactByEMail(parts["email"]) ??
                                   (parts["name"] != null ? _client.MailboxManager.AddContact(parts["name"], parts["email"]) : _client.MailboxManager.AddContact(parts["first"], parts["last"], parts["email"]));

                contacts.Add(contact);
            }


            return contacts.ToArray();
        }

        private Dictionary<string, string> ParseAddress(string addr)
        {
            Dictionary<string, string> addrParts = new Dictionary<string, string>();
            string email = null;
            string first = null;
            string last = null;
            string name = null;
            // "\"Don't Waste It!\" presented by WM at INNOVENTIONS at Epcot(r)" <wastemanagement@disneyworld.com>
            Match match = Regex.Match(addr, "^\\s*[\"']*(?<first>(\\w*[.-]*\\w*))\\s*,*(?<last>(\\w*[.-]*\\w*))[\"']*\\s*[<]*(?<email>([^>]*))[>]*$");
            if (match.Success)
            {
                first = match.Groups["first"].Value.Trim();
                last = match.Groups["last"].Value.Trim();
                email = match.Groups["email"].Value.Trim();
// ReSharper disable RedundantAssignment
                name = null;
// ReSharper restore RedundantAssignment
            }

            match = Regex.Match(addr, "^[\"]*(?<last>([^,]*)),\\s*(?<first>([^\"]*))\\W*\\s[<]*(?<email>([^>]*))[>]*$");
            if (match.Success)
            {
                first = match.Groups["first"].Value.Trim();
                last = match.Groups["last"].Value.Trim();
                email = match.Groups["email"].Value.Trim();
// ReSharper disable RedundantAssignment
                name = null;
// ReSharper restore RedundantAssignment
            }

            match = Regex.Match(addr, "^[<]*(?<email>(\\w+@\\w+\\.\\w+))[>]*$");
            if (match.Success)
            {
                first = null;
                last = null;
                email = match.Groups["email"].Value.Trim();
// ReSharper disable RedundantAssignment
                name = null;
// ReSharper restore RedundantAssignment
            }

            match = Regex.Match(addr, "^(?<name>(.[^<>@\\.,]*))$");
            if (match.Success)
            {
                first = match.Groups["name"].Value.Trim();
                last = null;
                email = null;
// ReSharper disable RedundantAssignment
                name = null;
// ReSharper restore RedundantAssignment
            }

            match = Regex.Match(addr, "^\\s*[\"']*(?<name>([^<\"']*))[\"\']*\\s*[<]*(?<email>([^>]*))[>]*$");
            if (match.Success)
            {
                first = null;
                last = null;
                name = match.Groups["name"].Value.Trim();
                email = match.Groups["email"].Value.Trim();
            }

            match = Regex.Match(addr, @"(?<name>(.*))\s*<(?<email>(.*))>");
            if (match.Success)
            {
                first = null;
                last = null;
                name = match.Groups["name"].Value.Trim();
                email = match.Groups["email"].Value.Trim();
            }
            
            addrParts.Add("first", first);
            addrParts.Add("last", last);
            addrParts.Add("email", email);
            addrParts.Add("name", name);

            return addrParts;
        }

        private string[] ExtractAddresses(string str)
        {
            List<String> addresses = new List<string>();

            int atIdx = 0;
            int commaIdx = 0;
            int lastComma = 0;
            for (int c = 0; c < str.Length; c++)
            {
                if (str[c] == '@')
                    atIdx = c;

                if (str[c] == ',')
                    commaIdx = c;

                if (commaIdx > atIdx && atIdx > 0)
                {
                    // we are at a comma after an '@'
                    string temp = str.Substring(lastComma, commaIdx-lastComma);
                    addresses.Add(temp.Trim(new char[]{' ',','}));
                    lastComma = commaIdx;
                    atIdx = commaIdx;
                }

                if (c == str.Length-1 && lastComma > 0 && c != lastComma)
                {
                    string temp = str.Substring(lastComma, str.Length - lastComma);
                    addresses.Add(temp);
                }
            }

            if (commaIdx < atIdx && lastComma == 0)
                addresses.Add(str);

            if (commaIdx < 2)
            {
                // if we get here we can assume either, there was no comma, which means there is only only address,
                // or there is only one comma that is used to separate last name, first name
                addresses.Add(str);
            }

            return addresses.ToArray();
        }


        private void ResetIndex(string input, int idx)
        {
            CmdResult.Results[idx] = input;
        }

        private string CombineLines(int lineIdx)
        {
            return String.Format("{0}{1}", CmdResult.Results[lineIdx], CmdResult.Results[lineIdx + 1].ToString().Replace("\t",""));
        }

        private bool LineContinues(string line1)
        {
            return line1.StartsWith("\t") || line1.StartsWith(" ");                
        }

        private void RemoveLine(int idx)
        {
            CmdResult.Results.RemoveAt(idx);
        }
    }
}
