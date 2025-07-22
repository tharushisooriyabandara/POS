namespace POS_UI.Models
{
    public enum TableStatus
    {
        Available,
        Reserved,
        Drafted,
        Served
    }

    public class TableModel : System.ComponentModel.INotifyPropertyChanged
    {
        public int TableNumber { get; set; }
        private TableStatus _status;
        public TableStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }
        public decimal Amount { get; set; }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
} 