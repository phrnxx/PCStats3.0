using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCStats.Core.Benchmarking
{
    public class CpuStresser
    {
        private CancellationTokenSource _cts;
        private Task[] _workerTasks;

        public bool IsRunning { get; private set; }

        public void Start()
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            IsRunning = true;

            int threadCount = Environment.ProcessorCount;
            _workerTasks = new Task[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                _workerTasks[i] = Task.Run(() => StressLoop(_cts.Token), _cts.Token);
            }
        }

        public void Stop()
        {
            if (!IsRunning) return;

            _cts?.Cancel();

            try
            {
                Task.WaitAll(_workerTasks);
            }
            catch (AggregateException)
            {
            }
            finally
            {
                _cts?.Dispose();
                IsRunning = false;
            }
        }

        private void StressLoop(CancellationToken token)
        {
            double result = 0;

            while (!token.IsCancellationRequested)
            {
                for (int i = 1; i < 100000; i++)
                {
                    result += Math.Sqrt(i) * Math.Sin(i);
                }
            }
        }
    }
}