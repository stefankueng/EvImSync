// Evernote2Onenote - imports Evernote notes to Onenote
// Copyright (C) 2014 - Stefan Kueng

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Evernote2Onenote
{
    /// <summary>
    /// Wrapper for the ENScript.exe tool
    /// </summary>
    public class ENScriptWrapper
    {
        /// <summary>
        /// path to the ENScript.exe
        /// </summary>
        private string exePath;

        /// <summary>
        /// the full path to ENScript.exe
        /// </summary>
        public string ENScriptPath
        {
            get
            {
                return exePath;
            }

            set
            {
                exePath = value;
            }
        }

        /// <summary>
        /// Lists all the notebooks in Evernote
        /// </summary>
        /// <returns>List of notebook names</returns>
        public List<string> GetNotebooks()
        {
            List<string> notebooks = new List<string>();

            ProcessStartInfo processStartInfo = new ProcessStartInfo(this.exePath, "listNotebooks");
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            bool processStarted = process.Start();
            if (processStarted)
            {
                StreamWriter inputWriter = process.StandardInput;
                StreamReader outputReader = process.StandardOutput;
                StreamReader errorReader = process.StandardError;
                process.WaitForExit();
                while (outputReader.Peek() >= 0)
                {
                    notebooks.Add(outputReader.ReadLine());
                }
            }

            return notebooks;
        }

        /// <summary>
        /// Exports the specified notebook to the specified file
        /// </summary>
        /// <param name="notebook">the notebook to export</param>
        /// <param name="exportFile">the file to export the notebook to</param>
        /// <returns>true if successful, false in case of an error</returns>
        public bool ExportNotebook(string notebook, string exportFile)
        {
            bool ret = false;
            if (!File.Exists(exePath))
                return ret;

            ProcessStartInfo processStartInfo = new ProcessStartInfo(this.exePath, "exportNotes /q \"notebook:" + notebook + "\" /f " + exportFile);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            bool processStarted = process.Start();
            if (processStarted)
            {
                StreamWriter inputWriter = process.StandardInput;
                StreamReader outputReader = process.StandardOutput;
                StreamReader errorReader = process.StandardError;
                process.WaitForExit();
                ret = (process.ExitCode == 0) && File.Exists(exportFile);
            }

            return ret;
        }

        /// <summary>
        /// Imports all the notes in the export file to the 
        /// specified notebook in Evernote
        /// </summary>
        /// <param name="notesPath">the path to the export file</param>
        /// <param name="notebook">the notebook where the export file should be imported to</param>
        /// <returns>true if successful, false in case of an error</returns>
        public bool ImportNotes(string notesPath, string notebook)
        {
            bool ret = false;

            ProcessStartInfo processStartInfo = new ProcessStartInfo(this.exePath, "importNotes /n " + notebook + " /s " + notesPath);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            bool processStarted = process.Start();
            if (processStarted)
            {
                StreamWriter inputWriter = process.StandardInput;
                StreamReader outputReader = process.StandardOutput;
                StreamReader errorReader = process.StandardError;
                process.WaitForExit();
                ret = process.ExitCode == 0;
            }

            return ret;
        }
    }
}
