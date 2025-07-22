using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using POS_UI.Models;
using System.Windows;
using System.Collections.Generic;
using POS_UI.Services;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POS_UI.ViewModels
{
    public enum ProductSortOption
    {
        None,
        AZ,
        ZA,
        PriceLowHigh,
        PriceHighLow
    }

    public enum PaymentMethod { CreditCard, Paypal, Cash }

    public class CashierHomeViewModel : LoadingViewModelBase
    {
        private readonly ApiService _apiService = new ApiService();
        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<POS_UI.Models.ProductItemModel> AllProducts { get; set; }
        public ObservableCollection<POS_UI.Models.ProductItemModel> Products { get; set; }
        public ObservableCollection<OrderItem> OrderItems { get; set; }
        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    FilterProducts();
                }
            }
        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    FilterProducts();
                }
            }
        }
        public string OrderType { get; set; } = "Take Away";
        private string _timeButtonText = "Now";
        public string TimeButtonText
        {
            get => OrderType == "Dine In" && SelectedTable != null ? $"Table {SelectedTable.TableNumber}" : _timeButtonText;
            set
            {
                _timeButtonText = value;
                OnPropertyChanged();
            }
        }
        public string CustomerName { get; set; }
        private string _note;
        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNote));
            }
        }
        public bool HasNote => !string.IsNullOrWhiteSpace(Note);
        public bool CanAddNote => !HasNote;
        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SubTotal));
            }
        }
        public decimal Total => OrderItems.Sum(i => i.Total);
        public decimal SubTotal => Total - Discount - CouponDiscount;
        public ICommand AddToOrderCommand { get; }
        public ICommand RemoveFromOrderCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ChangeOrderTypeCommand { get; }
        public ICommand PlaceOrderCommand { get; }
        public ICommand SaveOrderCommand { get; }
        public ICommand ApplyDiscountCommand { get; }
        public ICommand AddNoteCommand { get; }
        public ICommand EditNoteCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand OpenAddItemDialogCommand { get; }
        public ICommand EditOrderItemCommand { get; }
        public string CurrentPage { get; set; }
        public ObservableCollection<TableModel> Tables { get; set; }
        private TableModel _selectedTable;
        public TableModel SelectedTable
        {
            get => _selectedTable;
            set
            {
                _selectedTable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeButtonText));
            }
        }
        public ICommand OpenTableSelectionCommand { get; }
        public ICommand OpenCouponDialogCommand { get; }
        public ICommand ApplyCouponCommand { get; }
        public ICommand RemoveCouponCommand { get; }
        public ICommand OpenDiscountDialogCommand { get; }
        public ICommand OpenTimePickerCommand { get; }
        private DateTime? _selectedOrderTime;
        public DateTime? SelectedOrderTime
        {
            get => _selectedOrderTime;
            set
            {
                _selectedOrderTime = value;
                TimeButtonText = value.HasValue ? value.Value.ToString("hh:mm tt") : "Now";
                OnPropertyChanged();
            }
        }

        private string _couponCode;
        public string CouponCode
        {
            get => _couponCode;
            set { _couponCode = value; OnPropertyChanged(); }
        }

        private string _couponDescription;
        public string CouponDescription
        {
            get => _couponDescription;
            set { _couponDescription = value; OnPropertyChanged(); }
        }

        private decimal _couponDiscount;
        public decimal CouponDiscount
        {
            get => _couponDiscount;
            set { _couponDiscount = value; OnPropertyChanged(); OnPropertyChanged(nameof(SubTotal)); }
        }

        public bool HasCoupon => !string.IsNullOrWhiteSpace(CouponCode);
        public bool CanAddCoupon => !HasCoupon;

        private decimal _discountPercent;
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                _discountPercent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DiscountDescription));
            }
        }
        public string DiscountDescription => DiscountPercent > 0 ? $"Discount ({DiscountPercent}%)" : "Discount";

        public ObservableCollection<POS_UI.Models.CategoryModel> CategoriesWithCount
        {
            get
            {
                var list = Categories
                    .Select(cat => new POS_UI.Models.CategoryModel
                    {
                        CategoryName = cat,
                        Quantity = cat == "All Items" ? AllProducts.Count : AllProducts.Count(p => p.Category == cat)
                    })
                    .ToList();
                return new ObservableCollection<POS_UI.Models.CategoryModel>(list);
            }
        }

        private CustomerModel _selectedCustomer;
        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }
        public ObservableCollection<CustomerModel> Customers { get; set; }
        public ICommand SelectCustomerCommand { get; }

        public ObservableCollection<DraftOrderModel> DraftOrders { get; set; } = new ObservableCollection<DraftOrderModel>();
        public ICommand OpenDraftsCommand { get; }

        private ProductSortOption _selectedSortOption = ProductSortOption.AZ;
        public ProductSortOption SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption != value)
                {
                    _selectedSortOption = value;
                    OnPropertyChanged();
                    ApplySortFilter();
                }
            }
        }

        public ICommand ClearSortCommand { get; }

        public List<ProductSortOption> SortOptions { get; } = new List<ProductSortOption>
        {
            ProductSortOption.AZ,
            ProductSortOption.ZA,
            ProductSortOption.PriceLowHigh,
            ProductSortOption.PriceHighLow
        };

        private PaymentMethod _selectedPaymentMethod = PaymentMethod.CreditCard;
        public PaymentMethod SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set { _selectedPaymentMethod = value; OnPropertyChanged(); }
        }
        public ICommand SelectPaymentMethodCommand { get; }
        public ICommand ConfirmOrderCommand { get; }
        public bool CanPlaceOrder => OrderItems != null && OrderItems.Count > 0;

        //Alert Notification
        private bool _isOrderAlertVisible;
        public bool IsOrderAlertVisible
        {
            get => _isOrderAlertVisible;
            set { _isOrderAlertVisible = value; OnPropertyChanged(); }
        }



        private ICommand _showOrderAlertCommand;
        public ICommand ShowOrderAlertCommand => _showOrderAlertCommand ??= new RelayCommand(ShowOrderAlert);
        private void ShowOrderAlert()
        {
            IsOrderAlertVisible = true;
        }

        private ICommand _viewOrderCommand;
        public ICommand ViewOrderCommand => _viewOrderCommand ??= new RelayCommand(ViewOrder);
        private void ViewOrder()
        {
            // Hide the popup and navigate to the order page or perform any action
            IsOrderAlertVisible = false;
            // Add navigation or logic here
        }

        private string _displayOrderId;
        public string DisplayOrderId
        {
            get => _displayOrderId;
            set { _displayOrderId = value; OnPropertyChanged(nameof(DisplayOrderId)); }
        }

        private static string GenerateOrderId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public CashierHomeViewModel()
        {
            // Set the bearer token for protected API calls
            var accessToken = POS_UI.Properties.Settings.Default.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                _apiService.SetBearerToken(accessToken);
            } 
            
            // Initialize collections
            Categories = new ObservableCollection<string>();
            AllProducts = new ObservableCollection<POS_UI.Models.ProductItemModel>();
            Products = new ObservableCollection<POS_UI.Models.ProductItemModel>();
            OrderItems = new ObservableCollection<OrderItem>();
            OrderItems.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(SubTotal));
                OnPropertyChanged(nameof(CanPlaceOrder));
                ((RelayCommand)PlaceOrderCommand)?.RaiseCanExecuteChanged();
            };
            AddToOrderCommand = new CashierRelayCommand<POS_UI.Models.ProductItemModel>(AddToOrder);
            RemoveFromOrderCommand = new CashierRelayCommand<OrderItem>(RemoveFromOrder);
            DecreaseQuantityCommand = new CashierRelayCommand<OrderItem>(DecreaseQuantity);
            ChangeOrderTypeCommand = new CashierRelayCommand<string>(type => 
            { 
                OrderType = type;
                TimeButtonText = type == "Dine In" ? "Table" : "Now";
                OnPropertyChanged(nameof(OrderType));
            });
            PlaceOrderCommand = new RelayCommand(PlaceOrder, () => CanPlaceOrder);
            SaveOrderCommand = new CashierRelayCommand(SaveOrder);
            ApplyDiscountCommand = new CashierRelayCommand(ApplyDiscount);
            AddNoteCommand = new CashierRelayCommand(AddNote);
            EditNoteCommand = new CashierRelayCommand(EditNote);
            SelectCategoryCommand = new CashierRelayCommand<string>(SelectCategory);
            OpenAddItemDialogCommand = new CashierRelayCommand<POS_UI.Models.ProductItemModel>(OpenAddItemDialog);
            EditOrderItemCommand = new CashierRelayCommand<OrderItem>(EditOrderItem);
            CurrentPage = "Cashier"; // Set the active page
            SelectedSortOption = ProductSortOption.AZ;
            OpenTableSelectionCommand = new CashierRelayCommand(OpenTableSelection);
            OpenCouponDialogCommand = new CashierRelayCommand(OpenCouponDialog, () => CanAddCoupon);
            ApplyCouponCommand = new CashierRelayCommand<string>(ApplyCoupon);
            RemoveCouponCommand = new CashierRelayCommand(RemoveCoupon);
            OpenDiscountDialogCommand = new CashierRelayCommand(OpenDiscountDialog);
            OpenTimePickerCommand = new RelayCommand(async () => await OpenTimePicker());
            Customers = new ObservableCollection<CustomerModel>
            {
                new CustomerModel { CustomerId=123, Name = "Banuka Perera", Phone = "+971 2738 292 232" },
                new CustomerModel { CustomerId=456, Name = "John Doe", Phone = "+971 1111 222 333" },
                new CustomerModel { CustomerId=789, Name = "Jane Smith", Phone = "+971 4444 555 666" }
            };
            SelectedCustomer = Customers.FirstOrDefault();
            SelectCustomerCommand = new CashierRelayCommand(OpenSelectCustomerDialog);
            OpenDraftsCommand = new RelayCommand(OpenDrafts);
            ClearSortCommand = new RelayCommand(ClearSort);
            SelectPaymentMethodCommand = new RelayCommand<PaymentMethod>(pm => SelectedPaymentMethod = pm);
            ConfirmOrderCommand = new RelayCommand(ConfirmOrder);
            DisplayOrderId = GenerateOrderId();
            
            // Load categories from API
            _ = LoadDataAsync();
        }
        
        private async Task LoadDataAsync()
        {
            await SetLoadingAsync(async () =>
            {
                try
                {
                    var (apiCategories, apiProducts) = await _apiService.GetProductsAndCategoriesAsync();
                    
                    // Add "All Items" as the first category
                    Categories.Clear();
                    Categories.Add("All Items");
                    
                    // Add categories from API
                    foreach (var category in apiCategories)
                    {
                        Categories.Add(category);
                    }

                    AllProducts.Clear();
                    foreach(var product in apiProducts)
                    {
                        AllProducts.Add(product);
                    }
                    
                    // Set the first category as selected
                    if (Categories.Count > 0)
                    {
                        SelectedCategory = Categories.First();
                    }
                    
                    OnPropertyChanged(nameof(CategoriesWithCount));
                    FilterProducts();
                }
                catch (Exception ex)
                {
                    // Fallback to hardcoded categories if API fails
                    Categories.Clear();
                    Categories.Add("All Items");
                    Categories.Add("Desert");
                    Categories.Add("Bakery");
                    Categories.Add("Starter");
                    Categories.Add("Main Dish");
                    Categories.Add("Beverage");
                    
                    if (Categories.Count > 0)
                    {
                        SelectedCategory = Categories.First();
                    }
                    
                    OnPropertyChanged(nameof(CategoriesWithCount));
                    
                    // Log the error (you might want to show a user-friendly message)
                    System.Diagnostics.Debug.WriteLine($"Failed to load data from API: {ex.Message}");
                }
            });
        }

        private void AddToOrder(POS_UI.Models.ProductItemModel product)
        {
            // For simple add operations (no modifiers), just match by name, price, and size
            var item = OrderItems.FirstOrDefault(i => i.Product.ItemName == product.ItemName && i.Product.Price == product.Price && i.Product.Size == product.Size);
            if (item != null) 
            {
                // If item exists, increment quantity and preserve discount percentage
                item.Quantity++;
            }
            else 
            {
                // If item doesn't exist, create new item without discount
                OrderItems.Add(new OrderItem { Product = product, Price = product.Price, Quantity = 1, DiscountPercent = 0 });
            }
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(SubTotal));
        }

        // Helper method to find matching order item considering all properties
        private OrderItem FindMatchingOrderItem(string itemName, string size, Dictionary<int, List<string>> selectedModifiers, Dictionary<string, List<string>> nestedModifierDetails)
        {
            // First, try to find an exact match
            var exactMatch = OrderItems.FirstOrDefault(i => 
                i.Product.ItemName == itemName && 
                i.Product.Size == size &&
                AreModifiersEqual(i.SelectedModifiers, selectedModifiers) &&
                AreNestedModifiersEqual(i.NestedModifierDetails, nestedModifierDetails));
            
            if (exactMatch != null)
                return exactMatch;
            
            // If no exact match, try to find a match by name and size only (for updating existing items)
            // This is useful when the user is editing an existing item and changing its modifiers
            return OrderItems.FirstOrDefault(i => 
                i.Product.ItemName == itemName && 
                i.Product.Size == size);
        }

        // Helper method to compare modifier selections
        private bool AreModifiersEqual(Dictionary<int, List<string>> modifiers1, Dictionary<int, List<string>> modifiers2)
        {
            if (modifiers1 == null && modifiers2 == null) return true;
            if (modifiers1 == null || modifiers2 == null) return false;
            if (modifiers1.Count != modifiers2.Count) return false;

            foreach (var kvp in modifiers1)
            {
                if (!modifiers2.TryGetValue(kvp.Key, out var list2)) return false;
                if (kvp.Value == null && list2 == null) continue;
                if (kvp.Value == null || list2 == null) return false;
                if (kvp.Value.Count != list2.Count) return false;
                if (!kvp.Value.OrderBy(x => x).SequenceEqual(list2.OrderBy(x => x))) return false;
            }
            return true;
        }

        // Helper method to compare nested modifier details
        private bool AreNestedModifiersEqual(Dictionary<string, List<string>> nested1, Dictionary<string, List<string>> nested2)
        {
            if (nested1 == null && nested2 == null) return true;
            if (nested1 == null || nested2 == null) return false;
            if (nested1.Count != nested2.Count) return false;

            foreach (var kvp in nested1)
            {
                if (!nested2.TryGetValue(kvp.Key, out var list2)) return false;
                if (kvp.Value == null && list2 == null) continue;
                if (kvp.Value == null || list2 == null) return false;
                if (kvp.Value.Count != list2.Count) return false;
                if (!kvp.Value.OrderBy(x => x).SequenceEqual(list2.OrderBy(x => x))) return false;
            }
            return true;
        }
        private void RemoveFromOrder(OrderItem item)
        {
            OrderItems.Remove(item);
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(SubTotal));
        }
        
        private void DecreaseQuantity(OrderItem item)
        {
            if (item.Quantity > 1)
            {
                // Decrease quantity - discount will be recalculated automatically due to property change notifications
                item.Quantity--;
            }
            else
            {
                // If quantity is 1, remove the entire item
                OrderItems.Remove(item);
            }
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(SubTotal));
        }
        private void PlaceOrder()
        {
            // Show the MaterialDesign DialogHost popup
            var dialog = new POS_UI.View.CheckoutDialog { DataContext = this };
            MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "AddItemDialogHost");
        }
        private void SaveOrder()
        {
            try
            {
                if (OrderItems.Count == 0)
                {
                    MessageBox.Show("Cannot save an empty order.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Save current order as draft
                var draft = new DraftOrderModel
                {
                    CustomerName = SelectedCustomer?.Name,
                    Amount = SubTotal,
                    CreatedAt = DateTime.Now,
                    OrderType = OrderType,
                    TableName = OrderType == "Dine In" ? SelectedTable?.TableNumber.ToString() : null,
                    Items = OrderItems.Select(i => new Models.OrderItem
                    {
                        Name = i.Product.ItemName,
                        Price = i.Product.Price,
                        Quantity = i.Quantity,
                        Notes = i.Note
                    }).ToList()
                };

                // Log draft order details for debugging
                Console.WriteLine($"Saving draft order: Customer={draft.CustomerName}, OrderType={draft.OrderType}, Amount={draft.Amount}, Items={draft.Items.Count}");

                // Add to appropriate collection based on order type
                DraftOrders.Add(draft);
                OnPropertyChanged(nameof(DraftOrders));

                // Clear the current order
                OrderItems.Clear();
                OnPropertyChanged(nameof(OrderItems));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(SubTotal));

                MessageBox.Show("Order saved as draft successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving draft: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ApplyDiscount() { /* Discount logic */ }
        private async void AddNote()
        {
            try
            {
                var dialogVm = new NoteDialogViewModel(Note);
                var dialog = new POS_UI.View.NoteDialog { DataContext = dialogVm };
                
                dialogVm.NoteSaved += (note) =>
                {
                    Note = note;
                    OnPropertyChanged(nameof(CanAddNote));
                };
                
                dialogVm.DialogClosed += () => DialogHost.CloseDialogCommand.Execute(null, null);
                await DialogHost.Show(dialog, "AddItemDialogHost");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding note: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void EditNote()
        {
            try
            {
                var dialogVm = new NoteDialogViewModel(Note);
                var dialog = new POS_UI.View.NoteDialog { DataContext = dialogVm };
                
                dialogVm.NoteSaved += (note) =>
                {
                    Note = note;
                    OnPropertyChanged(nameof(CanAddNote));
                };
                
                dialogVm.DialogClosed += () => DialogHost.CloseDialogCommand.Execute(null, null);
                await DialogHost.Show(dialog, "AddItemDialogHost");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing note: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SelectCategory(string category)
        {
            SelectedCategory = category;
        }
        private void FilterProducts()
        {
            ApplySortFilter();
        }
        private void ApplySortFilter()
        {
            IEnumerable<POS_UI.Models.ProductItemModel> filtered = AllProducts;
            if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "All Items")
                filtered = filtered.Where(p => p.Category == SelectedCategory);
            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(p => p.ItemName.ToLower().Contains(SearchText.ToLower()));
            switch (SelectedSortOption)
            {
                case ProductSortOption.AZ:
                    filtered = filtered.OrderBy(p => p.ItemName);
                    break;
                case ProductSortOption.ZA:
                    filtered = filtered.OrderByDescending(p => p.ItemName);
                    break;
                case ProductSortOption.PriceLowHigh:
                    filtered = filtered.OrderBy(p => p.Price);
                    break;
                case ProductSortOption.PriceHighLow:
                    filtered = filtered.OrderByDescending(p => p.Price);
                    break;
            }
            Products = new ObservableCollection<POS_UI.Models.ProductItemModel>(filtered);
            OnPropertyChanged(nameof(Products));
        }
        private void ClearSort()
        {
            SelectedSortOption = ProductSortOption.None;
            ApplySortFilter();
        }
        private async void OpenAddItemDialog(POS_UI.Models.ProductItemModel product)
        {
            try
            {
                if (product == null)
                {
                    System.Windows.MessageBox.Show("Product is null", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }

                // Check if product has modifiers
                bool hasModifiers = product.Modifiers != null && product.Modifiers.Count > 0 && 
                                   product.Modifiers.FirstOrDefault()?.ModifierItems != null && 
                                   product.Modifiers.FirstOrDefault().ModifierItems.Count > 0;

                if (hasModifiers)
                {
                    // Product has modifiers - open AddModifiersDialog first
                    await OpenModifiersDialogFirst(product);
                }
                else
                {
                    // Product has no modifiers - open AddItemDialog directly
                    await OpenAddItemDialogDirectly(product, null);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error showing dialog: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task OpenModifiersDialogFirst(POS_UI.Models.ProductItemModel product)
        {
            Dictionary<int, string> selectedModifiers = null;
            AddModifiersDialogViewModel modifiersDialogVm = null;
            try
            {
                modifiersDialogVm = new AddModifiersDialogViewModel(product.Modifiers);
                var modifiersDialog = new POS_UI.View.AddModifiersDialog { DataContext = modifiersDialogVm };
                modifiersDialogVm.ModifierSaved += (modifiers) =>
                {
                    selectedModifiers = modifiers;
                };
                modifiersDialogVm.DialogClosed += () => MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
                // Handle nested modifiers
                modifiersDialogVm.NestedModifierRequested += async (modifierItem) =>
                {
                    try
                    {
                        var nestedDialogVm = new NestedModifiersDialogViewModel(modifierItem.NestedModifiers, modifierItem.ItemName);
                        var nestedDialog = new POS_UI.View.NestedModifiersDialog { DataContext = nestedDialogVm };
                        Dictionary<int, string> selectedNestedModifiers = null;
                        nestedDialogVm.NestedModifierSaved += (nestedModifiers) =>
                        {
                            selectedNestedModifiers = nestedModifiers;
                        };
                        nestedDialogVm.DialogClosed += () => MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
                        await MaterialDesignThemes.Wpf.DialogHost.Show(nestedDialog, "NestedModifiersDialogHost");
                        if (selectedNestedModifiers != null)
                        {
                            // Format nested details for summary
                            var nestedDetails = new List<string>();
                            foreach (var group in modifierItem.NestedModifiers)
                            {
                                // Handle multiple selections from nested modifiers
                                if (selectedNestedModifiers.TryGetValue(group.Id, out var selectedNames))
                                {
                                    // Split the comma-separated string into individual selections
                                    var itemNames = selectedNames.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var selectedName in itemNames)
                                {
                                    var selected = group.ModifierItems?.FirstOrDefault(item => item.ItemName == selectedName);
                                    if (selected != null)
                                    {
                                        nestedDetails.Add($"{group.Title}: {selected.ItemName}   ${selected.ItemPrice:0.00}");
                                        }
                                    }
                                }
                            }
                            // Find the parent groupId for this modifierItem
                            var parentGroup = modifiersDialogVm.ModifierGroups.FirstOrDefault(g => g.ModifierItems != null && g.ModifierItems.Contains(modifierItem));
                            if (parentGroup != null)
                            {
                                modifiersDialogVm.SetNestedModifierDetails(parentGroup.Id, nestedDetails, modifierItem);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error opening nested modifiers dialog: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                };
                await MaterialDesignThemes.Wpf.DialogHost.Show(modifiersDialog, "AddItemDialogHost");
                // Only open AddItemDialog after the modifiers dialog is fully closed
                if (selectedModifiers != null && selectedModifiers.Count > 0)
                {
                    // Convert to multiple selection format
                    var multipleSelections = new Dictionary<int, List<string>>();
                    foreach (var kvp in selectedModifiers)
                    {
                        var itemNames = kvp.Value.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                        multipleSelections[kvp.Key] = new List<string>(itemNames);
                    }
                    
                    // Get the first selected modifier for backward compatibility
                    var firstSelected = selectedModifiers.Values.FirstOrDefault();
                    var firstItemName = firstSelected?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    
                    await OpenAddItemDialogDirectly(product, firstItemName, selectedModifiers, modifiersDialogVm.GetAllNestedModifierDetails(), multipleSelections);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening modifiers dialog: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task OpenAddItemDialogDirectly(POS_UI.Models.ProductItemModel product, string selectedSize = null, Dictionary<int, string> selectedModifiers = null, Dictionary<string, List<string>> nestedModifierDetails = null, Dictionary<int, List<string>> selectedModifiersMultiple = null)
        {
            try
            {
                // For pre-filling, we need to find the most similar item since we don't have the exact modifiers yet
                // We'll look for an item with the same name and size, and if there are multiple, we'll use the first one
                var existingItem = OrderItems.FirstOrDefault(i => i.Product.ItemName == product.ItemName && i.Product.Size == (selectedSize ?? "Small"));
                // Create the dialog view model with proper error handling
                AddItemDialogViewModel dialogVm;
                try
                {
                    // Use the new constructor for multiple selections if available, otherwise fall back to the old one
                    if (selectedModifiersMultiple != null && selectedModifiersMultiple.Count > 0)
                    {
                        dialogVm = new AddItemDialogViewModel(product.ItemName, product.Price, product, selectedModifiersMultiple, nestedModifierDetails);
                    }
                    else
                {
                    dialogVm = new AddItemDialogViewModel(product.ItemName, product.Price, product, selectedModifiers, nestedModifierDetails);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error creating dialog view model: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
                if (existingItem != null)
                {
                    dialogVm.SelectedSize = existingItem.Product?.Size ?? selectedSize ?? "Small";
                    dialogVm.Quantity = existingItem.Quantity;
                    dialogVm.Note = existingItem.Note;
                    // Pre-fill modifiers
                    if (existingItem.SelectedModifiers != null && existingItem.SelectedModifiers.Count > 0)
                        dialogVm.SelectedModifiersMultiple = new Dictionary<int, List<string>>(existingItem.SelectedModifiers);
                    if (existingItem.NestedModifierDetails != null && existingItem.NestedModifierDetails.Count > 0)
                        dialogVm.NestedModifierDetails = new Dictionary<string, List<string>>(existingItem.NestedModifierDetails);
                    // Pre-fill discount
                    dialogVm.DiscountPercent = existingItem.DiscountPercent;
                    dialogVm.IsDiscount10Selected = Math.Abs(existingItem.DiscountPercent - 10) < 0.1m;
                    dialogVm.IsDiscount20Selected = Math.Abs(existingItem.DiscountPercent - 20) < 0.1m;
                 }
                else if (selectedSize != null)
                {
                    dialogVm.SelectedSize = selectedSize;
                }
                // Create the dialog with proper error handling
                POS_UI.View.AddItemDialog dialog;
                try
                {
                    dialog = new POS_UI.View.AddItemDialog { DataContext = dialogVm };
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error creating dialog: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
                dialogVm.ItemAdded += (itemVm) =>
                {
                    try
                    {
                        // Use the comprehensive matching function to find the exact item
                        var item = FindMatchingOrderItem(product.ItemName, itemVm.SelectedSize, itemVm.SelectedModifiersMultiple, itemVm.NestedModifierDetails);
                        
                        if (item != null)
                        {
                            // Update existing item with new values
                            item.Quantity = itemVm.Quantity;
                            item.Product.Size = itemVm.SelectedSize;
                            item.Note = itemVm.Note;
                            // Update modifiers
                            if (itemVm.SelectedModifiersMultiple != null)
                                item.SelectedModifiers = new Dictionary<int, List<string>>(itemVm.SelectedModifiersMultiple);
                            if (itemVm.NestedModifierDetails != null)
                                item.NestedModifierDetails = new Dictionary<string, List<string>>(itemVm.NestedModifierDetails);
                            // Update discount percentage
                            item.DiscountPercent = itemVm.DiscountPercent;
                            // Update price
                            item.Price = Math.Round(itemVm.FinalPrice / itemVm.Quantity, 2);
                        }
                        else
                        {
                            // Create new item if no exact match found
                            var newOrderItem = new OrderItem
                            {
                                Product = new POS_UI.Models.ProductItemModel 
                                { 
                                    ItemName = product.ItemName, 
                                    Price = itemVm.UnitPrice, 
                                    Category = product.Category, 
                                    Size = itemVm.SelectedSize,
                                    Modifiers = product.Modifiers // Copy the modifiers from the original product
                                },
                                Quantity = itemVm.Quantity,
                                Note = itemVm.Note,
                                Price = Math.Round(itemVm.FinalPrice / itemVm.Quantity, 2), // Store per-item discounted price
                                DiscountPercent = itemVm.DiscountPercent // Store discount percentage for this item
                            };
                            
                            // Store modifier details
                            if (itemVm.SelectedModifiersMultiple != null)
                            {
                                newOrderItem.SelectedModifiers = new Dictionary<int, List<string>>(itemVm.SelectedModifiersMultiple);
                            }
                            if (itemVm.NestedModifierDetails != null)
                            {
                                newOrderItem.NestedModifierDetails = new Dictionary<string, List<string>>(itemVm.NestedModifierDetails);
                            }
                            
                            OrderItems.Add(newOrderItem);
                        }
                        OnPropertyChanged(nameof(Total));
                        OnPropertyChanged(nameof(SubTotal));
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error adding item: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                };
                dialogVm.DialogClosed += () => 
                {
                    try
                    {
                        DialogHost.CloseDialogCommand.Execute(null, null);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error closing dialog: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                };
                await DialogHost.Show(dialog, "AddItemDialogHost");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error showing dialog: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        private async void OpenTableSelection()
        {
            if (OrderType != "Dine In") return;
            try
            {
                var tablesViewModel = new POS_UI.ViewModels.TablesViewModel();
                var dialogVm = new TableSelectionDialogViewModel(tablesViewModel.Tables, SelectedTable);
                var dialog = new POS_UI.View.TableSelectionDialog { DataContext = dialogVm };
                var result = await DialogHost.Show(dialog, "AddItemDialogHost");
                if (result is TableModel selected && selected != null)
                {
                    SelectedTable = selected;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open table selection dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void OpenCouponDialog()
        {
            try
            {
                var dialog = new POS_UI.View.SetCouponDialog();
                // Set DataContext if you want to bind to a dialog-specific VM
                var result = await DialogHost.Show(dialog, "AddItemDialogHost");
                if (result is string couponCode && !string.IsNullOrWhiteSpace(couponCode))
                {
                    ApplyCoupon(couponCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening coupon dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ApplyCoupon(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            code = code.Trim();
            if (int.TryParse(code, out int percent) && percent > 0 && percent <= 100)
            {
                CouponCode = code;
                CouponDescription = $"Coupon ({percent}%)";
                CouponDiscount = Total * percent / 100m;
            }
            else
            {
                CouponCode = code;
                CouponDescription = $"Coupon ({code})";
                CouponDiscount = 0;
                MessageBox.Show("Invalid coupon code. Please enter a number between 1 and 100 for percentage discount.", "Coupon", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            OnPropertyChanged(nameof(HasCoupon));
            OnPropertyChanged(nameof(CanAddCoupon));
            OnPropertyChanged(nameof(SubTotal));
        }
        private void RemoveCoupon()
        {
            CouponCode = null;
            CouponDescription = null;
            CouponDiscount = 0;
            OnPropertyChanged(nameof(HasCoupon));
            OnPropertyChanged(nameof(CanAddCoupon));
            OnPropertyChanged(nameof(SubTotal));
        }
        private async void OpenDiscountDialog()
        {
            try
            {
                var dialog = new POS_UI.View.SetDiscountDialog();
                var result = await DialogHost.Show(dialog, "AddItemDialogHost");
                if (result is string discountStr && !string.IsNullOrWhiteSpace(discountStr))
                {
                    if (decimal.TryParse(discountStr, out decimal percent) && percent > 0 && percent <= 100)
                    {
                        DiscountPercent = percent;
                        Discount = Total * percent / 100m;
                    }
                    else
                    {
                        MessageBox.Show("Invalid discount. Please enter a number between 1 and 100.", "Discount", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening discount dialog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void OpenSelectCustomerDialog()
        {
            try
            {
                var dialogVm = new SelectCustomerDialogViewModel(Customers, SelectedCustomer);
                var dialog = new POS_UI.View.SelectCustomerDialog { DataContext = dialogVm };
                var result = await DialogHost.Show(dialog, "AddItemDialogHost");
                if (result is CustomerModel selected && selected != null)
                {
                    SelectedCustomer = selected;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async System.Threading.Tasks.Task OpenTimePicker()
        {
            var dialogVm = new TimePickerDialogViewModel();
            var dialog = new POS_UI.View.TimePickerDialog { DataContext = dialogVm };
            var result = await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "AddItemDialogHost");
            if (result is DateTime selectedTime)
            {
                SelectedOrderTime = selectedTime;
            }
        }
        private async void OpenDrafts()
        {
            try
            {
                if (DraftOrders.Count == 0)
                {
                    MessageBox.Show("No draft orders available.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialogVm = new DraftsDialogViewModel(DraftOrders);
                var dialog = new POS_UI.View.DraftsDialog { DataContext = dialogVm };
                
                // Ensure the dialog is properly initialized
                dialog.Loaded += (s, e) => 
                {
                    Console.WriteLine($"Dialog loaded with {DraftOrders.Count} drafts");
                };

                try
                {
                    var result = await DialogHost.Show(dialog, "AddItemDialogHost");
                    Console.WriteLine($"Dialog result: {result}");

                    if (result is DraftOrderModel selectedDraft)
                    {
                        // Load the selected draft
                        CustomerName = selectedDraft.CustomerName;
                        OrderType = selectedDraft.OrderType;
                        if (OrderType == "Dine In" && !string.IsNullOrEmpty(selectedDraft.TableName))
                        {
                            SelectedTable = Tables.FirstOrDefault(t => t.TableNumber.ToString() == selectedDraft.TableName);
                        }
                        
                        // Clear current order items and load the draft items
                        OrderItems.Clear();
                        foreach (var item in selectedDraft.Items)
                        {
                            var product = AllProducts.FirstOrDefault(p => p.ItemName == item.Name && p.Price == item.Price);
                            if (product != null)
                            {
                                OrderItems.Add(new OrderItem
                                {
                                    Product = product,
                                    Quantity = item.Quantity,
                                    Note = item.Notes
                                });
                            }
                        }
                        
                        OnPropertyChanged(nameof(OrderItems));
                        OnPropertyChanged(nameof(Total));
                        OnPropertyChanged(nameof(SubTotal));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error showing draft dialog: {ex.Message}\n{ex.StackTrace}", "Dialog Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening drafts: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SelectPaymentMethod(object method)
        {
            if (method is PaymentMethod pm)
                SelectedPaymentMethod = pm;
        }
        private async void ConfirmOrder()
        {
            try
            {
                string shippingMethod = OrderType switch
                {
                    "Take Away" => "TAKEAWAY",
                    "Dine In" => "DINE-IN",
                    "Delivery" => "DELIVERY",
                   
                };

                var orderRequest = new
                {
                    customer_id = SelectedCustomer?.CustomerId ?? 0,
                    customer_name = SelectedCustomer?.Name ?? string.Empty,
                    delivery_date_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    platform_id = 5,
                    discount = Discount,
                    discount_type = "",
                    display_order_id = DisplayOrderId,
                    order_note = Note ?? string.Empty,
                    payment_method = SelectedPaymentMethod.ToString().ToUpper(),
                    transaction_id = "123",
                    shipping_method = OrderType.ToUpper().Replace(" ", "-"),
                    shipping_tax = 0.0,
                    shipping_total = 0.0,
                    shop_id = 2,
                    sub_total = Total,
                    total_amount = SubTotal,
                    total_cash = Total,
                    total_balance = 0.0,
                    table_id = OrderType == "Dine In" ? (SelectedTable?.TableNumber.ToString() ?? "") : "",
                    tip = 0.0,
                    tip_percentage = 0.0,
                    total_tax = 0.0,
                    user_id = 47,
                    order_items = OrderItems.Select(item => new
                    {
                        ItemID = item.Product?.Id ?? 0,
                        item_name = item.Product?.ItemName ?? item.Name,
                        quantity = item.Quantity,
                        price_per_item = item.Product?.Price ?? item.Price,
                        original_price = item.Product?.Price ?? item.Price,
                        is_sale = false,
                        discount_amount = item.DiscountAmount,
                        tax = 0.0,
                        total = item.Total,
                        modifierDetails = item.SelectedModifiers.SelectMany(kvp =>
                            kvp.Value.Select(selectedName =>
                            {
                                var group = item.Product?.Modifiers?.FirstOrDefault(m => m.Id == kvp.Key);
                                var modifierItem = group?.ModifierItems?.FirstOrDefault(mi => mi.ItemName == selectedName);
                                return new
                                {
                                    modifier_main = group?.Title ?? string.Empty,
                                    modifier_main_item = modifierItem?.Id ?? 0,
                                    quantity = 1,
                                    modifierItem = modifierItem == null ? null : new
                                    {
                                        external_item_id = modifierItem.Id,
                                        item_name = modifierItem.ItemName,
                                        price = modifierItem.ItemPrice
                                    }
                                };
                            })
                        ).ToList()
                    }).ToList()
                };

                var json = System.Text.Json.JsonSerializer.Serialize(orderRequest);
                MessageBox.Show(json, "DEBUG: JSON Sent");
                var result = await _apiService.PlaceOrderAsync(orderRequest);
                MessageBox.Show($"Order placed successfully!\n{result}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
                DisplayOrderId = GenerateOrderId();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadOrder(POS_UI.Models.OrderModel order)
        {
            OrderItems.Clear();
            foreach (var item in order.Items)
            {
                // Create a new OrderItem to ensure proper initialization
                var newItem = new OrderItem
                {
                    Name = item.Name, // Explicitly set the name
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Note = item.Notes
                };

                // Improved product matching logic
                string size = item.Product?.Size ?? item.Notes ?? string.Empty;
                var product = AllProducts?.FirstOrDefault(p => p.ItemName == item.Name && p.Price == item.Price && (string.IsNullOrEmpty(size) || p.Size == size));
                
                if (product != null)
                {
                    newItem.Product = product;
                }
                else
                {
                    // If no matching product found, create a basic product with the item's details
                    newItem.Product = new POS_UI.Models.ProductItemModel
                    {
                        ItemName = item.Name,
                        Price = item.Price,
                        Size = size
                    };
                }

                OrderItems.Add(newItem);
            }
            CustomerName = order.CustomerName;
            OrderType = order.OrderType.ToString();
            SelectedTable = Tables?.FirstOrDefault(t => t.TableNumber == order.TableNumber);
            // Set discounts/coupons for summary
            Discount = order.DiscountAmount;
            CouponCode = order.CouponCode;
            CouponDiscount = order.CouponAmount;
            
            // Set coupon description based on the coupon code
            if (!string.IsNullOrEmpty(order.CouponCode))
            {
                if (decimal.TryParse(order.CouponCode, out decimal percent) && percent > 0 && percent <= 100)
                {
                    CouponDescription = $"Coupon ({percent}%)";
                }
                else
                {
                    CouponDescription = $"Coupon ({order.CouponCode})";
                }
            }
            else
            {
                CouponDescription = null;
            }
            
            OnPropertyChanged(nameof(OrderItems));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(CouponCode));
            OnPropertyChanged(nameof(CouponDescription));
            OnPropertyChanged(nameof(CouponDiscount));
            OnPropertyChanged(nameof(HasCoupon));
        }

        private async void EditOrderItem(OrderItem orderItem)
        {
            if (orderItem == null) return;
            var product = orderItem.Product;
            bool hasModifiers = product.Modifiers != null && product.Modifiers.Count > 0 &&
                                product.Modifiers.FirstOrDefault()?.ModifierItems != null &&
                                product.Modifiers.FirstOrDefault().ModifierItems.Count > 0;

            Dictionary<int, List<string>> selectedModifiersMultiple = orderItem.SelectedModifiers != null
                ? new Dictionary<int, List<string>>(orderItem.SelectedModifiers)
                : new Dictionary<int, List<string>>();
            Dictionary<string, List<string>> nestedModifierDetails = orderItem.NestedModifierDetails != null
                ? new Dictionary<string, List<string>>(orderItem.NestedModifierDetails)
                : new Dictionary<string, List<string>>();

            if (hasModifiers)
            {
                // Show AddModifiersDialog pre-filled
                var modifiersDialogVm = new AddModifiersDialogViewModel(
                    product.Modifiers,
                    selectedModifiersMultiple,
                    nestedModifierDetails
                );
                var modifiersDialog = new POS_UI.View.AddModifiersDialog { DataContext = modifiersDialogVm };

                bool modifiersConfirmed = false;
                modifiersDialogVm.ModifierSaved += (modifiers) =>
                {
                    // Convert to multiple selection format
                    selectedModifiersMultiple = modifiers != null
                        ? modifiers.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList()
                        )
                        : new Dictionary<int, List<string>>();
                    modifiersConfirmed = true;
                };
                modifiersDialogVm.DialogClosed += () => MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);

                // Nested modifier dialog handler
                modifiersDialogVm.NestedModifierRequested += async (modifierItem) =>
                {
                    var preSelectedNested = new Dictionary<int, List<string>>();
                    if (nestedModifierDetails.TryGetValue(modifierItem.ItemName, out var nestedList))
                    {
                        foreach (var group in modifierItem.NestedModifiers)
                        {
                            var selectedItems = new List<string>();
                            foreach (var detail in nestedList)
                            {
                                if (detail.StartsWith($"{group.Title}:"))
                                {
                                    var itemName = detail.Substring(detail.IndexOf(':') + 1).Split('$')[0].Trim();
                                    selectedItems.Add(itemName);
                                }
                            }
                            if (selectedItems.Count > 0)
                                preSelectedNested[group.Id] = selectedItems;
                        }
                    }

                    var nestedDialogVm = new NestedModifiersDialogViewModel(
                        modifierItem.NestedModifiers,
                        modifierItem.ItemName,
                        preSelectedNested
                    );
                    var nestedDialog = new POS_UI.View.NestedModifiersDialog { DataContext = nestedDialogVm };
                    Dictionary<int, string> selectedNestedModifiers = null;
                    nestedDialogVm.NestedModifierSaved += (nestedModifiers) => { selectedNestedModifiers = nestedModifiers; };
                    nestedDialogVm.DialogClosed += () => MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
                    await MaterialDesignThemes.Wpf.DialogHost.Show(nestedDialog, "NestedModifiersDialogHost");

                    if (selectedNestedModifiers != null)
                    {
                        var formattedNestedDetails = new List<string>();
                        foreach (var group in modifierItem.NestedModifiers)
                        {
                            if (selectedNestedModifiers.TryGetValue(group.Id, out var selectedNames))
                            {
                                var itemNames = selectedNames.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var selectedName in itemNames)
                                {
                                    var selected = group.ModifierItems?.FirstOrDefault(item => item.ItemName == selectedName);
                                    if (selected != null)
                                        formattedNestedDetails.Add($"{group.Title}: {selected.ItemName}   ${selected.ItemPrice:0.00}");
                                }
                            }
                        }
                        nestedModifierDetails[modifierItem.ItemName] = formattedNestedDetails;
                    }
                };

                await MaterialDesignThemes.Wpf.DialogHost.Show(modifiersDialog, "AddItemDialogHost");
                if (!modifiersConfirmed) return; // User cancelled
            }

            // Show AddItemDialog pre-filled with updated modifiers
            var dialogVm = new AddItemDialogViewModel(
                product.ItemName,
                product.Price,
                product,
                selectedModifiersMultiple,
                nestedModifierDetails
            )
            {
                SelectedSize = product.Size,
                Quantity = orderItem.Quantity,
                Note = orderItem.Note,
                DiscountPercent = orderItem.DiscountPercent,
                IsDiscount10Selected = Math.Abs(orderItem.DiscountPercent - 10) < 0.1m,
                IsDiscount20Selected = Math.Abs(orderItem.DiscountPercent - 20) < 0.1m
            };

            var dialog = new POS_UI.View.AddItemDialog { DataContext = dialogVm };
            bool itemConfirmed = false;
            dialogVm.ItemAdded += (itemVm) =>
            {
                // Update the existing item with new values
                orderItem.Quantity = itemVm.Quantity;
                orderItem.Product.Size = itemVm.SelectedSize;
                orderItem.Note = itemVm.Note;
                if (itemVm.SelectedModifiersMultiple != null)
                    orderItem.SelectedModifiers = new Dictionary<int, List<string>>(itemVm.SelectedModifiersMultiple);
                if (itemVm.NestedModifierDetails != null)
                    orderItem.NestedModifierDetails = new Dictionary<string, List<string>>(itemVm.NestedModifierDetails);
                orderItem.DiscountPercent = itemVm.DiscountPercent;
                orderItem.Price = Math.Round(itemVm.FinalPrice / itemVm.Quantity, 2);
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(SubTotal));
                itemConfirmed = true;
            };
            dialogVm.DialogClosed += () =>
            {
                try
                {
                    DialogHost.CloseDialogCommand.Execute(null, null);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error closing dialog: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            };
            await DialogHost.Show(dialog, "AddItemDialogHost");
            // If itemConfirmed is false, user cancelled
        }
    }
    public class CashierRelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        public CashierRelayCommand(Action execute, Func<bool> canExecute = null) { _execute = execute; _canExecute = canExecute; }
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();
    }
    public class CashierRelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        public CashierRelayCommand(Action<T> execute, Func<T, bool> canExecute = null) { _execute = execute; _canExecute = canExecute; }
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
        public void ExecuteCommand(object parameter) => _execute((T)parameter);
        void ICommand.Execute(object parameter) => ExecuteCommand(parameter);
    }
} 