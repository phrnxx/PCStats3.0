using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading.Tasks;
using PCStats.Shared.Models;

namespace PCStats.UI.IPC
{
    public class ClientIPC
    {
        public event EventHandler<List<SensorData>> DataReceived;
        public event EventHandler<bool> ConnectionChanged;
        private bool _isRunning;

        public void Start()
        {
            _isRunning = true;
            Task.Run(ListenAsync);
        }

        public void Stop() => _isRunning = false;

        private async Task ListenAsync()
        {
            while (_isRunning)
            {
                try
                {
                    // Подключаемся к "трубе", которую создало Ядро
                    using (var client = new NamedPipeClientStream(".", "PCStatsPipe", PipeDirection.In))
                    {
                        ConnectionChanged?.Invoke(this, false);
                        await client.ConnectAsync(5000);
                        ConnectionChanged?.Invoke(this, true);

                        using (var reader = new StreamReader(client))
                        {
                            while (_isRunning && client.IsConnected)
                            {
                                var json = await reader.ReadLineAsync();
                                if (!string.IsNullOrEmpty(json))
                                {
                                    // Десериализуем данные в наш список моделей
                                    var data = JsonSerializer.Deserialize<List<SensorData>>(json);
                                    DataReceived?.Invoke(this, data);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Если сервер не найден, ждем 2 секунды перед повтором
                    await Task.Delay(2000);
                }
            }
        }
    }
}