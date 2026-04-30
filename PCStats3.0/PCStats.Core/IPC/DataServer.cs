using PCStats.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
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
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var pipeServer = new NamedPipeServerStream(
                        PipeName, PipeDirection.Out,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    await pipeServer.WaitForConnectionAsync(token);
                    lock (_connectedClients) { _connectedClients.Add(pipeServer); }
                }
                catch (OperationCanceledException) { break; }
                catch { }
            }
        }

        public async Task BroadcastDataAsync(List<SensorData> data, CancellationToken token)
        {
            // Формируем пуленепробиваемый текстовый пакет вместо JSON
            StringBuilder sb = new StringBuilder();
            foreach (var s in data)
            {
                sb.Append($"{s.HardwareName}\t{s.SensorName}\t{s.Value}|");
            }
            string payload = sb.ToString() + "\n";
            byte[] buffer = Encoding.UTF8.GetBytes(payload);

            NamedPipeServerStream[] clients;
            lock (_connectedClients) { clients = _connectedClients.ToArray(); }

            List<NamedPipeServerStream> deadClients = new List<NamedPipeServerStream>();

            foreach (var client in clients)
            {
                try
                {
                    if (client.IsConnected)
                    {
                        await client.WriteAsync(buffer, 0, buffer.Length, token);
                        await client.FlushAsync(token);
                    }
                    else { deadClients.Add(client); }
                }
                catch { deadClients.Add(client); }
            }

            lock (_connectedClients)
            {
                foreach (var dead in deadClients)
                {
                    _connectedClients.Remove(dead);
                    dead.Dispose();
                }
            }
        }
    }
}