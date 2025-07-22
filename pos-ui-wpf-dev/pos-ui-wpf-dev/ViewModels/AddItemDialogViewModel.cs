using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using POS_UI.ViewModels;
using POS_UI.Models;
using System.Collections.Generic;
using System.Linq;

namespace POS_UI.ViewModels
{
    public class AddItemDialogViewModel : INotifyPropertyChanged
    {
        public string ProductName { get; set; }
        public decimal BasePrice { get; set; }
        public ProductItemModel Product { get; set; }
        
        private string _selectedSize = "None";
        public string SelectedSize
        {
            get => _selectedSize;
            set
            {
                if (_selectedSize != value)
                {
                    _selectedSize = value;
                    UpdatePriceAdjustment();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UnitPrice));
                    OnPropertyChanged(nameof(FinalPrice));
                    OnPropertyChanged(nameof(Size));
                    OnPropertyChanged(nameof(DiscountAmount));
                }
            }
        }
        public decimal SizePriceAdjustment { get; set; }
        
        public string ModifierTitle => Product?.Modifiers?.FirstOrDefault()?.Title ?? "#####";
        public List<ModifierItemModel> ModifierItems => Product?.Modifiers?.FirstOrDefault()?.ModifierItems ?? new List<ModifierItemModel>();
        public bool HasModifiers => Product?.Modifiers != null && Product.Modifiers.Count > 0 && 
                                   Product.Modifiers.FirstOrDefault()?.ModifierItems != null && 
                                   Product.Modifiers.FirstOrDefault().ModifierItems.Count > 0;
        
        public bool HasSelectedModifiers => SelectedModifierDetails != null && SelectedModifierDetails.Count > 0;
        
        public List<ModifierDetailModel> StructuredModifierDetails
        {
            get
            {
                var details = new List<ModifierDetailModel>();
                if (Product?.Modifiers != null)
                {
                    foreach (var group in Product.Modifiers)
                    {
                        // Check for multiple selections first (new format)
                        if (SelectedModifiersMultiple.TryGetValue(group.Id, out var selectedNames) && selectedNames != null && selectedNames.Count > 0)
                        {
                            foreach (var selectedName in selectedNames)
                            {
                                var selected = group.ModifierItems?.FirstOrDefault(item => item.ItemName == selectedName);
                                if (selected != null)
                                {
                                    details.Add(new ModifierDetailModel($"{group.Title}: {selected.ItemName}", selected.ItemPrice));
                                    
                                    // If this specific modifier item has nested modifier details, add them indented
                                    if (_nestedModifierDetails.TryGetValue(selected.ItemName, out var nestedList) && nestedList != null && nestedList.Count > 0)
                                    {
                                        foreach (var nested in nestedList)
                                        {
                                            // Parse nested modifier details to extract price
                                            var parts = nested.Split('$');
                                            if (parts.Length >= 2)
                                            {
                                                var nestedName = parts[0].Trim();
                                                if (decimal.TryParse(parts[1].Trim(), out decimal nestedPrice))
                                                {
                                                    details.Add(new ModifierDetailModel($"↳ {nestedName}", nestedPrice, true, "    "));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // Fallback to single selection (backward compatibility)
                        else if (SelectedModifiers.TryGetValue(group.Id, out var selectedName))
                        {
                            var selected = group.ModifierItems?.FirstOrDefault(item => item.ItemName == selectedName);
                            if (selected != null)
                            {
                                details.Add(new ModifierDetailModel($"{group.Title}: {selected.ItemName}", selected.ItemPrice));
                                // If this specific modifier item has nested modifier details, add them indented
                                if (_nestedModifierDetails.TryGetValue(selected.ItemName, out var nestedList) && nestedList != null && nestedList.Count > 0)
                                {
                                    foreach (var nested in nestedList)
                                    {
                                        // Parse nested modifier details to extract price
                                        var parts = nested.Split('$');
                                        if (parts.Length >= 2)
                                        {
                                            var nestedName = parts[0].Trim();
                                            if (decimal.TryParse(parts[1].Trim(), out decimal nestedPrice))
                                            {
                                                details.Add(new ModifierDetailModel($"↳ {nestedName}", nestedPrice, true, "    "));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return details;
            }
        }
        
        public double DialogHeight => HasModifiers ? 530.0 : 400.0;
        
        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { if (value < 1) value = 1; _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(FinalPrice)); OnPropertyChanged(nameof(DiscountAmount)); OnPropertyChanged(nameof(TotalModifierPrice)); }
        }
        private decimal _discountPercent;
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set { _discountPercent = value; OnPropertyChanged(); OnPropertyChanged(nameof(FinalPrice)); OnPropertyChanged(nameof(DiscountAmount)); }
        }
        
        private bool _isDiscount10Selected;
        public bool IsDiscount10Selected
        {
            get => _isDiscount10Selected;
            set { _isDiscount10Selected = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDiscount10SelectedString)); }
        }
        
        public string IsDiscount10SelectedString => _isDiscount10Selected ? "True" : "False";
        
        private bool _isDiscount20Selected;
        public bool IsDiscount20Selected
        {
            get => _isDiscount20Selected;
            set { _isDiscount20Selected = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDiscount20SelectedString)); }
        }
        
        public string IsDiscount20SelectedString => _isDiscount20Selected ? "True" : "False";
        public string Note { get; set; }
        public ICommand IncrementQtyCommand { get; }
        public ICommand DecrementQtyCommand { get; }
        public ICommand Discount10Command { get; }
        public ICommand Discount20Command { get; }
        public ICommand AddItemCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SelectSizeCommand { get; }
        public ICommand OpenModifiersDialogCommand { get; }
        public string Size => SelectedSize;
        public decimal UnitPrice => BasePrice + SizePriceAdjustment;
        public decimal DiscountAmount => Math.Round((BasePrice + BaseModifierPrice) * Quantity * DiscountPercent / 100, 2);


        public decimal BaseModifierPrice
        {
            get
            {
                decimal total = 0;
                
                // Calculate price from main modifiers
                if (Product?.Modifiers != null)
                {
                    foreach (var group in Product.Modifiers)
                    {
                        // Check for multiple selections first (new format)
                        if (SelectedModifiersMultiple.TryGetValue(group.Id, out var selectedNames) && selectedNames != null && selectedNames.Count > 0)
                        {
                            foreach (var selectedName in selectedNames)
                            {
                                var selected = group.ModifierItems?.FirstOrDefault(item => item.ItemName == selectedName);
                                if (selected != null)
                                {
                                    total += selected.ItemPrice;
                                }
                            }
                        }
                        // Fallback to single selection (backward compatibility)
                        else if (SelectedModifiers.TryGetValue(group.Id, out var selectedName))
                        {
                            var selected = group.ModifierItems?.FirstOrDefault(item => item.ItemName == selectedName);
                            if (selected != null)
                            {
                                total += selected.ItemPrice;
                            }
                        }
                    }
                }
                
                // Calculate price from nested modifiers
                if (_nestedModifierDetails != null)
                {
                    foreach (var nestedList in _nestedModifierDetails.Values)
                    {
                        if (nestedList != null)
                        {
                            foreach (var nested in nestedList)
                            {
                                // Parse nested modifier details to extract price
                                var parts = nested.Split('$');
                                if (parts.Length >= 2)
                                {
                                    if (decimal.TryParse(parts[1].Trim(), out decimal nestedPrice))
                                    {
                                        total += nestedPrice;
                                    }
                                }
                            }
                        }
                    }
                }
                
                return total;
            }
        }
        
        public decimal TotalModifierPrice => BaseModifierPrice * Quantity;
        

        public decimal FinalPrice => Math.Round((BasePrice + TotalModifierPrice) * Quantity * (1 - DiscountPercent / 100), 2);
        public event Action<AddItemDialogViewModel> ItemAdded;
        public event Action DialogClosed;
        
        public AddItemDialogViewModel(string productName, decimal basePrice, ProductItemModel product = null)
        {
            ProductName = productName;
            BasePrice = basePrice;
            Product = product;
            
            IncrementQtyCommand = new RelayCommand(() => { Quantity++; });
            DecrementQtyCommand = new RelayCommand(() => { if (Quantity > 1) Quantity--; });
            Discount10Command = new RelayCommand(() => 
            { 
                if (IsDiscount10Selected)
                {
                    // If already selected, deselect it
                    IsDiscount10Selected = false;
                    DiscountPercent = 0;
                }
                else
                {
                    // Select 10% discount and deselect 20%
                    IsDiscount10Selected = true;
                    IsDiscount20Selected = false;
                    DiscountPercent = 10;
                }
            });
            Discount20Command = new RelayCommand(() => 
            { 
                if (IsDiscount20Selected)
                {
                    // If already selected, deselect it
                    IsDiscount20Selected = false;
                    DiscountPercent = 0;
                }
                else
                {
                    // Select 20% discount and deselect 10%
                    IsDiscount20Selected = true;
                    IsDiscount10Selected = false;
                    DiscountPercent = 20;
                }
            });
            AddItemCommand = new RelayCommand(() => { ItemAdded?.Invoke(this); DialogClosed?.Invoke(); });
            CloseCommand = new RelayCommand(() => { DialogClosed?.Invoke(); });
            SelectSizeCommand = new RelayCommand<string>(OnSelectSize);
            OpenModifiersDialogCommand = new RelayCommand(OpenModifiersDialog);
            
            // Set initial size based on available modifier items after commands are initialized
            InitializeSelectedSize();
        }
        
        public AddItemDialogViewModel(string productName, decimal basePrice, ProductItemModel product = null, Dictionary<int, string> selectedModifiers = null)
            : this(productName, basePrice, product)
        {
            if (selectedModifiers != null)
                SelectedModifiers = new Dictionary<int, string>(selectedModifiers);
        }
        
        public AddItemDialogViewModel(string productName, decimal basePrice, ProductItemModel product = null, Dictionary<int, string> selectedModifiers = null, Dictionary<string, List<string>> nestedModifierDetails = null)
            : this(productName, basePrice, product, selectedModifiers)
        {
            if (nestedModifierDetails != null)
                _nestedModifierDetails = new Dictionary<string, List<string>>(nestedModifierDetails);
        }

        // New constructor for multiple selections
        public AddItemDialogViewModel(string productName, decimal basePrice, ProductItemModel product = null, Dictionary<int, List<string>> selectedModifiersMultiple = null, Dictionary<string, List<string>> nestedModifierDetails = null)
            : this(productName, basePrice, product)
        {
            if (selectedModifiersMultiple != null)
                SelectedModifiersMultiple = new Dictionary<int, List<string>>(selectedModifiersMultiple);
            if (nestedModifierDetails != null)
                _nestedModifierDetails = new Dictionary<string, List<string>>(nestedModifierDetails);
        }
        
        private void InitializeSelectedSize()
        {
            try
            {
                if (Product?.Modifiers == null || Product.Modifiers.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No modifiers found. Using default size.");
                    SelectedSize = "Small";
                    return;
                }
                var firstModifier = Product.Modifiers.FirstOrDefault();
                if (firstModifier == null || firstModifier.ModifierItems == null || firstModifier.ModifierItems.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No modifier items found. Using default size.");
                    SelectedSize = "Small";
                    return;
                }
                var firstItem = firstModifier.ModifierItems.FirstOrDefault(item => !string.IsNullOrEmpty(item?.ItemName) && item.ItemName != "#####");
                if (firstItem != null)
                {
                    SelectedSize = firstItem.ItemName;
                    UpdatePriceAdjustment();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No valid modifier item name found. Using default size.");
                    SelectedSize = "Small";
                }
            }
            catch (Exception ex)
            {
                // Fallback to default size if there's any error
                SelectedSize = "Small";
                System.Diagnostics.Debug.WriteLine($"Error initializing selected size: {ex.Message}");
            }
        }
        
        private void UpdatePriceAdjustment()
        {
            try
            {
                var selectedItem = ModifierItems?.FirstOrDefault(item => item?.ItemName == SelectedSize);
                if (selectedItem != null)
                {
                    SizePriceAdjustment = selectedItem.ItemPrice;
                    System.Diagnostics.Debug.WriteLine($"Selected size: {SelectedSize}, Price adjustment: {SizePriceAdjustment}");
                }
                else
                {
                    SizePriceAdjustment = 0;
                    System.Diagnostics.Debug.WriteLine($"No matching item found for size: {SelectedSize}");
                }
            }
            catch (Exception ex)
            {
                // Fallback to 0 if there's any error
                SizePriceAdjustment = 0;
                System.Diagnostics.Debug.WriteLine($"Error updating price adjustment: {ex.Message}");
            }
        }
        private void OnSelectSize(string size)
        {
            SelectedSize = size;
        }

        // Store all selected modifiers from AddModifiersDialog
        private Dictionary<int, string> _selectedModifiers = new Dictionary<int, string>();
        public Dictionary<int, string> SelectedModifiers
        {
            get => _selectedModifiers;
            set { _selectedModifiers = value; OnPropertyChanged(nameof(SelectedModifiers)); OnPropertyChanged(nameof(SelectedModifierDetails)); OnPropertyChanged(nameof(StructuredModifierDetails)); OnPropertyChanged(nameof(BaseModifierPrice)); OnPropertyChanged(nameof(TotalModifierPrice)); OnPropertyChanged(nameof(FinalPrice)); OnPropertyChanged(nameof(DiscountAmount)); }
        }

        // Store multiple selected modifiers from AddModifiersDialog (new format)
        private Dictionary<int, List<string>> _selectedModifiersMultiple = new Dictionary<int, List<string>>();
        public Dictionary<int, List<string>> SelectedModifiersMultiple
        {
            get => _selectedModifiersMultiple;
            set { _selectedModifiersMultiple = value; OnPropertyChanged(nameof(SelectedModifiersMultiple)); OnPropertyChanged(nameof(SelectedModifierDetails)); OnPropertyChanged(nameof(StructuredModifierDetails)); OnPropertyChanged(nameof(BaseModifierPrice)); OnPropertyChanged(nameof(TotalModifierPrice)); OnPropertyChanged(nameof(FinalPrice)); OnPropertyChanged(nameof(DiscountAmount)); }
        }

        // Store nested modifier selections: parentModifierItemName -> List<string> (details)
        private Dictionary<string, List<string>> _nestedModifierDetails = new Dictionary<string, List<string>>();
        
        public Dictionary<string, List<string>> NestedModifierDetails
        {
            get => _nestedModifierDetails;
            set { _nestedModifierDetails = value; OnPropertyChanged(nameof(NestedModifierDetails)); }
        }
        
        public void SetNestedModifierDetails(int groupId, List<string> details)
        {
            // This method is kept for backward compatibility but should not be used
            // Nested modifier details should be set using the parent modifier item name
            OnPropertyChanged(nameof(SelectedModifierDetails));
        }

        private async void OpenModifiersDialog()
        {
            try
            {
                // Pass the current selections to pre-populate the dialog
                var dialogVm = new AddModifiersDialogViewModel(
                    Product?.Modifiers ?? new List<ModifierModel>(), 
                    SelectedModifiersMultiple, 
                    _nestedModifierDetails
                );
                var dialog = new POS_UI.View.AddModifiersDialog { DataContext = dialogVm };
                dialogVm.ModifierSaved += (selectedModifiers) =>
                {
                    if (selectedModifiers != null && selectedModifiers.Count > 0)
                    {
                        // Convert the backward compatible format to the new multiple selection format
                        var multipleSelections = new Dictionary<int, List<string>>();
                        foreach (var kvp in selectedModifiers)
                        {
                            var itemNames = kvp.Value.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                            multipleSelections[kvp.Key] = new List<string>(itemNames);
                        }
                        
                        SelectedModifiersMultiple = multipleSelections;
                        
                        // For backward compatibility, also set the old format with the first selected item
                        var firstSelected = selectedModifiers.Values.FirstOrDefault();
                        if (!string.IsNullOrEmpty(firstSelected))
                        {
                            var firstItemName = firstSelected.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                            if (!string.IsNullOrEmpty(firstItemName))
                            {
                                SelectedSize = firstItemName;
                            UpdatePriceAdjustment();
                            }
                        }
                        OnPropertyChanged(nameof(SelectedModifierDetails));
                        OnPropertyChanged(nameof(StructuredModifierDetails));
                        OnPropertyChanged(nameof(TotalModifierPrice));
                        OnPropertyChanged(nameof(FinalPrice));
                    }
                };
                
                // Handle nested modifier details updates
                dialogVm.ModifierSavedWithNested += (selectedModifiers, nestedModifierDetails) =>
                {
                    // Update nested modifier details regardless of whether main modifiers are selected
                    if (nestedModifierDetails != null)
                    {
                        _nestedModifierDetails = new Dictionary<string, List<string>>(nestedModifierDetails);
                    }
                    
                    if (selectedModifiers != null && selectedModifiers.Count > 0)
                    {
                        // Convert the backward compatible format to the new multiple selection format
                        var multipleSelections = new Dictionary<int, List<string>>();
                        foreach (var kvp in selectedModifiers)
                        {
                            var itemNames = kvp.Value.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                            multipleSelections[kvp.Key] = new List<string>(itemNames);
                        }
                        
                        SelectedModifiersMultiple = multipleSelections;
                        
                        // For backward compatibility, also set the old format with the first selected item
                        var firstSelected = selectedModifiers.Values.FirstOrDefault();
                        if (!string.IsNullOrEmpty(firstSelected))
                        {
                            var firstItemName = firstSelected.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                            if (!string.IsNullOrEmpty(firstItemName))
                            {
                                SelectedSize = firstItemName;
                                UpdatePriceAdjustment();
                            }
                        }
                    }
                    else
                    {
                        // Clear selections if no modifiers are selected
                        SelectedModifiersMultiple = new Dictionary<int, List<string>>();
                    }
                    
                    OnPropertyChanged(nameof(SelectedModifierDetails));
                    OnPropertyChanged(nameof(StructuredModifierDetails));
                    OnPropertyChanged(nameof(TotalModifierPrice));
                    OnPropertyChanged(nameof(FinalPrice));
                };
                
                // Handle nested modifiers with pre-selection
                dialogVm.NestedModifierRequested += async (modifierItem) =>
                {
                    try
                    {
                        // Get previously selected nested modifiers for this specific modifier item
                        var preSelectedNestedModifiers = new Dictionary<int, List<string>>();
                        if (_nestedModifierDetails.TryGetValue(modifierItem.ItemName, out var nestedDetails))
                        {
                            // Convert the nested details back to the format expected by NestedModifiersDialogViewModel
                            foreach (var group in modifierItem.NestedModifiers)
                            {
                                var selectedItems = new List<string>();
                                foreach (var detail in nestedDetails)
                                {
                                    if (detail.StartsWith($"{group.Title}:"))
                                    {
                                        var itemName = detail.Substring(detail.IndexOf(':') + 1).Split('$')[0].Trim();
                                        selectedItems.Add(itemName);
                                    }
                                }
                                if (selectedItems.Count > 0)
                                {
                                    preSelectedNestedModifiers[group.Id] = selectedItems;
                                }
                            }
                        }
                        
                        var nestedDialogVm = new NestedModifiersDialogViewModel(
                            modifierItem.NestedModifiers, 
                            modifierItem.ItemName,
                            preSelectedNestedModifiers
                        );
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
                            var formattedNestedDetails = new List<string>();
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
                                            formattedNestedDetails.Add($"{group.Title}: {selected.ItemName}   ${selected.ItemPrice:0.00}");
                                        }
                                    }
                                }
                            }
                            
                            // Find the parent groupId for this modifierItem
                            var parentGroup = dialogVm.ModifierGroups.FirstOrDefault(g => g.ModifierItems != null && g.ModifierItems.Contains(modifierItem));
                            if (parentGroup != null)
                            {
                                dialogVm.SetNestedModifierDetails(parentGroup.Id, formattedNestedDetails, modifierItem);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error opening nested modifiers dialog: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                };
                
                dialogVm.DialogClosed += () => MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
                await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "ModifiersDialogHost");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening AddModifiersDialog: {ex.Message}\n\n{ex.StackTrace}", "Dialog Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Updated summary property to include nested modifier details and multiple selections
        public List<string> SelectedModifierDetails
        {
            get
            {
                var details = new List<string>();
                if (Product?.Modifiers != null)
                {
                    foreach (var group in Product.Modifiers)
                    {
                        // Check for multiple selections first (new format)
                        if (SelectedModifiersMultiple.TryGetValue(group.Id, out var selectedNames) && selectedNames != null && selectedNames.Count > 0)
                        {
                            foreach (var selectedName in selectedNames)
                            {
                                var selected = group.ModifierItems?.FirstOrDefault(item => item.ItemName == selectedName);
                                if (selected != null)
                                {
                                    var line = $"{group.Title}: {selected.ItemName}  \t ${selected.ItemPrice:0.00}";
                                    details.Add(line);
                                    
                                    // If this specific modifier item has nested modifier details, add them indented
                                    if (_nestedModifierDetails.TryGetValue(selected.ItemName, out var nestedList) && nestedList != null && nestedList.Count > 0)
                                    {
                                        foreach (var nested in nestedList)
                                        {
                                            details.Add($"\t↳ {nested}");
                                        }
                                    }
                                }
                            }
                        }
                        // Fallback to single selection (backward compatibility)
                        else if (SelectedModifiers.TryGetValue(group.Id, out var selectedName))
                        {
                            var selected = group.ModifierItems?.FirstOrDefault(item => item.ItemName == selectedName);
                            if (selected != null)
                            {
                                var line = $"{group.Title}: {selected.ItemName}  \t ${selected.ItemPrice:0.00}";
                                details.Add(line);
                                // If this specific modifier item has nested modifier details, add them indented
                                if (_nestedModifierDetails.TryGetValue(selected.ItemName, out var nestedList) && nestedList != null && nestedList.Count > 0)
                                {
                                    foreach (var nested in nestedList)
                                    {
                                        details.Add($"\t↳ {nested}");
                                    }
                                }
                            }
                        }
                    }
                }
                return details;
            }
        }
    }
} 