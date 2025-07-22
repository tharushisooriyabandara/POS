using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace POS_UI.Models
{
    public enum OrderType
    {
        DineIn,
        TakeAway,
        Delivery
    }

    public enum OrderStatus
    {
        Draft,
        Reserved,
        Served,
    }

    public class OrderModel : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public OrderType OrderType { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        // Customer Information
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        // Delivery/Takeaway Information
        public DateTime? ScheduledTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryInstructions { get; set; }

        // Dine-in Information
        public int? TableNumber { get; set; }

        // Order Items
        private List<OrderItem> _items = new List<OrderItem>();
        public List<OrderItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Total));
            }
        }

        // Notes 
        public string OrderNotes { get; set; }

        // Payment Information
        public decimal Total => Items?.Sum(item => item.Total) ?? 0;
        public decimal DiscountAmount { get; set; }
        public string CouponCode { get; set; }
        public decimal CouponAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal Subtotal => Total - DiscountAmount - CouponAmount;
        public decimal SubTotal => Subtotal;
        public string DiscountDescription => DiscountPercentage > 0 ? $"Discount ({DiscountPercentage}%)" : "Discount";
        public decimal Discount => -DiscountAmount;

        // Payment Status
        public bool IsPaid { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }

        // Platform
        public string Platform { get; set; }

        public void AddItem(OrderItem item)
        {
            Items.Add(item);
            OnPropertyChanged(nameof(Items));
            
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Subtotal));
        }

        public void RemoveItem(OrderItem item)
        {
            Items.Remove(item);
            OnPropertyChanged(nameof(Items));
           
            OnPropertyChanged(nameof(Total));
             OnPropertyChanged(nameof(Subtotal));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 