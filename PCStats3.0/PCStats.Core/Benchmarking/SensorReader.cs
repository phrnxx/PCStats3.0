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
                if (hName.Contains("Ryzen")) hName = "Ryzen" + hName.Substring(hName.IndexOf("Ryzen") + 5);
                else if (hName.Contains("GeForce RTX")) hName = hName.Substring(hName.IndexOf("RTX"));
                else if (hName.Contains("Radeon")) hName = hName.Substring(hName.IndexOf("Radeon"));

                else if (hName.Contains("Memory")) hName = "RAM";

                else if (hName.Length > 22) hName = hName.Substring(0, 22) + "...";

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (!sensor.Value.HasValue) continue;

                    string sName = sensor.Name;

                    if (sName.Contains("Thread") || sName.Contains("Effective") || sName.Contains("Port") || sName.Contains("PCIe")) continue;

                    bool isImportant = sensor.SensorType == SensorType.Temperature ||
                                     (sensor.SensorType == SensorType.Load && sName.Contains("Total")) ||
                                     (sensor.SensorType == SensorType.Load && sName == "Core") || 
                                     (sensor.SensorType == SensorType.Clock && sName.Contains("Core #1")) ||
                                     (sensor.SensorType == SensorType.SmallData && sName.Contains("Memory Used")) || 
                                     (sensor.SensorType == SensorType.Data && sName.Contains("Memory Used"));  

                    if (isImportant)
                    {
                        string val = sensor.Value.Value.ToString("0.0");
                        if (sensor.SensorType == SensorType.Temperature) val += " °C";
                        else if (sensor.SensorType == SensorType.Load) val += " %";
                        else if (sensor.SensorType == SensorType.Clock) val += " MHz";
                        else if (sensor.SensorType == SensorType.SmallData) val += " MB";
                        else if (sensor.SensorType == SensorType.Data) val += " GB";
                        else if (sensor.SensorType == SensorType.Power) val += " W";

                        stats[$"[{hName}] {sName}"] = val;
                    }
                }
            }
            return stats;
        }

        public void Close() => _computer.Close();
    }
}