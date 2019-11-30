using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace WebDAVClient.Helpers
{
    public class TransmissionHelper
    {
        public event Action<int, double, long> OnProgressHandler;
        private static readonly object StaticLockObj = new object();
        private Timer timer;
        private long totalLength = 0;
        private long realTimeLength = 0;
        private long lastlLength = 0;
        private int timeCost = 0;

        public TransmissionHelper()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
        }

        public void SetTotalLength(long totalLength)
        {
            this.totalLength = totalLength;
        }

        public void IncreaseRealTimeLength(long length)
        {
            lock (StaticLockObj)
            {
                realTimeLength += length;
            }
        }

        public void Start()
        {
            timer.Enabled = true;
        }

        public void Stop()
        {
            totalLength = 0;
            realTimeLength = 0;
            lastlLength = 0;
            timeCost = 0;
            timer.Enabled = false;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            double plannedSpeed = Math.Round(realTimeLength * 1.0 / totalLength, 2);

            long downloadSpeed = realTimeLength - lastlLength;
            lastlLength = realTimeLength;

            timeCost++;

            OnProgressHandler?.Invoke(timeCost, plannedSpeed, downloadSpeed);
        }
    }
}
