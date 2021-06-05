#nullable enable

using System.Collections.Generic;

namespace BrowserNavigator.Helpers.Registry {
    public static partial class Browsers {
        public static Browser? detectLaunchedBrowser(IEnumerable<Browser> browsers)
        {
            foreach (Browser browserData in browsers)
            {
                bool browserLaunched = browserData.getProcessName() != null;
                if (browserLaunched)
                {
                    return browserData;
                }
            }

            return null;
        }
    }
}
