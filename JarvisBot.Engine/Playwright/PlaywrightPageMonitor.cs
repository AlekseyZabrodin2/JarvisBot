using JarvisBot.Core.Enums;
using JarvisBot.Core.Interfaces;
using JarvisBot.Core.Models;
using Microsoft.Playwright;
using NLog;

namespace JarvisBot.Engine.Playwright
{
    public sealed class PlaywrightPageMonitor : IPageMonitor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly PlaywrightBrowser _browser;


        public PlaywrightPageMonitor(PlaywrightBrowser browser)
        {
            _browser = browser;
        }


        public async Task<MonitoringResult> CheckAsync(WatchTask task, CancellationToken cancellationToken = default)
        {
            var checkedAt = DateTimeOffset.UtcNow;

            try
            {
                await using var context = await _browser.CreateContextAsync();

                var page = await context.NewPageAsync();

                Logger.Info("Checking URL [{Url}] for task [{TaskId}]", task.Url, task.Id);

                await page.GotoAsync( task.Url.ToString(),
                    new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded
                    });

                cancellationToken.ThrowIfCancellationRequested();

                var content = await page.Locator("body").InnerTextAsync();

                var conditionMet = task.ConditionType switch
                {
                    ConditionType.TextExists => content.Contains(task.ConditionValue, StringComparison.OrdinalIgnoreCase),

                    _ => false
                };

                var value = content.Length > 4000 ? content[..4000] : content;

                Logger.Info("Page loaded. Title: [{Title}], Content length: [{Length}]", await page.TitleAsync(), content.Length);

                return new MonitoringResult
                {
                    TaskId = task.Id,
                    CheckedAt = checkedAt,
                    IsSuccess = true,
                    ConditionMet = conditionMet,
                    Value = value
                };
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error checking task [{TaskId}]", task.Id);

                return new MonitoringResult
                {
                    TaskId = task.Id,
                    CheckedAt = checkedAt,
                    IsSuccess = false,
                    ConditionMet = false,
                    Error = ex.Message
                };
            }
        }
    }
}
