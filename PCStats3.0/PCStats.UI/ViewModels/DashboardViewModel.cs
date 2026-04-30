using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using PCStats.UI.IPC;
using PCStats.Shared.Models;

namespace PCStats.UI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly ClientIPC _client;
        private bool _isConnected;
        private List<IGrouping<string, SensorData>> _groupedSensors;

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }

        public List<IGrouping<string, SensorData>> GroupedSensors
        {
            get => _groupedSensors;
            set { _groupedSensors = value; OnPropertyChanged(); }
        }

        public DashboardViewModel()
        {
            _client = new ClientIPC();
            _client.DataReceived += OnDataReceived;
            _client.ConnectionChanged += (s, connected) => IsConnected = connected;

            // Запуск клиента в фоновом потоке
            _client.Start();
        }

        private void OnDataReceived(object sender, List<SensorData> data)
        {
            // Магия группировки: превращаем плоский список в группы по HardwareName
            if (data != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GroupedSensors = data.GroupBy(s => s.HardwareName).ToList();
                });
            }
        }

        public void Stop() => _client.Stop();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}