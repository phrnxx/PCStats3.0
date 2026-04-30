using PCStats.Core.Benchmarking;
using PCStats.Core.Hardware;
using PCStats.Core.IPC;
using PCStats.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PCStats.Core
{
    public class CoreService
    {
        private readonly SensorReader _sensorReader;
        private readonly CpuStresser _cpuStresser;
        private readonly DataServer _dataServer;
        private CancellationTokenSource _cts;

        public CoreService()
        {
            _sensorReader = new SensorReader();
            _cpuStresser = new CpuStresser();
            _dataServer = new DataServer();
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();

            Task.Run(() => _dataServer.StartAsync(_cts.Token));

            Task.Run(() => MonitoringLoop(_cts.Token));

            Console.WriteLine("Ядро PCStats активировано. Духи машины пробудились.");
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cpuStresser.Stop();
            _sensorReader.Close();

            Console.WriteLine("Ядро остановлено.");
        }

        public void StartBenchmark() => _cpuStresser.Start();
        public void StopBenchmark() => _cpuStresser.Stop();

        private async Task MonitoringLoop(CancellationToken token)
        {
            bool debugPrinted = false;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var rawData = _sensorReader.GetFormattedStats();

                    // ВРЕМЕННАЯ ДИАГНОСТИКА — печатаем один раз при старте
                    if (!debugPrinted)
                    {
                        Console.WriteLine("=== ВСЕ ДАТЧИКИ ===");
                        foreach (var item in rawData)
                            Console.WriteLine($"{item.Key} = {item.Value}");
                        Console.WriteLine("===================");
                        debugPrinted = true;
                    }

                    var sensorList = new List<SensorData>();

                    foreach (var item in rawData)
                    {
                        var parts = item.Key.Split(new[] { ']' }, 2);
                        string hName = parts[0].TrimStart('[');
                        string sName = parts.Length > 1 ? parts[1].Trim() : item.Key;

                        sensorList.Add(new SensorData
                        {
                            HardwareName = hName,
                            SensorName = sName,
                            Value = item.Value
                        });
                    }

                    await _dataServer.BroadcastDataAsync(sensorList, token);
                    await Task.Delay(1000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Сбой в цикле мониторинга: {ex.Message}");
                }
            }
        }
    }
}