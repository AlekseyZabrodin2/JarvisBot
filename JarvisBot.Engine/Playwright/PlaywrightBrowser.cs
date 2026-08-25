using Microsoft.Playwright;
using NLog;

namespace JarvisBot.Engine.Playwright
{
    public sealed class PlaywrightBrowser : IAsyncDisposable
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private IPlaywright? _playwright;
        private IBrowser? _browser;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            if (!File.Exists(_playwright.Chromium.ExecutablePath))
            {
                _logger.Info("Chromium not found. Installing Playwright browser...");

                Program.Main(new[] { "install" });
            }

            _logger.Info("Chromium installation completed.");

            _browser = await _playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = true
                });
        }

        public async Task<IBrowserContext> CreateContextAsync()
        {
            if (_browser is null)
                throw new InvalidOperationException("Playwright browser is not initialized.");

            return await _browser.NewContextAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser is not null)
                await _browser.DisposeAsync();

            _playwright?.Dispose();

            _browser = null;
            _playwright = null;
        }
    }
}
