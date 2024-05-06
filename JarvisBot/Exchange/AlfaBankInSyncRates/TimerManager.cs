using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{
    public class TimerManager
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private Dictionary<string, System.Timers.Timer> _timer = new();
        public string TimerId { get; set; }

        public double Interval { get; set; }

        public System.Timers.Timer Timer {  get; set; }


        public void CreateTimer(string timerId, double interval)
        {
            TimerId = timerId;
            Interval = interval;

            Timer = new System.Timers.Timer();

            Timer.Interval = TimeSpan.FromMinutes(interval).TotalMilliseconds;
            _logger.Trace($"New Timer started with {Timer.Interval} interval");

            Timer.AutoReset = true;
            Timer.Enabled = true;

            _timer.Add(timerId, Timer);
        }

        public void StartTimer(string timerId)
        {
            if (_timer.ContainsKey(timerId))
            {
                _timer[timerId].Start();
                _logger.Trace($"Timer - {TimerId} is started");
            }
        }

        public void StopTimer(string timerId)
        {
            if (_timer.ContainsKey(timerId))
            {
                _timer[timerId].Stop();
                _logger.Trace($"Timer - {TimerId} is stopped");
            }
        }

        public void DisposeTimer(string timerId)
        {
            if (_timer.ContainsKey(timerId))
            {
                _timer[timerId].Dispose();
                _logger.Trace($"Timer - {TimerId} is disposed");
            }
        }

    }
}
