using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V145.Network;
using System.Text.Json;
using System.Collections.Concurrent;

namespace Enfinity.ERP.Automation.Core.Utilities
{
    public class NetworkHelper
    {
        private readonly IWebDriver _driver;
        private readonly DevToolsSession _devTools;

        // Store multiple responses safely
        private readonly ConcurrentDictionary<string, string> _responses = new();

        private string _urlFilter = string.Empty;

        public NetworkHelper(IWebDriver driver)
        {
            _driver = driver;
            _devTools = ((IDevTools)_driver).GetDevToolsSession();
        }

        // ─────────────────────────────────────────────
        // Start capturing API responses
        // ─────────────────────────────────────────────
        public void StartCapture(string urlContains)
        {
            _urlFilter = urlContains;

            var domains = _devTools.GetVersionSpecificDomains<OpenQA.Selenium.DevTools.V145.DevToolsSessionDomains>();
            var network = domains.Network;

            network.Enable(new EnableCommandSettings());

            network.ResponseReceived += async (sender, e) =>
            {
                try
                {
                    if (!e.Response.Url.Contains(_urlFilter)) return;

                    var body = await network.GetResponseBody(new GetResponseBodyCommandSettings
                    {
                        RequestId = e.RequestId
                    });

                    _responses[e.Response.Url] = body.Body;
                }
                catch
                {
                    // Ignore intermittent failures
                }
            };
        }

        // ─────────────────────────────────────────────
        // Wait & Get Latest Matching Response
        // ─────────────────────────────────────────────
        public T GetResponse<T>(int timeoutSeconds = 10)
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));

            wait.Until(_ => _responses.Count > 0);

            var latest = _responses.Last().Value;

            return JsonSerializer.Deserialize<T>(latest)!;
        }

        // ─────────────────────────────────────────────
        // Optional: Clear old responses
        // ─────────────────────────────────────────────
        public void Clear()
        {
            _responses.Clear();
        }
    }
}