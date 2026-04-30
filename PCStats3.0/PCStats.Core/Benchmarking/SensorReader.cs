using LibreHardwareMonitor.Hardware;
using System.Collections.Generic;
using System.Linq; // Добавили для удобной фильтрации

namespace PCStats.Core.Hardware
{
    public class SensorReader
    {
        private Computer _computer;

        public SensorReader()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true
            };
            _computer.Open();
        }

        public Dictionary<string, float?> GetAllStats()
        {
            var stats = new Dictionary<string, float?>();

            foreach (IHardware hardware in _computer.Hardware)
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.Value.HasValue)
                    {
                        // 1. Убираем "мусорные" датчики, которые забивают список
                        if (sensor.Name.Contains("Thread") || sensor.Name.Contains("Effective")) continue;

                        // 2. Оставляем только самое важное (Температуры, Общую нагрузку, Частоту ядер)
                        bool isImportant = sensor.SensorType == SensorType.Temperature ||
                                         (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Total")) ||
                                         (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Core #1"));

                        if (isImportant)
                        {
                            // Формируем короткое имя: "[CPU] Package" или "[GPU] Core"
                            string hardwareType = hardware.HardwareType.ToString().Replace("GpuNvidia", "GPU").Replace("Cpu", "CPU");
                            string key = $"[{hardwareType}] {sensor.Name}";

                            stats[key] = sensor.Value;
                        }
                    }
                }
            }
            return stats;
        }

        public void Close() => _computer.Close();
    }
}