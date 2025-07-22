using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using POS_UI.Models;
using MaterialDesignThemes.Wpf;
using System;

namespace POS_UI.ViewModels
{
    public class SelectCustomerDialogViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CustomerModel> AllCustomers { get; set; }
        public ObservableCollection<CustomerModel> FilteredCustomers { get; set; }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterCustomers();
            }
        }
        private CustomerModel _selectedCustomer;
        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }
        public ICommand ProceedCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand AddNewCustomerCommand { get; }

        public SelectCustomerDialogViewModel(ObservableCollection<CustomerModel> customers, CustomerModel selected)
        {
            AllCustomers = customers;
            FilteredCustomers = new ObservableCollection<CustomerModel>(AllCustomers);
            SelectedCustomer = selected;
            ProceedCommand = new RelayCommand(Proceed);
            CloseCommand = new RelayCommand(Close);
            AddNewCustomerCommand = new RelayCommand(async () => await AddNewCustomer());
        }
        private void FilterCustomers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCustomers = new ObservableCollection<CustomerModel>(AllCustomers);
            }
            else
            {
                var lower = SearchText.ToLower();
                FilteredCustomers = new ObservableCollection<CustomerModel>(AllCustomers.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(lower)) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(lower))));
            }
            OnPropertyChanged(nameof(FilteredCustomers));
        }
        private void Proceed()
        {
            DialogHost.CloseDialogCommand.Execute(SelectedCustomer, null);
        }
        private void Close()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
        private async System.Threading.Tasks.Task AddNewCustomer()
        {
            try
            {
                var dialogVm = new AddCustomerDialogViewModel();
                var dialog = new POS_UI.View.AddCustomerDialog { DataContext = dialogVm };
                MaterialDesignThemes.Wpf.DialogHost.OpenDialogCommand.Execute(dialog, null);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("dialog_error.log", $"[{DateTime.Now}] Error opening AddCustomerDialog: {ex}\n");
                System.Windows.MessageBox.Show($"Error opening Add Customer dialog: {ex.Message}", "Dialog Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    } 
}