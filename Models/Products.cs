using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasiCash.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Required]
        public int QuantityInStock { get; set; }

        public int LowStockLevel { get; set; } = 5;

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        public ApplicationUser? Owner { get; set; }
    }
}