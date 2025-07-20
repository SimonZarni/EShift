// EShift123.Models/LoadProduct.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShift123.Models
{
    public class LoadProduct
    {
        [Key]
        public int LoadProductId { get; set; }

        [Required(ErrorMessage = "Load ID is required.")]
        public int LoadId { get; set; }
        [ForeignKey("LoadId")]
        public Load Load { get; set; }

        [Required(ErrorMessage = "Product ID is required.")]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}