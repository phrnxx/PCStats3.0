using LibreHardwareMonitor.Hardware;
using System.Collections.Generic;

namespace PCStats.Core.Hardware
{
    // Visitor который форсирует Update на всём дереве железа включая субхардвар
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer) => computer.Traverse(this);
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware sub in hardware.SubHardware)
                sub.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }

    public class SensorReader
    {
        private Computer _computer;
        private readonly UpdateVisitor _visitor = new UpdateVisitor();

        public SensorReader()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsStorageEnabled = true
            };

            _computer.Open();
            _computer.Accept(_visitor);
        }

        public Dictionary<string, string> GetFormattedStats()
        {
            var stats = new Dictionary<string, string>();

            _computer.Accept(_visitor);

            foreach (IHardware hw in _computer.Hardware)
            {
                string hwName = hw.Name
                    .Replace("NVIDIA GeForce ", "")
                    .Replace("AMD Radeon ", "");
                if (hw.HardwareType == HardwareType.Memory) hwName = "RAM";

                ProcessSensors(hw, hwName, stats);

                foreach (IHardware sub in hw.SubHardware)
                    ProcessSensors(sub, hwName, stats);
            }

            // Если температура CPU = 0 — убираем её совсем, не показываем фейк
            string cpuTempKey = null;
            foreach (var key in stats.Keys)
                if (key.Contains("CPU Temp")) { cpuTempKey = key; break; }

            if (cpuTempKey != null && stats[cpuTempKey] == "0.0 °C")
                stats.Remove(cpuTempKey);

            return stats;
        }

        private void ProcessSensors(IHardware hw, string hwName, Dictionary<string, string> stats)
        {
            foreach (ISensor sensor in hw.Sensors)
            {
                if (!sensor.Value.HasValue) continue;

                string sName = sensor.Name;
                bool isImportant = false;
                string val = sensor.Value.Value.ToString("0.0");

                if (sensor.SensorType == SensorType.Temperature)
                { val += " °C"; isImportant = true; }

                if (sensor.SensorType == SensorType.Load &&
                   (sName.Contains("Total") || sName.Contains("Core") ||
                    sName.Contains("Memory") || sName.Contains("Activity")))
                { val += " %"; isImportant = true; }

                if (sensor.SensorType == SensorType.Data &&
                   (sName.Contains("Used") || sName.Contains("Available")))
                { val += " GB"; isImportant = true; }

                if (sensor.SensorType == SensorType.SmallData)
                { val += " MB"; isImportant = true; }

                if (sensor.SensorType == SensorType.Fan)
                { val += " RPM"; isImportant = true; }

                // Частота CPU
                if (sensor.SensorType == SensorType.Clock &&
                    hw.HardwareType == HardwareType.Cpu &&
                    sName == "Core #1")
                {
                    val = (sensor.Value.Value / 1000f).ToString("0.00") + " GHz";
                    sName = "CPU Frequency";
                    isImportant = true;
                }

                // Фильтры мусора
                if (sName.Contains("Thread") || sName.Contains("Core #") || sName.Contains("Distance"))
                    isImportant = false;

                if (sName.Contains("Warning") || sName.Contains("Critical"))
                    isImportant = false;

                if (hw.HardwareType == HardwareType.Cpu && sensor.SensorType == SensorType.Temperature)
                {
                    if (!sName.Contains("Tdie") && !sName.Contains("Tctl") &&
                        !sName.Contains("Package") && sName != "Core Average")
                        isImportant = false;
                }

                if (hw.HardwareType == HardwareType.Memory && sName.Contains("Virtual"))
                    isImportant = false;

                if ((hw.HardwareType == HardwareType.GpuNvidia || hw.HardwareType == HardwareType.GpuAmd) &&
                    sensor.SensorType == SensorType.Load && sName.Contains("Memory"))
                    isImportant = false;

                if (!isImportant) continue;

                // Красивые имена
                if (sName == "CPU Total") sName = "CPU Core";
                if (sName == "Memory Used" && hw.HardwareType == HardwareType.Memory) sName = "RAM Used";
                if (sName == "Memory Available" && hw.HardwareType == HardwareType.Memory) sName = "RAM Available";
                if (sName == "Memory" && hw.HardwareType == HardwareType.Memory) sName = "RAM Load";
                if (sName == "GPU Memory") sName = "GPU Memory Used";
                if (sName == "D3D Dedicated Memory Used") sName = "VRAM Dedicated";
                if (sName == "D3D Shared Memory Used") sName = "VRAM Shared";
                if (sName == "Core (Tctl/Tdie)") sName = "CPU Temp";
                if (sName == "GPU Hot Spot") sName = "GPU Hot Spot";
                if (sName == "Composite Temperature") sName = "Temperature";
                if (sName == "GPU Core" && sensor.SensorType == SensorType.Temperature) sName = "GPU Temp";

                string key = $"[{hwName}] {sName}";

                if (!stats.ContainsKey(key))
                    stats[key] = val;
            }
        }

        public void Close()
        {
            try { _computer.Close(); } catch { }
        }
    }
}