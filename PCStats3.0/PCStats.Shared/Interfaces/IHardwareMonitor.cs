using PCStats.Shared.Models;
using PCStats.Shared.Models;
using System.Collections.Generic;

namespace PCStats.Shared.Interfaces
{
    public interface IHardwareMonitor
    {
        void Start();
        void Stop();
        List<SensorData> GetCurrentData();
    }
}