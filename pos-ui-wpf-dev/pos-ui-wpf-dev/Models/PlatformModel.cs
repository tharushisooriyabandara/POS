using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_UI.Models
{
    public class PlatformModel : INotifyPropertyChanged
    {
        private bool _isActive;

        public string PlatformName { get; set; }
        public string Branch { get; set; }
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }
        public string Status => IsActive ? "Active" : "Inactive";
        public string StatusColor => IsActive ? "#00C853" : "#FF5252";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
} 