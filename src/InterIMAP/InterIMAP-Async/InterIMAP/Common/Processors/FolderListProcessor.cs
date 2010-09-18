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
using InterIMAP.Asynchronous.Objects;
using InterIMAP.Common.Data;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Processors
{
    /// <summary>
    /// Processes the folder list returned from the server.
    /// </summary>
    public class FolderListProcessor : BaseProcessor
    {
        private readonly List<String> _tempResults;
        private char _delimiter;

        /// <summary>
        /// Instantiate a new FolderListProcessor
        /// </summary>
        public FolderListProcessor()
        {
            _tempResults = new List<string>();
        }
        
        /// <summary>
        /// Process the results of the command
        /// </summary>
        public override void ProcessResult()
        {
            base.ProcessResult();

            // first we strip out the text we dont need
            foreach (string s in CmdResult.Results)
            {
                if (s.Contains("OK LIST")) continue;
                if (Regex.IsMatch(s, @"^IMAP[0-9]+ [Oo][Kk].*$")) continue;
                _tempResults.Add(s.Replace("* LIST ",""));
            }

            // then we start processing whats left
            ProcessFolderDepth(0);
        }

        /// <summary>
        /// Recursive method that processes the strings stored in _tempResults, generating and adding the unique folders to the database.
        /// </summary>
        /// <param name="depth">The depth at which to process the folder structure, should only be invoked with 0, recursive calls will use other values.</param>
        private void ProcessFolderDepth(int depth)
        {
            List<String> linesToRemove = new List<string>();
            
            foreach (string line in _tempResults)
            {
                string[] parts = GetFolderParts(line);
                string fullPath = GetFullPath(parts, depth);
                IFolder parentFolder = null;

                if (depth > 0) // if this is a sub folder, need to find its parent
                {
                    FindFolder(GetFullPath(parts, depth - 1), out parentFolder);

                }

                IFolder foundFolder;
                if (FindFolder(fullPath, out foundFolder) == -1) // if this folder does not already exist
                {
                    _client.MailboxManager.AddFolder(parts[depth], parentFolder);
                    
                }

                if (parts.Length == depth+1) // this entry has no futher sub folders
                {
                    linesToRemove.Add(line);
                }

            }

            foreach (string l in linesToRemove)
                _tempResults.Remove(l);

            if (_tempResults.Count > 0) // if after removal we still have lines left, that means we have another level to process
                ProcessFolderDepth(++depth);
        }

        
        /// <summary>
        /// Parses the complete folder string, determining folder delimiter, stripping unneeded content,
        /// and generating the necessary array.
        /// </summary>
        /// <param name="folderString">The full folder string to parse</param>
        /// <returns>An array consisting of the individual folder names</returns>
        private string[] GetFolderParts(string folderString)
        {            
            int idx = folderString.IndexOf(")");
            string temp = folderString.Substring(idx + 2);
            _delimiter = temp[1];
            return temp.Substring(3).Replace("\"","").Trim().Split(new char[] { _delimiter });            
        }

        /// <summary>
        /// Searches the Folder table for a folder that has the specified fullpath
        /// </summary>
        /// <param name="fullpath">The full path to search for</param>
        /// <param name="foundFolder">An IFolder object for the folder that was found</param>
        /// <returns>The ID of the folder that was found</returns>
        private int FindFolder(string fullpath, out IFolder foundFolder)
        {
            foundFolder = null;

            Mailbox.FolderDataTable folderTable = _client.DataManager.FolderTable;
            if (folderTable.Rows.Count == 0)
                return -1;
            
            foreach (Mailbox.FolderRow row in folderTable.Rows)
            {
                if (!row.FullPath.Equals(fullpath)) continue;
                foundFolder = new Folder(_client, row.ID);
                return row.ID;
            }
                        
            return -1;
        }

        /// <summary>
        /// Generates the full path string based on the array passed and the depth level
        /// </summary>
        /// <param name="parts">An array representing the hierarchy of a given folder</param>
        /// <param name="depth">How deep to go into the supplied array</param>
        /// <returns>A single string reprsenting the full path of the folder</returns>
        private string GetFullPath(string[] parts, int depth)
        {
            if (depth == 0)
                return parts[0];
            
            List<String> finalparts = new List<string>();
            for (int i = 0; i <= depth; i++)            
                finalparts.Add(parts[i]);
            
            return string.Join(_delimiter.ToString(), finalparts.ToArray());
        }
    }
}
