using System;
using System.Collections.Generic;
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
                    using (var pipeClient = new NamedPipeClientStream(".", "PCStatsDataPipe", PipeDirection.In, PipeOptions.Asynchronous))
                    {
                        await pipeClient.ConnectAsync(token);

                        byte[] buffer = new byte[8192];
                        while (pipeClient.IsConnected && !token.IsCancellationRequested)
                        {
                            int bytesRead = await pipeClient.ReadAsync(buffer, 0, buffer.Length, token);
                            if (bytesRead > 0)
                            {
                                string json = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim('\0');
                                var data = JsonSerializer.Deserialize<List<SensorData>>(json);

                                if (data != null)
                                {
                                    _hook.UpdateData(data);
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