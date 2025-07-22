using POS_UI.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace POS_UI.ViewModels
{
    public class KitchenViewModel : BaseViewModel
    {
        // Backing collections for all orders
        private List<OrderModel> _allQueueOrders;
        private List<OrderModel> _allPreparingOrders;
        private List<OrderModel> _allServedOrders;

        private ObservableCollection<OrderModel> _queueOrders;
        public ObservableCollection<OrderModel> QueueOrders
        {
            get => _queueOrders;
            set { _queueOrders = value; OnPropertyChanged(nameof(QueueOrders)); }
        }
        private ObservableCollection<OrderModel> _preparingOrders;
        public ObservableCollection<OrderModel> PreparingOrders
        {
            get => _preparingOrders;
            set { _preparingOrders = value; OnPropertyChanged(nameof(PreparingOrders)); }
        }
        private ObservableCollection<OrderModel> _servedOrders;
        public ObservableCollection<OrderModel> ServedOrders
        {
            get => _servedOrders;
            set { _servedOrders = value; OnPropertyChanged(nameof(ServedOrders)); }
        }
        public ObservableCollection<string> Platforms { get; set; }
        private string _selectedPlatform;
        public string SelectedPlatform
        {
            get => _selectedPlatform;
            set
            {
                if (_selectedPlatform != value)
                {
                    _selectedPlatform = value;
                    OnPropertyChanged(nameof(SelectedPlatform));
                    FilterOrders();
                }
            }
        }
        public string CurrentPage { get; set; }

        public KitchenViewModel()
        {
            CurrentPage = "Kitchen";
            Platforms = new ObservableCollection<string> { "All Platforms", "UberEats", "Deliveroo", "Webshop" };

            // Sample data
            _allQueueOrders = new List<OrderModel>
            {
                new OrderModel {
                    OrderNumber = "Order #1827",
                    CreatedAt = System.DateTime.Now.AddMinutes(-29),
                    Platform = "UberEats",
                    CustomerName = "John Doe",
                    CustomerPhone = "1234567890",
                    Items = new List<OrderItem> { new OrderItem { Name = "Sample Item", Price = 83.92m, Quantity = 1 } }
                },
                new OrderModel {
                    OrderNumber = "Order #1828",
                    CreatedAt = System.DateTime.Now.AddMinutes(-25),
                    Platform = "Deliveroo",
                    CustomerName = "Jane Smith",
                    CustomerPhone = "1234567890",
                    Items = new List<OrderItem> { 
                        new OrderItem { Name = "Burger", Price = 45.50m, Quantity = 1 },
                        new OrderItem { Name = "Salad", Price = 10.00m, Quantity = 1 },
                        new OrderItem { Name = "Coke", Price = 2.00m, Quantity = 1 },
                     }
                    
                },
                new OrderModel {
                OrderNumber = "Order #1829",
                CreatedAt = System.DateTime.Now.AddMinutes(-22),
                Platform = null,
                OrderType = OrderType.DineIn,
                TableNumber = 2,
                CustomerName = "John Doe",
                CustomerPhone = "1234567890",
                Items = new List<OrderItem> { new OrderItem { Name = "Burger", Price = 55.00m, Quantity = 1 } }
                }   
            };
            _allPreparingOrders = new List<OrderModel>
            {
                new OrderModel {
                    OrderNumber = "Order #1829",
                    CreatedAt = System.DateTime.Now.AddMinutes(-20),
                    Platform = "Webshop",
                    CustomerName = "John Doe",
                    CustomerPhone = "1234567890",
                    Items = new List<OrderItem> { new OrderItem { Name = "Sample Item", Price = 99.99m, Quantity = 1 } }
                },
                new OrderModel {
                    OrderNumber = "Order #1830",
                    CreatedAt = System.DateTime.Now.AddMinutes(-15),
                    Platform = "UberEats",
                    CustomerName = "Jane Smith",
                    CustomerPhone = "1234567890",
                    Items = new List<OrderItem> { new OrderItem { Name = "Sample Item", Price = 120.00m, Quantity = 1 } }
                },
            };
            _allServedOrders = new List<OrderModel>
            {
                new OrderModel {
                    OrderNumber = "Order #1831",
                    CreatedAt = System.DateTime.Now.AddMinutes(-10),
                    Platform = "Deliveroo",
                    CustomerName = "Mike Johnson",
                    CustomerPhone = "1234567890",
                    Items = new List<OrderItem> { new OrderItem { Name = "Burger", Price = 60.00m, Quantity = 1 } }
                },
                new OrderModel {
                    OrderNumber = "Order #1832",
                    CreatedAt = System.DateTime.Now.AddMinutes(-5),
                    Platform = "Webshop",
                    CustomerName = "Emily Brown",
                    CustomerPhone = "1234567890",
                    Items = new List<OrderItem> { new OrderItem { Name = "Sample Item", Price = 75.00m, Quantity = 1 } }
                },
                new OrderModel {
                OrderNumber = "Order #1834",
                TableNumber = 1,
                    CreatedAt = System.DateTime.Now.AddMinutes(-8),
                    Platform = null,
                    OrderType = OrderType.TakeAway,
                    CustomerName = "David Wilson",
                    CustomerPhone = "1234567890",
                    Items = new List<OrderItem> { new OrderItem { Name = "Salad", Price = 30.00m, Quantity = 1 } }
                },
                new OrderModel {
                    OrderNumber = "Order #1833",
                    CreatedAt = System.DateTime.Now.AddMinutes(-18),
                    Platform = null,
                    OrderType = OrderType.Delivery,
                    CustomerName = "Olivia Davis",
                    CustomerPhone = "1234567890",
                    Items = new List<OrderItem> { new OrderItem { Name = "Pizza", Price = 70.00m, Quantity = 1 } }
                },
            };
           
            // Initialize collections to avoid null
            QueueOrders = new ObservableCollection<OrderModel>(_allQueueOrders);
            PreparingOrders = new ObservableCollection<OrderModel>(_allPreparingOrders);
            ServedOrders = new ObservableCollection<OrderModel>(_allServedOrders);

            // Now set the platform (this will trigger filtering)
            SelectedPlatform = Platforms[0];
        }

        private void FilterOrders()
        {
            if (SelectedPlatform == "All Platforms")
            {
                QueueOrders = new ObservableCollection<OrderModel>(_allQueueOrders);
                PreparingOrders = new ObservableCollection<OrderModel>(_allPreparingOrders);
                ServedOrders = new ObservableCollection<OrderModel>(_allServedOrders);
            }
            else
            {
                QueueOrders = new ObservableCollection<OrderModel>(_allQueueOrders.Where(o => o.Platform == SelectedPlatform));
                PreparingOrders = new ObservableCollection<OrderModel>(_allPreparingOrders.Where(o => o.Platform == SelectedPlatform));
                ServedOrders = new ObservableCollection<OrderModel>(_allServedOrders.Where(o => o.Platform == SelectedPlatform));
            }
        }
    }
} 