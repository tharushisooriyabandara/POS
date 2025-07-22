using System;

namespace POS_UI.Models
{
    public class ModifierDetailModel
    {
        public string ModifierName { get; set; }
        public decimal Price { get; set; }
        public bool IsNested { get; set; }
        public string Indentation { get; set; } = "";

        public ModifierDetailModel(string modifierName, decimal price, bool isNested = false, string indentation = "")
        {
            ModifierName = modifierName;
            Price = price;
            IsNested = isNested;
            Indentation = indentation;
        }
    }
} 