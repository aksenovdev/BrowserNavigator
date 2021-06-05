using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace BrowserNavigator.Helpers.Registry
{

    [SupportedOSPlatform("windows")]
    public static partial class Browsers
    {
        private static HashSet<string> getBrowserClasses(RegistryKey registryKey)
        {
            string[] classes = Array.ConvertAll<string, string>(
                registryKey.GetValueNames(),
                (string valueName) => registryKey.GetValue(valueName)?.ToString() ?? ""
            );

            return new HashSet<string>(classes);
        }

        private static Func<RegistryKey, Dictionary<string, string>> createAssociationsListFactory(string defaultExecutablePath)
        {
            Dictionary<string, string> classes = new Dictionary<string, string>();

            return (RegistryKey registryKey) => new Dictionary<string, string>(
                System.Array.ConvertAll<string, KeyValuePair<string, string>>(
                    registryKey.GetValueNames(),
                    (string key) => {
                        var value = registryKey.GetValue(key);
                        string browserClass = registryKey.GetValue(key)!.ToString();
                        if (!classes.ContainsKey(browserClass))
                        {
                            using RegistryKey shellRegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"{browserClass}\shell\open\command");
                            string command = (shellRegistryKey?.GetValue(null, "") as string ?? "").Trim('"');

                            classes.Add(browserClass, command);
                        }
                     return new KeyValuePair<string, string>(key, classes[browserClass]);
                    }
                )
            );
        }

        public static List<Browser> getBrowsers()
        {
            using RegistryKey browsersRegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            List<string> browserRegistryNames = new List<string>(browsersRegistryKey.GetSubKeyNames());
            browserRegistryNames.Remove("IEXPLORE.EXE");

            return browserRegistryNames.ConvertAll<Browser>((string browserRegistryName) =>
                {
                    using RegistryKey browserRegistryKey = browsersRegistryKey.OpenSubKey(browserRegistryName);
                    using RegistryKey openShellRegistryKey = browserRegistryKey.OpenSubKey(@"shell\open\command");
                    using RegistryKey capabilitiesRegistryKey = browserRegistryKey.OpenSubKey("Capabilities");
                    using RegistryKey urlAssociationsRegistryKey = capabilitiesRegistryKey.OpenSubKey("UrlAssociations");
                    using RegistryKey fileAssociationsRegistryKey = capabilitiesRegistryKey.OpenSubKey("FileAssociations");
                    string defaultExecutablePath = (openShellRegistryKey?.GetValue(null, "") as string ?? "").Trim('"');
                    Func<RegistryKey, Dictionary<string, string>> associationsListFactory = createAssociationsListFactory(defaultExecutablePath);

                    string name = capabilitiesRegistryKey.GetValue("ApplicationName", "") as string;
                    string icon = capabilitiesRegistryKey.GetValue("ApplicationIcon", "") as string;
                    Dictionary<string, string> urlAssociations = associationsListFactory(urlAssociationsRegistryKey);
                    HashSet<string> urlAssociationsClasses = getBrowserClasses(urlAssociationsRegistryKey);
                    Dictionary<string, string> fileAssociations = associationsListFactory(fileAssociationsRegistryKey);
                    HashSet<string> fileAssociationsClasses = getBrowserClasses(fileAssociationsRegistryKey);

                    return new Browser(
                        name,
                        icon,
                        defaultExecutablePath,
                        urlAssociations,
                        urlAssociationsClasses,
                        fileAssociations,
                        fileAssociationsClasses
                    );
                }
            );
        }

        public static (List<Browser> browsers, Browser defaultBrowser, Browser browserNavigation) getSplittedBrowsers()
        {
            List<Browser> allBrowsers = getBrowsers();

            Browser defaultBrowser = getInitialDefaultBrowser(allBrowsers);
            allBrowsers.Remove(defaultBrowser);

            Browser applicationBrowser = allBrowsers
                .Find((Browser browser) => browser.name == "Browser Navigator");
            allBrowsers.Remove(applicationBrowser);
            
            return (allBrowsers, defaultBrowser, applicationBrowser);
        }

        public static Browser getInitialDefaultBrowser(List<Browser> browsers = null)
        {
            browsers = browsers ?? getBrowsers();

            using RegistryKey capabilitiesKey = Microsoft.Win32.Registry.LocalMachine
                .OpenSubKey(@"Software\Clients\StartMenuInternet\BrowserNavigator\Capabilities")!;
            string defaultBrowserName = capabilitiesKey?.GetValue("InitialDefaultBrowserName", "")?.ToString() ?? "";

            return browsers
                .Find((Browser browser) => browser.name == defaultBrowserName);
        }
    }
}
