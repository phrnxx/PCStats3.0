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
                IsMemoryEnabled = false, // Не трогаем, чтобы не крашило RAMSPDToolkit
                IsMotherboardEnabled = true,
                IsControllerEnabled = true
            };
            try { _computer.Open(); } catch { }
        }

        public Dictionary<string, string> GetFormattedStats()
        {
            var stats = new Dictionary<string, string>();

            // МАЯЧОК: Всегда показывает, что ядро работает
            stats["[PCStats] Engine"] = "Online";

            if (_computer.Hardware.Count == 0)
            {
                stats["[Система] Ошибка"] = "Нет доступа к железу (Нужен Админ)";
                return stats;
            }

            // Рекурсивный поиск (достает датчики с материнской платы)
            void Traverse(IHardware hw)
            {
                hw.Update();
                foreach (var sensor in hw.Sensors)
                {
                    if (!sensor.Value.HasValue) continue;
                    string sName = sensor.Name;
                    bool isImportant = false;

                    if (sensor.SensorType == SensorType.Temperature ||
                        sensor.SensorType == SensorType.Load ||
                        sensor.SensorType == SensorType.Clock ||
                        sensor.SensorType == SensorType.Fan ||
                        sensor.SensorType == SensorType.Power)
                    {
                        isImportant = true;
                    }

                    // Отсекаем мусорные датчики
                    if (sName.Contains("Thread") || sName.Contains("Warning") || sName.Contains("Critical"))
                        isImportant = false;

                    if (isImportant)
                    {
                        string val = sensor.Value.Value.ToString("0.0");
                        if (sensor.SensorType == SensorType.Temperature) val += " °C";
                        else if (sensor.SensorType == SensorType.Load) val += " %";
                        else if (sensor.SensorType == SensorType.Clock) val += " MHz";
                        else if (sensor.SensorType == SensorType.Fan) val += " RPM";
                        else if (sensor.SensorType == SensorType.Power) val += " W";

                        string cleanHwName = hw.Name;
                        // Универсальная чистка для любых ПК (у друга тоже будет красиво)
                        if (cleanHwName.Contains("NVIDIA GeForce ")) cleanHwName = cleanHwName.Replace("NVIDIA GeForce ", "");
                        if (cleanHwName.Contains("AMD Radeon ")) cleanHwName = cleanHwName.Replace("AMD Radeon ", "");

                        string key = $"[{cleanHwName}] {sName}";

                        int count = 1;
                        while (stats.ContainsKey(key))
                        {
                            count++;
                            key = $"[{cleanHwName}] {sName} #{count}";
                        }
                        stats[key] = val;
                    }
                }
                foreach (var sub in hw.SubHardware) Traverse(sub);
            }

            foreach (var hw in _computer.Hardware) Traverse(hw);
            return stats;
        }

        public void Close()
        {
            try { _computer.Close(); } catch { }
        }
    }
}