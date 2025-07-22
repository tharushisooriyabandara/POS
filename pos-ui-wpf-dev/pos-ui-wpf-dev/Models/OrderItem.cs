using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq; // Added for FirstOrDefault

namespace POS_UI.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public decimal Price { get; set; }
        private decimal _discountPercent;
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (_discountPercent != value)
                {
                    _discountPercent = value;
                    OnPropertyChanged(nameof(DiscountPercent));
                    OnPropertyChanged(nameof(DiscountAmount));
                    OnPropertyChanged(nameof(Total));
                }
            }
        }
        public decimal DiscountAmount => Math.Round((Product?.Price ?? Price + TotalModifierPrice) * Quantity * DiscountPercent / 100, 2);
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Total));
                    OnPropertyChanged(nameof(DiscountAmount));
                }
            }
        }
        public string Notes { get; set; }
        public decimal Subtotal => (Product?.Price ?? Price) * Quantity - DiscountAmount;
        public decimal Total => (Product?.Price ?? Price + TotalModifierPrice) * Quantity - DiscountAmount;

        // New fields for ViewModel compatibility
        public ProductItemModel Product { get; set; }
        private string _note;
        public string Note
        {
            get => _note;
            set
            {
                if (_note != value)
                {
                    _note = value;
                    OnPropertyChanged(nameof(Note));
                }
            }
        }
        
        // Modifier details for display in order summary
        public Dictionary<int, List<string>> SelectedModifiers { get; set; } = new Dictionary<int, List<string>>();
        public Dictionary<string, List<string>> NestedModifierDetails { get; set; } = new Dictionary<string, List<string>>();
        
        // Check if the item has any modifiers
        public bool HasModifiers => (Product?.Modifiers != null && Product.Modifiers.Count > 0);
        
        // Calculate total price of all selected modifiers
        public decimal TotalModifierPrice
        {
            get
            {
                decimal total = 0;
                
                // Calculate price from main modifiers
                if (Product?.Modifiers != null)
                {
                    foreach (var group in Product.Modifiers)
                    {
                        if (SelectedModifiers.TryGetValue(group.Id, out var selectedNames) && selectedNames != null && selectedNames.Count > 0)
                        {
                            foreach (var selectedName in selectedNames)
                            {
                                var modifierItem = group.ModifierItems?.FirstOrDefault(item => item.ItemName == selectedName);
                                if (modifierItem != null)
                                {
                                    total += modifierItem.ItemPrice;
                                }
                            }
                        }
                    }
                }
                
                // Calculate price from nested modifiers
                if (NestedModifierDetails != null)
                {
                    foreach (var nestedList in NestedModifierDetails.Values)
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
        
        // Computed property to get formatted modifier details for display
        public List<ModifierDetailModel> ModifierDetailsForDisplay
        {
            get
            {
                var details = new List<ModifierDetailModel>();
                
                // Add main modifiers with their nested modifiers grouped together (same as AddItemDialog)
                if (SelectedModifiers != null)
                {
                    foreach (var kvp in SelectedModifiers)
                    {
                        if (kvp.Value != null && kvp.Value.Count > 0)
                        {
                            foreach (var modifierName in kvp.Value)
                            {
                                // Try to find the modifier group and item to get proper formatting
                                if (Product?.Modifiers != null)
                                {
                                    foreach (var group in Product.Modifiers)
                                    {
                                        if (group.Id == kvp.Key)
                                        {
                                            ModifierDetailModel mainModifier = null;
                                            
                                            // Check if the modifier name is already in "Group: Item" format
                                            if (modifierName.Contains(":"))
                                            {
                                                // Already formatted, extract price
                                                var parts = modifierName.Split(':');
                                                if (parts.Length >= 2)
                                                {
                                                    var groupTitle = parts[0].Trim();
                                                    var itemName = parts[1].Trim();
                                                    
                                                    // Try to find the price from the Product's modifiers
                                                    decimal itemPrice = 0;
                                                    var modifierItem = group.ModifierItems?.FirstOrDefault(item => item.ItemName == itemName);
                                                    if (modifierItem != null)
                                                    {
                                                        itemPrice = modifierItem.ItemPrice;
                                                    }
                                                    
                                                    mainModifier = new ModifierDetailModel($"{groupTitle}: {itemName}", itemPrice);
                                                }
                                            }
                                            else
                                            {
                                                // Not formatted, format it properly
                                                var modifierItem = group.ModifierItems?.FirstOrDefault(item => item.ItemName == modifierName);
                                                if (modifierItem != null)
                                                {
                                                    mainModifier = new ModifierDetailModel($"{group.Title}: {modifierItem.ItemName}", modifierItem.ItemPrice);
                                                }
                                                else
                                                {
                                                    // Fallback if item not found
                                                    mainModifier = new ModifierDetailModel($"{group.Title}: {modifierName}", 0);
                                                }
                                            }
                                            
                                            // Add the main modifier
                                            if (mainModifier != null)
                                            {
                                                details.Add(mainModifier);
                                                
                                                // Add nested modifiers for this specific modifier item
                                                if (NestedModifierDetails != null && NestedModifierDetails.TryGetValue(modifierName, out var nestedList))
                                                {
                                                    if (nestedList != null && nestedList.Count > 0)
                                                    {
                                                        foreach (var nestedDetail in nestedList)
                                                        {
                                                            // Parse nested modifier details to extract price
                                                            var parts = nestedDetail.Split('$');
                                                            if (parts.Length >= 2)
                                                            {
                                                                var nestedName = parts[0].Trim();
                                                                if (decimal.TryParse(parts[1].Trim(), out decimal nestedPrice))
                                                                {
                                                                    details.Add(new ModifierDetailModel($"↳ {nestedName}", nestedPrice, true, "    "));
                                                                }
                                                            }
                                                            else
                                                            {
                                                                details.Add(new ModifierDetailModel($"↳ {nestedDetail}", 0, true, "    "));
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    // Fallback if Product.Modifiers is null
                                    var mainModifier = new ModifierDetailModel(modifierName, 0);
                                    details.Add(mainModifier);
                                    
                                    // Add nested modifiers for this modifier item
                                    if (NestedModifierDetails != null && NestedModifierDetails.TryGetValue(modifierName, out var nestedList))
                                    {
                                        if (nestedList != null && nestedList.Count > 0)
                                        {
                                            foreach (var nestedDetail in nestedList)
                                            {
                                                var parts = nestedDetail.Split('$');
                                                if (parts.Length >= 2)
                                                {
                                                    var nestedName = parts[0].Trim();
                                                    if (decimal.TryParse(parts[1].Trim(), out decimal nestedPrice))
                                                    {
                                                            details.Add(new ModifierDetailModel($"↳ {nestedName}", nestedPrice, true, "    "));
                                                    }
                                                }
                                                else
                                                {
                                                    details.Add(new ModifierDetailModel($"↳ {nestedDetail}", 0, true, "    "));
                                                }
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 