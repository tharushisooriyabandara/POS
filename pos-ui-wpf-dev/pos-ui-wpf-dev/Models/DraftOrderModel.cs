using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace POS_UI.Models
{
    public class DraftOrderModel : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string OrderType { get; set; } // Take Away, Delivery, Dine In
        public string TableName { get; set; } // null for non-table orders
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public string ElapsedTimeText
        {
            get
            {
                var span = DateTime.Now - CreatedAt;
                if (span.TotalMinutes < 1)
                    return "Just now";
                if (span.TotalMinutes < 60)
                    return $"{(int)span.TotalMinutes} mins ago";
                if (span.TotalHours < 24)
                    return $"{(int)span.TotalHours} hours ago";
                return $"{(int)span.TotalDays} days ago";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnElapsedTimeChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ElapsedTimeText)));
        }
    }
} 