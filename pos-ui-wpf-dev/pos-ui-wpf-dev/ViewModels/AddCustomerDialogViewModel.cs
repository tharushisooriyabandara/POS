using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using POS_UI.Models;
using MaterialDesignThemes.Wpf;

namespace POS_UI.ViewModels
{
    public class AddCustomerDialogViewModel : INotifyPropertyChanged
    {
        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            set { _customerName = value; OnPropertyChanged(); }
        }
        private string _customerPhone;
        public string CustomerPhone
        {
            get => _customerPhone;
            set { _customerPhone = value; OnPropertyChanged(); }
        }
        public ICommand ProceedCommand { get; }
        public ICommand SkipCommand { get; }
        public ICommand CloseCommand { get; }

        public AddCustomerDialogViewModel()
        {
            ProceedCommand = new RelayCommand(Proceed, CanProceed);
            SkipCommand = new RelayCommand(Skip);
            CloseCommand = new RelayCommand(Skip);
        }
        private void Proceed()
        {
            var customer = new CustomerModel { Name = CustomerName, Phone = CustomerPhone };
            DialogHost.CloseDialogCommand.Execute(customer, null);
        }
        private bool CanProceed()
        {
            return !string.IsNullOrWhiteSpace(CustomerName) && !string.IsNullOrWhiteSpace(CustomerPhone);
        }
        private void Skip()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
} 