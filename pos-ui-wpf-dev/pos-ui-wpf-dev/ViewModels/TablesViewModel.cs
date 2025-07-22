using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using POS_UI.Models;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace POS_UI.ViewModels
{
    public class TablesViewModel : INotifyPropertyChanged
    {
        public string CurrentPage { get; set; }
        public ObservableCollection<TableModel> Tables { get; set; }
        public ICommand TableButtonClickCommand { get; }

        public TablesViewModel()
        {
            CurrentPage = "Tables";
            Tables = new ObservableCollection<TableModel>
            {
                new TableModel { TableNumber = 1, Status = TableStatus.Served, Amount = 15.98m },
                new TableModel { TableNumber = 2, Status = TableStatus.Available },
                new TableModel { TableNumber = 3, Status = TableStatus.Drafted },
                new TableModel { TableNumber = 4, Status = TableStatus.Reserved },
                new TableModel { TableNumber = 5, Status = TableStatus.Available },
                new TableModel { TableNumber = 6, Status = TableStatus.Available },
                new TableModel { TableNumber = 7, Status = TableStatus.Served, Amount = 15.98m },
                new TableModel { TableNumber = 8, Status = TableStatus.Available },
                new TableModel { TableNumber = 9, Status = TableStatus.Available },
                new TableModel { TableNumber = 10, Status = TableStatus.Available }
            };
            TableButtonClickCommand = new RelayCommand<TableModel>(OnTableButtonClicked);
        }

        private async void OnTableButtonClicked(TableModel table)
        {
            try
            {
                if (table.Status == TableStatus.Served)
                {
                    // Here you would load the actual OrderModel for the table
                    // For demonstration, create a sample order
                    var order = new POS_UI.Models.OrderModel
                    {
                        OrderNumber = $"{table.TableNumber}",
                        CustomerName = "Customer Name",
                        OrderType = POS_UI.Models.OrderType.DineIn,
                        TableNumber = table.TableNumber,
                        Items = new System.Collections.Generic.List<POS_UI.Models.OrderItem>
                        {
                            new POS_UI.Models.OrderItem { Name = "Chicken Pasta", Quantity = 2, Price = 10.93m, Product = new POS_UI.Models.ProductItemModel { Size = "Large" , Price=10.93m} },
                            new POS_UI.Models.OrderItem { Name = "Chocolate Donut", Quantity = 1, Price = 15.00m, Product = new POS_UI.Models.ProductItemModel { Size = "Small", Price = 15.00m } },
                            new POS_UI.Models.OrderItem { Name = "Spring Rolls", Quantity = 1, Price = 14.00m, Product = new POS_UI.Models.ProductItemModel { Size = "Small", Price = 14.00m } }
                        },
                        DiscountAmount = 10.3m,
                        DiscountPercentage = 10,
                        CouponCode = "SUMMER10",
                        CouponAmount = 5.3m
                    };
                    var dialog = new POS_UI.View.TableOrderDetails { DataContext = order };
                    dialog.UpdateOrderRequested += (orderModel) =>
                    {
                        MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
                        var mainWindow = System.Windows.Application.Current.MainWindow as POS_UI.MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.NavigateToCashierWithOrder(orderModel);
                        }
                    };
                    await DialogHost.Show(dialog, "RootDialogHost");
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Error showing order details: {ex.Message}\n{ex.StackTrace}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
} 