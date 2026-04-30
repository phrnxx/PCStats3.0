using LibreHardwareMonitor.Hardware;
using System.Collections.Generic;

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
                IsMemoryEnabled = false,
                IsMotherboardEnabled = false, // КРИТИЧЕСКИЙ ФИКС: Выключаем причину зависаний
                IsControllerEnabled = false,
                IsStorageEnabled = true
            };
            try { _computer.Open(); } catch { }
        }

        public Dictionary<string, string> GetFormattedStats()
        {
            var stats = new Dictionary<string, string>();
            stats["[PCStats] Engine"] = "Online"; // Маячок. Должен быть виден всегда.

            foreach (IHardware hardware in _computer.Hardware)
            {
                // КРИТИЧЕСКИЙ ФИКС: Если датчик сбоит, пропускаем его, а не убиваем всю программу
                try { hardware.Update(); } catch { continue; }

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (!sensor.Value.HasValue) continue;

                    string sName = sensor.Name;
                    if (sensor.SensorType == SensorType.Temperature ||
                        sensor.SensorType == SensorType.Load ||
                        sensor.SensorType == SensorType.Clock ||
                        sensor.SensorType == SensorType.Fan ||
                        sensor.SensorType == SensorType.Data)
                    {
                        string val = sensor.Value.Value.ToString("0.0");
                        if (sensor.SensorType == SensorType.Temperature) val += " °C";
                        else if (sensor.SensorType == SensorType.Load) val += " %";
                        else if (sensor.SensorType == SensorType.Clock) val += " MHz";
                        else if (sensor.SensorType == SensorType.Fan) val += " RPM";
                        else if (sensor.SensorType == SensorType.Data) val += " GB";

                        string cleanHwName = hardware.Name.Replace("NVIDIA GeForce ", "").Replace("AMD Radeon ", "");
                        stats[$"[{cleanHwName}] {sName}"] = val;
                    }
                }
            }
            return stats;
        }

        public void Close()
        {
            try { _computer.Close(); } catch { }
        }
    }
}