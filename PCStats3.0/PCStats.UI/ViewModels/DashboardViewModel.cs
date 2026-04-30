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

        // Порядок секций: CPU → GPU → RAM → SSD/NVMe → HDD → Motherboard → остальное
        private int GetGroupOrder(string hardwareName)
        {
            if (hardwareName == null) return 99;

            string n = hardwareName.ToLowerInvariant();

            // CPU
            if (n.Contains("ryzen") || n.Contains("intel") || n.Contains("core i") ||
                n.Contains("cpu") || n.Contains("xeon") || n.Contains("threadripper"))
                return 0;

            // GPU
            if (n.Contains("rtx") || n.Contains("gtx") || n.Contains("radeon") ||
                n.Contains("rx ") || n.Contains("gpu") || n.Contains("arc "))
                return 1;

            // RAM
            if (n == "ram" || n.Contains("memory") || n.Contains("ddr"))
                return 2;

            // SSD / NVMe (Kingston, Samsung SSD, etc.)
            if (n.Contains("kingston") || n.Contains("samsung") || n.Contains("crucial") ||
                n.Contains("nvme") || n.Contains("ssd") || n.Contains("snv") ||
                n.Contains("970") || n.Contains("860") || n.Contains("870"))
                return 3;

            // HDD (Toshiba, Seagate, WD, Hitachi)
            if (n.Contains("toshiba") || n.Contains("seagate") || n.Contains("western") ||
                n.Contains("wd") || n.Contains("hitachi") || n.Contains("hgst") ||
                n.Contains("dt01") || n.Contains("barracuda"))
                return 4;

            // Motherboard
            if (n.Contains("gigabyte") || n.Contains("asus") || n.Contains("msi") ||
                n.Contains("asrock") || n.Contains("aorus") || n.Contains("motherboard") ||
                n.Contains("b550") || n.Contains("b450") || n.Contains("x570") ||
                n.Contains("z690") || n.Contains("b660"))
                return 5;

            return 99;
        }

        private void OnDataReceived(object sender, List<SensorData> data)
        {
            if (data == null) return;

            var app = Application.Current;
            if (app == null || app.Dispatcher.HasShutdownStarted) return;

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

                        // Вставляем группу в правильную позицию по порядку
                        int newOrder = GetGroupOrder(group.Key);
                        int insertIndex = 0;
                        for (int i = 0; i < GroupedSensors.Count; i++)
                        {
                            if (GetGroupOrder(GroupedSensors[i].Key) <= newOrder)
                                insertIndex = i + 1;
                        }
                        GroupedSensors.Insert(insertIndex, newG);
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
                            existingGroup.Sensors.Remove(deadSensor);
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