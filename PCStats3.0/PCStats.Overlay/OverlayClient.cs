using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
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
                                var json = await reader.ReadLineAsync();
                                if (!string.IsNullOrEmpty(json))
                                {
                                    var data = JsonSerializer.Deserialize<List<SensorData>>(json);
                                    if (data != null) _hook.UpdateData(data);
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