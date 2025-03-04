using System;
using UnityEngine;

namespace UnityDevKit.Utils.TimeHandlers
{
    public class Clock : IClock
    {
        private DateTime startTime;
        private float lastStartTime;
        private float trackedTime;

        private bool isLaunched;
        private bool isWorking;

        public struct Data
        {
            public DateTime StartTime;
            public DateTime EndTime;
            public float Duration;
        }

        public void Launch()
        {
            isLaunched = true;
            isWorking = true;
            trackedTime = 0;
            startTime = DateTime.Now;
            lastStartTime = Time.time;
        }

        public Data Stop()
        {
            if (!isLaunched) throw new Exception("Clock hasn't been launched");
            isLaunched = false;
            isWorking = false;
            return new Data
            {
                StartTime = startTime,
                EndTime = DateTime.Now,
                Duration = trackedTime + (Time.time - lastStartTime)
            };
        }

        public void Pause()
        {
            if (!isWorking) return;
            isWorking = false;
            trackedTime += Time.time - lastStartTime;
        }

        public void Resume()
        {
            if (isWorking) return;
            isWorking = true;
            lastStartTime = Time.time;
        }
    }
}