using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using PCStats.Shared.Models;
using PCStats.Overlay.Hooks;

namespace PCStats.Overlay
{
    public class OverlayClient
    {
        private readonly DirectXHook _hook;
        private CancellationTokenSource _cts;

        public OverlayClient()
        {
            _hook = new DirectXHook();
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _hook.Start();
            Task.Run(() => ConnectAndListenAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _hook.Stop();
            _hook.Dispose();
        }

        private async Task ConnectAndListenAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (var client = new NamedPipeClientStream(".", "PCStatsDataPipe", PipeDirection.In, PipeOptions.Asynchronous))
                    {
                        await client.ConnectAsync(token);

                        using (var reader = new StreamReader(client))
                        {
                            while (client.IsConnected && !token.IsCancellationRequested)
                            {
                                var rawStr = await reader.ReadLineAsync();
                                if (!string.IsNullOrEmpty(rawStr))
                                {
                                    var data = new List<SensorData>();
                                    var records = rawStr.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                                    foreach (var rec in records)
                                    {
                                        var fields = rec.Split('\t');
                                        if (fields.Length == 3)
                                        {
                                            data.Add(new SensorData
                                            {
                                                HardwareName = fields[0],
                                                SensorName = fields[1],
                                                Value = fields[2]
                                            });
                                        }
                                    }
                                    if (data.Count > 0) _hook.UpdateData(data);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    await Task.Delay(2000, token);
                }
            }
        }
    }
}