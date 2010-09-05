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
using System.Text.RegularExpressions;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Processors
{
    /// <summary>
    /// Processes the results of the FolderData command
    /// </summary>
    public class FolderDataProcessor : BaseProcessor
    {
        #region Private Fields
        private IFolder _folder;
        #endregion

        public override void ProcessResult()
        {
            base.ProcessResult();

            _folder = (IFolder)Request.Command.ParameterObjects[0];
            int exists;
            int recent;
            int unseen;
            ParseFolderResult(out exists, out recent, out unseen);
            _folder.Exists = exists;
            _folder.Recent = recent;
        }

        private void ParseFolderResult(out int exists, out int recent, out int unseen)
        {
            exists = 0;
            recent = 0;
            unseen = 0;

            const string existsPattern = "^[\\s]*\\*\\s(?<exists>(\\d+))\\s[Ee][Xx][Ii][Ss][Tt][Ss]$";
            const string recentPattern = "^[\\s]*\\*\\s(?<recent>(\\d+))\\s[Rr][Ee][Cc][Ee][Nn][Tt]$";
            const string unseenPattern = "^[\\s]\\*\\s[Oo][Kk]\\s\\[UNSEEN\\s(?<unseen>(\\d*))\\]$";

            foreach (string line in CmdResult.Results)
            {

                // * 21 EXISTS
                // * 0 RECENT
                // * OK [UNSEEN 0]
                Match match = Regex.Match(line, existsPattern, RegexOptions.ExplicitCapture);
                if (match.Success)
                {
                    exists = Convert.ToInt32(match.Groups["exists"].Value);
                    continue;
                }

                match = Regex.Match(line, recentPattern, RegexOptions.ExplicitCapture);
                if (match.Success)
                {
                    recent = Convert.ToInt32(match.Groups["recent"].Value);
                    continue;
                }

                match = Regex.Match(line, unseenPattern, RegexOptions.ExplicitCapture);
                if (match.Success)
                {
                    unseen = Convert.ToInt32(match.Groups["unseen"].Value);
                    continue;
                }
                
                
            }
        }
    }
}
