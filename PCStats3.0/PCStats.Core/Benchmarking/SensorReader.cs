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
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true
            };
            _computer.Open();
        }

        public Dictionary<string, string> GetFormattedStats()
        {
            var stats = new Dictionary<string, string>();
            foreach (IHardware hardware in _computer.Hardware)
            {
                hardware.Update();
                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (!sensor.Value.HasValue) continue;

                    string sName = sensor.Name;
                    bool isImportant = false;

                    // Универсальные фильтры
                    if (sensor.SensorType == SensorType.Temperature) isImportant = true;
                    if (sensor.SensorType == SensorType.Load && (sName.Contains("Total") || sName.Contains("Core"))) isImportant = true;
                    if (sensor.SensorType == SensorType.Clock && sName.Contains("Core #1")) isImportant = true;
                    if (sensor.SensorType == SensorType.Data && (sName.Contains("Used") || sName.Contains("Available"))) isImportant = true;
                    if (sensor.SensorType == SensorType.Fan) isImportant = true;

                    if (isImportant)
                    {
                        string val = sensor.Value.Value.ToString("0.0");
                        if (sensor.SensorType == SensorType.Temperature) val += " °C";
                        else if (sensor.SensorType == SensorType.Load) val += " %";
                        else if (sensor.SensorType == SensorType.Clock) val += " MHz";
                        else if (sensor.SensorType == SensorType.Data) val += " GB";
                        else if (sensor.SensorType == SensorType.Fan) val += " RPM";

                        stats[$"[{hardware.Name}] {sName}"] = val;
                    }
                }
            }
            return stats;
        }

        public void Close() => _computer.Close();
    }
}