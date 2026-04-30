using PCStats.Shared.Models;
using PCStats.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PCStats.Core.IPC
{
    public class DataServer
    {
        private const string PipeName = "PCStatsDataPipe";
        private readonly List<NamedPipeServerStream> _connectedClients = new List<NamedPipeServerStream>();

        public async Task StartAsync(CancellationToken token)
        {
            Console.WriteLine("Сервер IPC запущен. Ожидание адептов (клиентов)...");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var pipeServer = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.Out,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    await pipeServer.WaitForConnectionAsync(token);

                    lock (_connectedClients)
                    {
                        _connectedClients.Add(pipeServer);
                    }

                    Console.WriteLine("Клиент подключен к информационному потоку.");
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка узла связи: {ex.Message}");
                }
            }
        }

        public async Task BroadcastDataAsync(List<SensorData> data, CancellationToken token)
        {
            string json = JsonSerializer.Serialize(data) + Environment.NewLine;
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            List<NamedPipeServerStream> deadClients = new List<NamedPipeServerStream>();

            lock (_connectedClients)
            {
                foreach (var client in _connectedClients)
                {
                    try
                    {
                        if (client.IsConnected)
                        {
                            client.Write(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            deadClients.Add(client);
                        }
                    }
                    catch
                    {
                        deadClients.Add(client);
                    }
                }

                foreach (var dead in deadClients)
                {
                    _connectedClients.Remove(dead);
                    dead.Dispose();
                }
            }
        }
    }
}