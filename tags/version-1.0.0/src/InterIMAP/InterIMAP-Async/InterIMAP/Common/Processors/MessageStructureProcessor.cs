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
using System.Text.RegularExpressions;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Processors
{
    /// <summary>
    /// Processes the result of the MessageStructure command
    /// </summary>
    public class MessageStructureProcessor : BaseProcessor
    {
        private IMessage _msg;

        public override void ProcessResult()
        {
            _msg = _request.Command.ParameterObjects[0] as IMessage;
            if (_msg == null) throw new ArgumentException("No valid IMessage object found.");
            
            base.ProcessResult();
            foreach (string s in CmdResult.Results)
            {
                if (!s.StartsWith("*")) continue;
                string s2 = s.Substring(s.IndexOf(" (", s.IndexOf("BODYSTRUCTURE")));
                ParseBodyStructure(s2.Trim().Substring(0, s2.Trim().Length - 1));                
                
            }

            

        }
        

        private List<IMessageContent> ParseBodyStructure(string input)
        {            
            List<IMessageContent> msgContentList = new List<IMessageContent>();

            int i = 0;
            int index = 1;
            int count = 0;
            do
            {
                int next = index;
                do
                {
                    if (input[next] == '(')
                        i++;
                    else if (input[next] == ')')
                        i--;
                    next++;
                } while (i > 0 || input[next - 1] != ')');
                if (i >= 0 && input[index] == '(')
                {
                    count++;                    
                    if (input.Substring(index, next - index).StartsWith("(("))
                    {
                        List<IMessageContent> temp = ParseBodyStructure(input.Substring(index, next - index));
                        for (int j = 0; j < temp.Count; j++)
                        {
                            //ParseContentPart(temp[j],);
                            temp[j].PartID = count + "." + temp[j].PartID;
                            msgContentList.Add(temp[j]);
                        }
                    }
                    else
                    {
                        IMessageContent Part = _request.Client.MailboxManager.AddMessageContent(_msg.ID);
                        ParseContentPart(Part, input.Substring(index, next - index));                        
                        Part.PartID = count.ToString();                        
                        msgContentList.Add(Part);
                    }
                }
                else if (msgContentList.Count == 0)
                {
                    IMessageContent Part = _request.Client.MailboxManager.AddMessageContent(_msg.ID);
                    Part.TextData = null;                       
                    Part.PartID = "1";
                    ParseContentPart(Part, input);
                    msgContentList.Add(Part);
                }
                index = next;
            } while (i >= 0);

            return msgContentList;
        }

        private void ParseContentPart(IMessageContent part, string s)
        {
            const string non_attach = "^\\((?<type>(\"[^\"]*\"|NIL))\\s(?<subtype>(\"[^\"]*\"|NIL))\\s(?<attr>(\\(.*\\)|NIL))\\s(?<id>(\"[^\"]*\"|NIL))\\s(?<desc>(\"[^\"]*\"|NIL))\\s(?<encoding>(\"[^\"]*\"|NIL))\\s(?<size>(\\d+|NIL))\\s(?<lines>(\\d+|NIL))\\s(?<md5>(\"[^\"]*\"|NIL))\\s(?<disposition>(\\([^\\)]*\\)|NIL))\\s(?<lang>(\"[^\"]*\"|NIL))\\)$";
            const string attachment = "^\\((?<type>(\"[^\"]*\"|NIL))\\s(?<subtype>(\"[^\"]*\"|NIL))\\s(?<attr>(\\(.*\\)|NIL))\\s(?<id>(\"[^\"]*\"|NIL))\\s(?<desc>(\"[^\"]*\"|NIL))\\s(?<encoding>(\"[^\"]*\"|NIL))\\s(?<size>(\\d+|NIL))\\s((?<data>(.*))\\s|)(?<lines>(\"[^\"]*\"|NIL))\\s(?<disposition>((?>\\((?<LEVEL>)|\\)(?<-LEVEL>)|(?!\\(|\\)).)+(?(LEVEL)(?!))|NIL))\\s(?<lang>(\"[^\"]*\"|NIL))\\)$";
            const string alt_attach = "^\\([\"]*(?<type>([\\w]*))[\"\\s]*(?<subtype>(\\w*))[\"\\s]*\\([\"*]\\w*[\"\\s\"]*(?<filename>([^\"]*))[\")\\s]*NIL\\sNIL\\s\"(?<encoding>(\\w*))[\"]\\s(?<size>(\\d*))";
            Match match;
            if ((match = Regex.Match(s, non_attach, RegexOptions.ExplicitCapture)).Success)
            {
                //this.Attachment = false;
                part.ContentType = string.Format("{0}/{1}", match.Groups["type"].Value.Replace("\"", ""), match.Groups["subtype"].Value.Replace("\"", ""));
                part.Charset = ParseCharacterSet(ParseNIL(match.Groups["attr"].Value));
                part.ContentId = ParseNIL(match.Groups["id"].Value);
                part.ContentDescription = ParseNIL(match.Groups["desc"].Value);
                part.ContentTransferEncoding = ParseNIL(match.Groups["encoding"].Value);
                part.ContentSize = Convert.ToInt64(ParseNIL(match.Groups["size"].Value));
                part.Lines = Convert.ToInt64(ParseNIL(match.Groups["lines"].Value));                
                part.MD5 = ParseNIL(match.Groups["md5"].Value);
                part.ContentDisposition = ParseNIL(match.Groups["disposition"].Value);
                part.Language = ParseNIL(match.Groups["lang"].Value);
            }
            else if ((match = Regex.Match(s, attachment, RegexOptions.ExplicitCapture)).Success)
            {
                //this.Attachment = true;
                part.ContentType = string.Format("{0}/{1}", match.Groups["type"].Value.Replace("\"", ""), match.Groups["subtype"].Value.Replace("\"", ""));
                part.ContentFilename = ParseFileName(ParseNIL(match.Groups["attr"].Value));
                part.ContentId = ParseNIL(match.Groups["id"].Value);
                part.ContentDescription = ParseNIL(match.Groups["desc"].Value);
                part.ContentTransferEncoding = ParseNIL(match.Groups["encoding"].Value.Replace("\"",""));
                part.ContentSize = Convert.ToInt64(ParseNIL(match.Groups["size"].Value));
                part.Lines = Convert.ToInt64(ParseNIL(match.Groups["lines"].Value));
                part.ContentDisposition = ParseNIL(match.Groups["disposition"].Value);
                part.Language = ParseNIL(match.Groups["lang"].Value);
            }
            else if ((match = Regex.Match(s, alt_attach, RegexOptions.ExplicitCapture)).Success)
            {
                part.ContentType = string.Format("{0}/{1}", match.Groups["type"].Value, match.Groups["subtype"].Value);
                part.ContentFilename = match.Groups["filename"].Value;
                part.ContentTransferEncoding = match.Groups["encoding"].Value;
                part.ContentSize = Convert.ToInt64(match.Groups["size"].Value);
            }
            else
                throw new Exception("Invalid format could not parse body part headers.");
        
        }

        private string ParseCharacterSet(string data)
        {
            if (data == null) return null;
            
            Match match = Regex.Match(data.ToLower(), "\"charset\"\\s\"(?<set>([^\"]*))\"", RegexOptions.ExplicitCapture);
            if (match.Success)
                return String.Format("{0}", match.Groups["set"].Value);

            return null;
            
        }

        private string ParseFileName(string data)
        {
            if (data == null) return null;
            
            
            Match match = Regex.Match(data, "\"name\"\\s\"(?<file>([^\"]*))\"", RegexOptions.ExplicitCapture);
            if (match.Success)
                return match.Groups["file"].Value;

            // ("ATTACHMENT" ("FILENAME" "some file.pdf"))
            match = Regex.Match(data, "^\\(\"[Aa][Tt][Tt][Aa][Cc][Hh][Mm][Ee][Nn][Tt]\"\\s\\(\"[Ff][Ii][Ll][Ee][Nn][Aa][Mm][Ee]\"\\s\"(?<file>(.*))\"\\)\\)$", RegexOptions.ExplicitCapture);
            if (match.Success)
                return match.Groups["file"].Value;

            // ("NAME" "some file.pdf")
            match = Regex.Match(data, "^\\(\"[Nn][Aa][Mm][Ee]\"\\s\"(?<file>(.*))\"\\)", RegexOptions.ExplicitCapture);
            if (match.Success)
                return match.Groups["file"].Value;

            return null;
            
        }

        private string ParseNIL(string data)
        {
            if (data.Trim() == "NIL")
                return null;
            return data ?? "";
        }
    }
}
