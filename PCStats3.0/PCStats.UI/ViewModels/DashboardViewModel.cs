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
    public class HardwareGroup : INotifyPropertyChanged
    {
        public string Key { get; set; }
        public ObservableCollection<SensorData> Sensors { get; set; } = new ObservableCollection<SensorData>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly ClientIPC _client;
        private bool _isConnected;
        private int _bgIndex = 1;
        private string _currentBackground = "/PCStats.UI/Background/Background01.jpg";

        public ObservableCollection<HardwareGroup> GroupedSensors { get; } = new ObservableCollection<HardwareGroup>();

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }
        public string CurrentBackground
        {
            get => _currentBackground;
            set { _currentBackground = value; OnPropertyChanged(); }
        }

        public DashboardViewModel()
        {
            _client = new ClientIPC();
            _client.DataReceived += OnDataReceived;
            _client.ConnectionChanged += (s, connected) => IsConnected = connected;
            _client.Start();
        }

        public void NextBackground()
        {
            _bgIndex++;
            if (_bgIndex > 5) _bgIndex = 1;
            CurrentBackground = $"/PCStats.UI/Background/Background0{_bgIndex}.jpg";
        }

        private void OnDataReceived(object sender, List<SensorData> data)
        {
            if (data == null) return;

            var app = Application.Current;
            if (app == null || app.Dispatcher.HasShutdownStarted)
                return;

            app.Dispatcher.Invoke(() =>
            {
                var newGroups = data.GroupBy(s => s.HardwareName);

                foreach (var group in newGroups)
                {
                    var existingGroup = GroupedSensors.FirstOrDefault(g => g.Key == group.Key);

                    if (existingGroup == null)
                    {
                        var newG = new HardwareGroup { Key = group.Key };
                        foreach (var s in group) newG.Sensors.Add(s);
                        GroupedSensors.Add(newG);
                    }
                    else
                    {
                        foreach (var newSensor in group)
                        {
                            var existingSensor = existingGroup.Sensors
                                .FirstOrDefault(s => s.SensorName == newSensor.SensorName);

                            if (existingSensor != null)
                                existingSensor.Value = newSensor.Value;
                            else
                                existingGroup.Sensors.Add(newSensor);
                        }

                        var sensorsToRemove = existingGroup.Sensors
                            .Where(old => !group.Any(n => n.SensorName == old.SensorName))
                            .ToList();

                        foreach (var deadSensor in sensorsToRemove)
                        {
                            existingGroup.Sensors.Remove(deadSensor);
                        }
                    }
                }
            });
        }

        public void Stop() => _client.Stop();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}