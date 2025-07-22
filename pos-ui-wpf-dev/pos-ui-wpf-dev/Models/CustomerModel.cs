namespace POS_UI.Models
{
    public class CustomerModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public int CustomerId { get; set; }
        public string Initials => string.Join("", Name?.Split(' ').Select(n => n[0]));
    }
} 