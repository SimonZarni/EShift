using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Required for SelectList

namespace EShift123.Models.ViewModels
{
    // ViewModel for editing an existing Load, including its products.
    public class LoadEditViewModel
    {
        [Required]
        public int LoadId { get; set; }

        [Required]
        public int JobId { get; set; }

        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Load number is required.")]
        [StringLength(50, ErrorMessage = "Load number cannot exceed 50 characters.")]
        [Display(Name = "Load Number")]
        public string LoadNumber { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        [Display(Name = "Load Description")]
        public string Description { get; set; }

        [Range(0.01, 10000.0, ErrorMessage = "Weight must be between 0.01 and 10000.0 kg.")]
        [Display(Name = "Total Load Weight (Kg)")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "Pickup date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Pickup Date")]
        public DateTime? PickupDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Delivery Date")]
        public DateTime? DeliveryDate { get; set; }

        [Required(ErrorMessage = "Load status is required.")]
        [Display(Name = "Load Status")]
        public LoadStatus Status { get; set; }

        // Property for Transport Unit Assignment
        [Display(Name = "Assigned Transport Unit")]
        public int? TransportUnitId { get; set; } // Nullable, as a load might not yet have a unit

        // SelectList for the dropdown of available Transport Units
        public SelectList TransportUnits { get; set; }

        [MinLength(1, ErrorMessage = "At least one product is required for each load.")]
        public List<ProductInputModel1> Products { get; set; } = new List<ProductInputModel1>();
    }

    public class ProductInputModel1
    {
        // ProductId is crucial for identifying existing products when passed between view and controller
        public int ProductId { get; set; } // 0 for new products, actual ID for existing ones

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public string Category { get; set; }

        [StringLength(500, ErrorMessage = "Product description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Product weight is required.")]
        [Range(0.01, 1000.0, ErrorMessage = "Unit weight must be between 0.01 and 1000.0 kg.")]
        [Display(Name = "Unit Weight (Kg)")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
