using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PCStats.Shared.Models
{
    public class SensorData : INotifyPropertyChanged
    {
        private string _value;
        public string HardwareName { get; set; }
        public string SensorName { get; set; }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}