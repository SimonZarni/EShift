using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EShift123.Models.ViewModels
{
    // ViewModel for the Job Request Form, encapsulating Job, Load, and Product details.
    public class JobRequestViewModel
    {
        // CustomerId is directly on the ViewModel for easier access during job creation.
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Start location is required.")]
        [StringLength(100, ErrorMessage = "Start location cannot exceed 100 characters.")]
        [Display(Name = "Pick-up Location")]
        public string StartLocation { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        [StringLength(100, ErrorMessage = "Destination cannot exceed 100 characters.")]
        [Display(Name = "Delivery Location")]
        public string Destination { get; set; }

        [Required(ErrorMessage = "Job date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Requested Job Date")]
        public DateTime JobDate { get; set; } = DateTime.Today;

        // Collection of loads associated with this job.
        // MinLength(1) ensures at least one load is provided.
        [MinLength(1, ErrorMessage = "At least one load is required for a job.")]
        public List<LoadInputModel> Loads { get; set; } = new List<LoadInputModel>();
    }

    // ViewModel for collecting Load details from the form.
    public class LoadInputModel
    {
        [Required(ErrorMessage = "Load description is required.")]
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        [Display(Name = "Load Description")]
        public string Description { get; set; }

        [Range(0.01, 10000.0, ErrorMessage = "Load weight must be between 0.01 and 10000.0 kg.")]
        [Display(Name = "Total Load Weight (Kg)")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "Pickup date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Pickup Date")]
        public DateTime? PickupDate { get; set; } = DateTime.Today;

        // Collection of products within this specific load.
        // MinLength(1) ensures at least one product is provided for each load.
        [MinLength(1, ErrorMessage = "At least one product is required for each load.")]
        public List<ProductInputModel> Products { get; set; } = new List<ProductInputModel>();
    }

    // ViewModel for collecting Product details from the form.
    public class ProductInputModel
    {
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
