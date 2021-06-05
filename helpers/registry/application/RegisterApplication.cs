#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace BrowserNavigator.Helpers.Registry {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static partial class Application {
        private static Browser getDefaultBrowser()
        {
            using RegistryKey? userChoiceKey = Microsoft.Win32.Registry.CurrentUser
                .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.htm\UserChoice", false);
            string defaultBrowserClass = userChoiceKey?.GetValue("ProgId", "")?.ToString() ?? "";

            return Browsers.getBrowsers()
                .Find(
                    (Browser browser) => browser.urlAssociationClasses.Contains(defaultBrowserClass)
                        || browser.fileAssociationsClasses.Contains(defaultBrowserClass)
                );
        }

        public static void registerApplication()
        {
            unregisterApplication();

            string currentFileName = System.Diagnostics.Process
                .GetCurrentProcess().MainModule?.FileName?.Replace(@"\", @"\\")!;
            Browser defaultBrowser = getDefaultBrowser();
            string path = System.IO.Path.Join(AppDomain.CurrentDomain.BaseDirectory, "RegApp.reg");
            string icon = defaultBrowser.icon.Replace(@"\", @"\\");
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(@"Windows Registry Editor Version 5.00");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\Software\Clients\StartMenuInternet\BrowserNavigator\Capabilities]");
                sw.WriteLine($@"""ApplicationIcon""=""{icon}""");
                sw.WriteLine(@"""ApplicationDescription""=""Open links in launched browser instead default.""");
                sw.WriteLine(@"""ApplicationName""=""Browser Navigator""");
                sw.WriteLine($@"""InitialDefaultBrowserName""=""{defaultBrowser.name}""");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\Software\Clients\StartMenuInternet\BrowserNavigator\DefaultIcon]");
                sw.WriteLine($@"@=""{icon}""");
                sw.WriteLine(@"");
                sw.WriteLine(@"; Infamous capabilities:");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\Software\Clients\StartMenuInternet\BrowserNavigator\Capabilities\FileAssociations]");
                sw.WriteLine(@""".htm""=""BrowserNavigatorHTML""");
                sw.WriteLine(@""".html""=""BrowserNavigatorHTML""");
                sw.WriteLine(@""".shtml""=""BrowserNavigatorHTML""");
                sw.WriteLine(@""".xht""=""BrowserNavigatorHTML""");
                sw.WriteLine(@""".xhtml""=""BrowserNavigatorHTML""");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\Software\Clients\StartMenuInternet\BrowserNavigator\Capabilities\Startmenu]");
                sw.WriteLine(@"""StartMenuInternet""=""Browser Navigator""");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\Software\Clients\StartMenuInternet\BrowserNavigator\Capabilities\UrlAssociations]");
                sw.WriteLine(@"""http""=""BrowserNavigatorHTML""");
                sw.WriteLine(@"""https""=""BrowserNavigatorHTML""");
                sw.WriteLine(@"""ftp""=""BrowserNavigatorHTML""");
                sw.WriteLine(@"""mailto""=""BrowserNavigatorHTML""");
                sw.WriteLine(@"");
                sw.WriteLine(@"; MyAppURL HANDLER:");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\Software\Classes\BrowserNavigatorHTML]");
                sw.WriteLine(@"@=""Browser Navigator HTML Document""");
                sw.WriteLine(@"""FriendlyTypeName""=""Browser Navigator HTML Document""");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\BrowserNavigatorHTML\Application]");
                sw.WriteLine($@"""ApplicationIcon""=""{icon}""");
                sw.WriteLine(@"""ApplicationName""=""Browser Navigator""");
                sw.WriteLine(@"""ApplicationDescription""=""Select browser for open link""");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\SOFTWARE\Classes\BrowserNavigatorHTML\DefaultIcon]");
                sw.WriteLine($@"@=""{icon}""");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\Software\Classes\BrowserNavigatorHTML\shell]");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\Software\Classes\BrowserNavigatorHTML\shell\open]");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\Software\Classes\BrowserNavigatorHTML\shell\open\command]");
                sw.WriteLine($@"@=""\""{currentFileName}\"" %1""");
                sw.WriteLine(@"");
                sw.WriteLine(@"; Register to Default Programs");
                sw.WriteLine(@"");
                sw.WriteLine(@"[HKEY_LOCAL_MACHINE\SOFTWARE\RegisteredApplications]");
                sw.WriteLine(@"""BrowserNavigator""=""Software\\Clients\\StartMenuInternet\\BrowserNavigator\\Capabilities""");
            }	
        
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "regedit.exe";
                process.StartInfo.Arguments = $@"/s {path}";
                process.StartInfo.UseShellExecute = true;
                process.Start();
                process.WaitForExit();
            }

            File.Delete(path);
        }

        public static void unregisterApplication()
        {
            string path = System.IO.Path.Join(AppDomain.CurrentDomain.BaseDirectory, "UnregApp.reg");
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(@"Windows Registry Editor Version 5.00");
                sw.WriteLine(@"");
                sw.WriteLine(@"[-HKEY_LOCAL_MACHINE\Software\Clients\StartMenuInternet\BrowserNavigator]");
                sw.WriteLine(@"[-HKEY_LOCAL_MACHINE\Software\Classes\BrowserNavigatorHTML]");
                sw.WriteLine(@"[-HKEY_LOCAL_MACHINE\SOFTWARE\RegisteredApplications\BrowserNavigator]");
            }	
        
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "regedit.exe";
                process.StartInfo.Arguments = $@"/s {path}";
                process.StartInfo.UseShellExecute = true;
                process.Start();
                process.WaitForExit();
            }

            File.Delete(path);
        }
    }
}
