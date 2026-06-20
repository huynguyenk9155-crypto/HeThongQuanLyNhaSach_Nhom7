using System.ComponentModel.DataAnnotations.Schema;

namespace Tuan6.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty; // "vnpay", "momo", "cod", etc.
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public string Status { get; set; } = "pending"; // "pending", "completed", "failed", "cancelled"
        public string TransactionCode { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public Order? Order { get; set; }
    }
}
