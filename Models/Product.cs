// EShift123.Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for [ForeignKey]
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Required for [ValidateNever]

namespace EShift123.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        // Foreign Key to Customer
        [Required(ErrorMessage = "Customer ID is required for the product.")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        [ValidateNever] // Prevents validation loops for the navigation property
        public Customer Customer { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public string Category { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Weight is required.")]
        [Range(0.01, 1000.0, ErrorMessage = "Weight must be between 0.01 and 1000.0 kg.")]
        [Display(Name = "Weight (Kg)")]
        public decimal WeightKg { get; set; }

        [Display(Name = "Is Valid?")] // Display name for UI
        public bool IsValid { get; set; } = false; // Default to false

        public ICollection<LoadProduct> LoadProducts { get; set; } = new List<LoadProduct>();
    }
}
