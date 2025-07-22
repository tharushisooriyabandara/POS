using System.ComponentModel;

namespace POS_UI.Models
{
    public class CategoryModel : INotifyPropertyChanged
    {
        private string _categoryName;
        private int _quantity;

        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(nameof(CategoryName)); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(nameof(Quantity)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 