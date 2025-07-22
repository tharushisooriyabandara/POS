using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;
using System.Collections.ObjectModel;
using POS_UI.ViewModels;

namespace POS_UI.View
{
    /// <summary>
    /// Interaction logic for OrderDetailsControl.xaml
    /// </summary>
    public partial class TableOrderDetails : UserControl, INotifyPropertyChanged
    {
        public TableOrderDetails()
        {
            InitializeComponent();
            Items = new ObservableCollection<Models.OrderItem>();
        }

        public ICommand AddNoteCommand => new RelayCommand(AddNote);
        public ICommand OpenCouponDialogCommand => new RelayCommand(OpenCouponDialog);
        public ICommand OpenDiscountDialogCommand => new RelayCommand(OpenDiscountDialog);
        public ICommand RemoveCouponCommand => new RelayCommand(RemoveCoupon);
        public ICommand UpdateOrderCommand => new RelayCommand(UpdateOrder);
        public event Action<Models.OrderModel> UpdateOrderRequested;

        private async void AddNote()
        {
            try
            {
                var dialogVm = new NoteDialogViewModel(Note);
                var dialog = new NoteDialog { DataContext = dialogVm };
                
                dialogVm.NoteSaved += (note) =>
                {
                    Note = note;
                    OnPropertyChanged(nameof(CanAddNote));
                };
                
                dialogVm.DialogClosed += () => DialogHost.CloseDialogCommand.Execute(null, null);
                await DialogHost.Show(dialog, "RootDialogHost");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding note: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OpenCouponDialog()
        {
            try
            {
                var dialog = new SetCouponDialog();
                var result = await DialogHost.Show(dialog, "RootDialogHost");
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

        private async void OpenDiscountDialog()
        {
            try
            {
                var dialog = new SetDiscountDialog();
                var result = await DialogHost.Show(dialog, "RootDialogHost");
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

        private void UpdateOrder()
        {
            UpdateOrderRequested?.Invoke(this.DataContext as Models.OrderModel);
        }

        // Properties
        private string _note;
        public string Note
        {
            get => _note;
            set
            {
                _note = value;
                OnPropertyChanged(nameof(Note));
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
                OnPropertyChanged(nameof(Discount));
                OnPropertyChanged(nameof(SubTotal));
            }
        }

        private decimal _discountPercent;
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                _discountPercent = value;
                OnPropertyChanged(nameof(DiscountPercent));
                OnPropertyChanged(nameof(DiscountDescription));
            }
        }
        public string DiscountDescription => DiscountPercent > 0 ? $"Discount ({DiscountPercent}%)" : "Discount";

        private string _couponCode;
        public string CouponCode
        {
            get => _couponCode;
            set 
            { 
                _couponCode = value; 
                OnPropertyChanged(nameof(CouponCode)); 
            }
        }

        private string _couponDescription;
        public string CouponDescription
        {
            get => _couponDescription;
            set 
            { 
                _couponDescription = value; 
                OnPropertyChanged(nameof(CouponDescription)); 
            }
        }

        private decimal _couponDiscount;
        public decimal CouponDiscount
        {
            get => _couponDiscount;
            set 
            { 
                _couponDiscount = value; 
                OnPropertyChanged(nameof(CouponDiscount)); 
                OnPropertyChanged(nameof(SubTotal)); 
            }
        }

        public bool HasCoupon => !string.IsNullOrWhiteSpace(CouponCode);
        public bool CanAddCoupon => !HasCoupon;

        private ObservableCollection<Models.OrderItem> _items;
        public ObservableCollection<Models.OrderItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(SubTotal));
            }
        }

        public decimal Total => Items?.Sum(i => i.Total) ?? 0;
        public decimal SubTotal => Total - Discount - CouponDiscount;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
