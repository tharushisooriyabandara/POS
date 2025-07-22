using System.ComponentModel;

namespace POS_UI.Models
{
    public class ProductItemModel : INotifyPropertyChanged
    {
        private int _id;
        private string _itemName;
        private decimal _price;
        private string _category;
        private string _size;
        private string _imageUrl;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(nameof(ItemName)); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(nameof(Price)); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }

        public string Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(nameof(Size)); }
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set { _imageUrl = value; OnPropertyChanged(nameof(ImageUrl)); }
        }

        public List<ModifierModel> Modifiers { get; set; } = new List<ModifierModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 