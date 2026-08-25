using Microsoft.Extensions.Hosting;

namespace JarvisBot.Engine.Playwright
{
    public sealed class PlaywrightBrowserHostedService : IHostedService
    {
        private readonly PlaywrightBrowser _browser;

        public PlaywrightBrowserHostedService(PlaywrightBrowser browser)
        {
            _browser = browser;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _browser.InitializeAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _browser.DisposeAsync();
        }
    }
}