using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using POS_UI.Models;

namespace POS_UI.ViewModels
{
    public class PrinterSettingsDialogViewModel : INotifyPropertyChanged
    {
        private StatusOption _selectedStatus;
        private PrinterModel _printer;

        public PrinterModel Printer
        {
            get => _printer;
            set
            {
                _printer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DeviceName));
                OnPropertyChanged(nameof(ConnectedVia));
            }
        }

        public string DeviceName => Printer?.DeviceName;
        public string ConnectedVia => Printer?.ConnectedVia;

        private ObservableCollection<StatusOption> _statusOptions;
        public ObservableCollection<StatusOption> StatusOptions
        {
            get => _statusOptions;
            set
            {
                _statusOptions = value;
                OnPropertyChanged();
            }
        }

        public StatusOption SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();
            }
        }

        public PrinterSettingsDialogViewModel(PrinterModel printer)
        {
            Printer = printer;
            
            StatusOptions = new ObservableCollection<StatusOption>
            {
                new StatusOption { Name = "Active", Color = "#00C853" },
                new StatusOption { Name = "Inactive", Color = "#FF5252" }
            };

            // Set the current status as selected
            SelectedStatus = Printer.IsActive ? StatusOptions[0] : StatusOptions[1];
            
            // Trigger property change notifications
            OnPropertyChanged(nameof(StatusOptions));
            OnPropertyChanged(nameof(SelectedStatus));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
} 