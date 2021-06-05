using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BrowserNavigator {
    public struct Browser {
        public static string executablePathTpProcessName(string executablePath)
        {
            return System.Text.RegularExpressions
                .Regex.Match(executablePath, @".*\\(.*)\.exe.*").Groups[1].Value;
        }

        public readonly string name;
        public readonly string icon;
        public readonly string processName;
        public readonly string defaultExecutablePath;
        public readonly Dictionary<string, string> urlAssociations;
        public readonly HashSet<string> urlAssociationClasses;
        public readonly Dictionary<string, string> fileAssociations;
        public readonly HashSet<string> fileAssociationsClasses;

        public Browser(
            string name,
            string icon,
            string defaultExecutablePath,
            Dictionary<string, string> urlAssociations,
            HashSet<string> urlAssociationClasses,
            Dictionary<string, string> fileAssociations,
            HashSet<string> fileAssociationsClasses,
            string processExecutablePath = null
        ) {
            this.name = name;
            this.icon = icon;
            this.defaultExecutablePath = defaultExecutablePath;
            this.processName = Browser.executablePathTpProcessName(this.defaultExecutablePath);
            this.urlAssociations = urlAssociations;
            this.urlAssociationClasses = urlAssociationClasses;
            this.fileAssociations = fileAssociations;
            this.fileAssociationsClasses = fileAssociationsClasses;
        }

        public string getProcessName()
        {
            Process[] processes = Process.GetProcessesByName(this.processName);
            return processes.Length > 0 ? processes[0].MainModule?.FileName ?? null : null; 
        }

        public (string executable, string args) getUrlExecutableCommand(string url)
        {
            string urlScheme = new Uri(url).Scheme;
            string shell = this.fileAssociations[urlScheme];
            string processName = this.getProcessName();
            string executable = processName == null
                ? System.Text.RegularExpressions
                    .Regex.Match(shell, @"(.*\\.*\.exe).*").Groups[1].Value
                : processName;
            string args = System.Text.RegularExpressions
                .Regex.Match(shell, @".*.exe[^ ]* (.*)").Groups[1].Value;
            args = args.Replace("%1", url);
            return (executable, args);
        }

        public (string executable, string args) getFileExecutableCommand(string file)
        {
            file = file.Replace('/', '\\');
            string fileExtension = Path.GetExtension(file);
            string shell = this.fileAssociations[fileExtension];
            string processName = this.getProcessName();
            string executable = processName == null
                ? System.Text.RegularExpressions
                    .Regex.Match(shell, @"(.*\\.*\.exe).*").Groups[1].Value
                : processName;
            string args = System.Text.RegularExpressions
                .Regex.Match(shell, @".*.exe[^ ]* (.*)").Groups[1].Value;
            args = args.Replace("%1", file);
            return (executable, args);
        }
    }
}

