using POS_UI.Models;

namespace POS_UI.ViewModels
{
    public class KitchenOrderDetailsDialogViewModel : BaseViewModel
    {
        private OrderModel _order;
        public OrderModel Order
        {
            get => _order;
            set
            {
                _order = value;
                OnPropertyChanged();
            }
        }

        public KitchenOrderDetailsDialogViewModel() { }
        public KitchenOrderDetailsDialogViewModel(OrderModel order)
        {
            Order = order;
        }
    }
} 