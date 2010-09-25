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
using System.Text;
using System.Text.RegularExpressions;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Processors
{
    /// <summary>
    /// Processes the result of the MessagePart command
    /// </summary>
    public class MessagePartProcessor : BaseProcessor
    {
        public override void ProcessResult()
        {
            base.ProcessResult();

            IMessageContent content = Request.Command.ParameterObjects[0] as IMessageContent;
            if (content == null) throw new ArgumentException("IMessageContent object is null");
            //* 72 FETCH (UID 4913 BODY[1] {6508}
            //const string partFetchHeader = @"^[\*] [0-9]+ FETCH \([UID [0-9]+]*BODY\[[0-9]+\] \{[0-9]+\}$";
            const string partFetchHeader = "^\\*\\s*\\d*\\s*\\w*";
            const string partFetchPreFooter = "^\\s*\\w*\\s*[0-9]*\\)$";
            const string partFetchFooter = "^IMAP[0-9]+\\s[Oo][Kk]";


            string ctType = content.ContentType.ToString().ToUpper(); ;
            string ctDisposition = null;
            if (content.ContentDisposition != null)
            {
                ctDisposition = content.ContentDisposition.ToString().ToUpper();
            }
            string sContentTransferEncoding = content.ContentTransferEncoding.ToUpper();

            bool isBinary = content.ContentTransferEncoding.ToUpper().Contains("BASE64");
            bool isHTML = content.ContentType.ToUpper().Contains("HTML");
            bool isQuotedPrintable = content.ContentType.ToUpper().Contains("QUOTED");
            StringBuilder sb = new StringBuilder();

            foreach (string line in CmdResult.Results)
            {
                if (Regex.IsMatch(line.ToUpper(), partFetchHeader)) continue;
                if (Regex.IsMatch(line.ToUpper(), partFetchPreFooter)) continue;
                if (Regex.IsMatch(line.ToUpper(), partFetchFooter))
                    continue;

                if (!isBinary)
                {
                    sb.AppendLine(line);
                }
                else
                {

                    Match match;
                    if ((match = Regex.Match(line, "^[a-zA-Z0-9+//=]*|(?<invalid>([^a-zA-Z0-9+//=]+))$")).Success)
                    {
                        if (!String.IsNullOrEmpty(match.Groups["invalid"].Value))
                        {
                            string badChars = match.Groups["invalid"].Value;
                            sb.Append(line.Replace(badChars, ""));
                        }
                        else
                        {
                            sb.Append(line);
                        }

                    }

                }
            }

            if (isBinary)
            {
                if (isHTML && ((ctDisposition == null) || !ctDisposition.Contains("ATTACHMENT")) && sContentTransferEncoding.Contains("BASE64"))
                {
                    string preEncoded = sb.ToString().Trim().TrimEnd(')');
                    content.HTMLData = base64Decode(preEncoded);
                }
                else if (ctType.Contains("TEXT") && ((ctDisposition == null) || !ctDisposition.Contains("ATTACHMENT")) && sContentTransferEncoding.Contains("BASE64"))
                {
                    string preEncoded = sb.ToString().Trim().TrimEnd(')');
                    content.TextData = base64Decode(preEncoded);
                }
                else
                {
                    try
                    {
                        string binaryStringToDecrypt = sb.ToString().Replace(" ", "+").Trim().TrimEnd(')');
                        byte[] bdata = Convert.FromBase64String(binaryStringToDecrypt);
                        content.BinaryData = bdata;
                    }
                    catch (FormatException fe)
                    {
                        _client.Aggregator.AddMessage(0, fe.Message);
                    }
                }
            }
            else
            {
                if (isHTML)
                {
                    string preEncodedHTML = sb.ToString().Trim().TrimEnd(')');
                    if (string.IsNullOrEmpty(content.Charset))
                        content.Charset = "utf-8";
                    charSet = content.Charset;
                    if (isQuotedPrintable)
                        content.HTMLData = DecodeQuotedPrintable(preEncodedHTML);
                    else
                        content.HTMLData = preEncodedHTML;
                }
                else
                {
                    string preEncoded = sb.ToString().Trim().TrimEnd(')');
                    content.TextData = String.IsNullOrEmpty(content.Charset) ? preEncoded : Decode(preEncoded, Encoding.GetEncoding(content.Charset));
                }
            }
        }

        private string charSet = "";

        private string Decode(string input, Encoding enc)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            MatchCollection matches = Regex.Matches(input, @"\=(?<num>[0-9A-Fa-f]{2})");
            foreach (Match match in matches)
            {

                int i = int.Parse(match.Groups["num"].Value, System.Globalization.NumberStyles.HexNumber);
                char str = (char)i;
                input = input.Replace(match.Groups[0].Value, str.ToString());
            }
            byte[] bytes = Encoding.Default.GetBytes(input);
            string decoded = enc.GetString(bytes);
            return decoded;
        }

        public string base64Decode(string data)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Decode" + e.Message);
            }
        }

        public string DecodeQuotedPrintable(string encoded)
        {
            Regex re = new Regex(
                                "(\\=([0-9A-F][0-9A-F]))",
                                RegexOptions.IgnoreCase
                        );

            string decoded = re.Replace(encoded, new MatchEvaluator(HexDecoderEvaluator));
            return decoded.Replace("_", " ");
        }

        private string HexDecoderEvaluator(Match m)
        {
            string hex = m.Groups[2].Value;
            int iHex = Convert.ToInt32(hex, 16);

            byte[] bytes = new byte[1];
            bytes[0] = Convert.ToByte(iHex);
            return Encoding.GetEncoding(charSet).GetString(bytes);

        }

        /*
                private string Decode(string input)
                {
                    if (string.IsNullOrEmpty(input))
                        return "";
                    Regex regex = new Regex(@"=\?(?<Encoding>[^\?]+)\?(?<Method>[^\?]+)\?(?<Text>[^\?]+)\?=");
                    MatchCollection matches = regex.Matches(input);
                    string ret = input;
                    foreach (Match match in matches)
                    {
                        string encoding = match.Groups["Encoding"].Value;
                        string method = match.Groups["Method"].Value;
                        string text = match.Groups["Text"].Value;
                        string decoded;
                        if (method == "B")
                        {
                            byte[] bytes = Convert.FromBase64String(text);
                            Encoding enc = Encoding.GetEncoding(encoding);
                            decoded = enc.GetString(bytes);
                        }
                        else
                            decoded = Decode(text, Encoding.GetEncoding(encoding));
                        ret = ret.Replace(match.Groups[0].Value, decoded);
                    }
                    return ret;
                }
        */


    }
}
