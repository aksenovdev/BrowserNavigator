#nullable enable

using System;
using System.Runtime.Versioning;

namespace BrowserNavigator
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        static void Main(string[] args)
        {
            string command = args[0];
            switch (command)
            {
                case "-h":
                case "--help":
                    Console.WriteLine("Commands:");
                    Console.WriteLine("    -h, --help - register app in registry");
                    Console.WriteLine("    --register - register app in registry");
                    Console.WriteLine("    --unregister - unregister app from registry");
                    break;
                case "--register":
                    Helpers.Registry.Application.registerApplication();
                    break;
                case "--unregister":
                    Helpers.Registry.Application.unregisterApplication();
                    break;
                default:
                    var (browsers, defaultBrowser, _) = Helpers.Registry.Browsers.getSplittedBrowsers();

                    Browser? launchedBrowser = Helpers.Registry.Browsers.detectLaunchedBrowser(browsers);
                    Browser targetBrowser = launchedBrowser ?? defaultBrowser;
                    var (executable, executableArgs) = System.IO.File.Exists(command)
                        ? targetBrowser.getFileExecutableCommand(command)
                        : targetBrowser.getUrlExecutableCommand(command);
                    System.Diagnostics.Process.Start(executable, executableArgs);
                    break;
            }
        }
    }
}
