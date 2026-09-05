using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasiCash.Models
{
    public class CreditSale
    {
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }

        public Sale? Sale { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountOwed { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidDate { get; set; }
    }
}