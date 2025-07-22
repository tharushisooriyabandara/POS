using System;
using System.Linq;

namespace POS_UI.Models
{
    public class UserModel
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // e.g., "Admin", "Cashier"
        public string Pin { get; set; }
        public string FullName => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}";
        public string Initials => string.Join("", (FirstName + " " + LastName).Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(n => n[0])).ToUpper();
    }
} 