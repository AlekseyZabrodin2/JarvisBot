using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace JarvisBot
{
    public class JarvisEchoTimer
    {
        public static Timer _anyDeskTimer;



        public void SetTimer()
        {
            // Create a timer with a two second interval.
            _anyDeskTimer = new Timer(2000);
            // Hook up the Elapsed event for the timer. 
            _anyDeskTimer.Elapsed += OnTimedEvent;
            _anyDeskTimer.AutoReset = true;
            _anyDeskTimer.Enabled = true;
        }

        public void StopTimer()
        {
            _anyDeskTimer.Stop();
            _anyDeskTimer.Dispose();
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("There is no activity, echo mode is enabled - {0:HH:mm:ss.fff}", e.SignalTime);
        }


    }
}
