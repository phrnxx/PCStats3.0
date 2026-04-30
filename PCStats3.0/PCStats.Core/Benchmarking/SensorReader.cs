using LibreHardwareMonitor.Hardware;
using System.Collections.Generic;
using System.Linq;

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

                string hName = hardware.Name;

                if (hName.Contains("Virtual Memory")) continue;

                if (hName.Contains("Ryzen")) hName = "Ryzen" + hName.Substring(hName.IndexOf("Ryzen") + 5);
                else if (hName.Contains("GeForce RTX")) hName = hName.Substring(hName.IndexOf("RTX"));
                else if (hName.Contains("Radeon")) hName = hName.Substring(hName.IndexOf("Radeon"));
                else if (hName.Contains("Memory")) hName = "RAM";
                else if (hName.Length > 22) hName = hName.Substring(0, 22) + "...";

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (!sensor.Value.HasValue) continue;

                    string sName = sensor.Name;

                    if (sName.Contains("Thread") ||
                        sName.Contains("Effective") ||
                        sName.Contains("Port") ||
                        sName.Contains("PCIe") ||
                        sName.Contains("Warning") ||
                        sName.Contains("Critical") ||
                        sName.Contains("Total Activity"))
                        continue;

                    bool isImportant = false;

                    if (sensor.SensorType == SensorType.Temperature || sensor.SensorType == SensorType.Fan)
                        isImportant = true;

                    if (sensor.SensorType == SensorType.Load && (sName.Contains("CPU Total") || sName == "Core"))
                        isImportant = true;

                    if (sensor.SensorType == SensorType.Clock && sName.Contains("Core"))
                        isImportant = true;

                    if (sensor.SensorType == SensorType.SmallData && sName.Contains("Memory Used"))
                        isImportant = true;

                    if (sensor.SensorType == SensorType.Data && (sName.Contains("Memory Used") || sName.Contains("Memory Available")))
                        isImportant = true;

                    if (sensor.SensorType == SensorType.Throughput)
                        isImportant = true;

                    if (isImportant)
                    {
                        string val = "";

                        if (sensor.SensorType == SensorType.Throughput)
                        {
                            val = (sensor.Value.Value / (1024f * 1024f)).ToString("0.0") + " MB/s";
                        }
                        else
                        {
                            val = sensor.Value.Value.ToString("0.0");
                            if (sensor.SensorType == SensorType.Temperature) val += " °C";
                            else if (sensor.SensorType == SensorType.Load) val += " %";
                            else if (sensor.SensorType == SensorType.Clock) val += " MHz";
                            else if (sensor.SensorType == SensorType.SmallData) val += " MB";
                            else if (sensor.SensorType == SensorType.Data) val += " GB";
                            else if (sensor.SensorType == SensorType.Power) val += " W";
                            else if (sensor.SensorType == SensorType.Fan) val += " RPM";
                        }

                        string baseKey = $"[{hName}] {sName}";
                        string finalKey = baseKey;
                        int counter = 1;

                        while (stats.ContainsKey(finalKey))
                        {
                            counter++;
                            finalKey = $"{baseKey} #{counter}";
                        }

                        stats[finalKey] = val;
                    }
                }
            }
            return stats;
        }

        public void Close() => _computer.Close();
    }
}