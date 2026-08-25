using JarvisBot.Core.Interfaces;
using JarvisBot.Core.Models;
using NLog;

namespace JarvisBot.Engine.Monitoring;

public sealed class MonitoringService
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly IWatchTaskRepository _watchTaskRepository;
    private readonly MonitoringTaskRunner _taskRunner;
    private readonly Dictionary<Guid, CancellationTokenSource> _runningTasks = new();



    public MonitoringService(IWatchTaskRepository watchTaskRepository, MonitoringTaskRunner taskRunner)
    {
        _watchTaskRepository = watchTaskRepository;
        _taskRunner = taskRunner;
    }



    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Monitoring service started.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tasks = await _watchTaskRepository.GetEnabledAsync(cancellationToken);

                    SynchronizeTasks(tasks, cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error while synchronizing monitoring tasks.");

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
            }
        }
        finally
        {
            await StopAllTasksAsync();
        }

        Logger.Info("Monitoring service stopped.");
    }

    private void SynchronizeTasks(IReadOnlyList<WatchTask> tasks, CancellationToken cancellationToken)
    {
        var enabledTaskIds = tasks.Select(x => x.Id).ToHashSet();

        var tasksToStop = _runningTasks.Where(x => !enabledTaskIds.Contains(x.Key))
            .Select(x => x.Key)
            .ToList();

        foreach (var taskId in tasksToStop)
        {
            StopTask(taskId);
        }

        foreach (var task in tasks)
        {
            if (_runningTasks.ContainsKey(task.Id))
            {
                continue;
            }

            StartTask(task, cancellationToken);
        }
    }

    private void StartTask(WatchTask task, CancellationToken applicationCancellationToken)
    {
        var taskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(applicationCancellationToken);

        _runningTasks[task.Id] = taskCancellationTokenSource;

        Logger.Info("Starting monitoring task [{TaskId}] [{Name}].", task.Id, task.Name);

        _ = RunTaskAsync(task, taskCancellationTokenSource);
    }

    private async Task RunTaskAsync(WatchTask task, CancellationTokenSource cancellationTokenSource)
    {
        var cancellationToken = cancellationTokenSource.Token;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _taskRunner.RunAsync(task, cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error monitoring task [{TaskId}].", task.Id);
                }

                await Task.Delay(task.Interval, cancellationToken);
            }
        }
        finally
        {
            _runningTasks.Remove(task.Id);

            cancellationTokenSource.Dispose();

            Logger.Info("Monitoring task [{TaskId}] stopped.", task.Id);
        }
    }

    public void StopTask(Guid taskId)
    {
        if (!_runningTasks.TryGetValue(taskId, out var cancellationTokenSource))
        {
            return;
        }

        Logger.Info("Stopping monitoring task [{TaskId}].", taskId);

        cancellationTokenSource.Cancel();
    }

    private async Task StopAllTasksAsync()
    {
        foreach (var cancellationTokenSource in _runningTasks.Values)
        {
            cancellationTokenSource.Cancel();
        }

        var runningTasks = _runningTasks.Values
            .ToArray();

        foreach (var cancellationTokenSource in runningTasks)
        {
            cancellationTokenSource.Dispose();
        }

        _runningTasks.Clear();

        await Task.CompletedTask;
    }
}