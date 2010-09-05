using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace EveImSync
{
    class ENScriptWrapper
    {
        public String ENScriptPath
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

        public List<String> GetNotebooks()
        {
            List<String> notebooks = new List<String>();

            ProcessStartInfo processStartInfo = new ProcessStartInfo(exePath, "listNotebooks");
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

        public bool ExportNotebook(String notebook, String exportFile)
        {
            bool bRet = false;
            ProcessStartInfo processStartInfo = new ProcessStartInfo(exePath, "exportNotes /q \"notebook:" + notebook +"\" /f " + exportFile);
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
                bRet = File.Exists(exportFile);
            }

            return bRet;
        }

        public bool ImportNotes(String notesPath, String notebook)
        {
            bool bRet = false;

            ProcessStartInfo processStartInfo = new ProcessStartInfo(exePath, "importNotes /n " + notebook + " /s " + notesPath);
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
                bRet = process.ExitCode == 0;
            }

            return bRet;
        }

        private String exePath;
    }
}
