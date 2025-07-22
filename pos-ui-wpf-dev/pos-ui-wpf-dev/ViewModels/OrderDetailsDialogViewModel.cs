using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;


namespace POS_UI.ViewModels
{
    public class OrderDetailsDialogViewModel : INotifyPropertyChanged
    {
        public string OrderNumber { get; set; }
        public string OrderTypeTime { get; set; }
        public string Contact { get; set; }
        public ObservableCollection<OrderItemViewModel> Items { get; set; } = new ObservableCollection<OrderItemViewModel>();
        public string OrderNotes { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public ICommand CloseCommand { get; set; }
        public event Action DialogClosed;

        public OrderDetailsDialogViewModel()
        {
            CloseCommand = new RelayCommand(() => { DialogClosed?.Invoke(); });
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static OrderDetailsDialogViewModel CreateSample()
        {
            var vm = new OrderDetailsDialogViewModel
            {
                OrderNumber = "APC356",
                OrderTypeTime = "Collection - Pickup at : 03:00 pm",
                Contact = "+94767886124",
                OrderNotes = "Make it less spicy",
                Subtotal = 3.00m,
                Total = 3.24m
            };
            vm.Items.Add(new OrderItemViewModel
            {
                Quantity = 1,
                Name = "Chicken Cheese Kottu",
                Size = "Large",
                Notes = "Less Spicy and send some tissues!",
                Price = 6.00m
            });
            vm.Items.Add(new OrderItemViewModel
            {
                Quantity = 2,
                Name = "Chicken Submarine",
                Size = "Small",
                Notes = "Less Spicy and send some tissues!",
                Price = 3.00m
            });
            return vm;
        }
    }


    public class OrderItemViewModel : INotifyPropertyChanged
    {
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string Notes { get; set; }
        public decimal Price { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 