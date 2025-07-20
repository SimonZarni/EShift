// EShift123.Models/Driver.cs
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace EShift123.Models
{
    public class Driver
    {
        [Key]
        public int DriverId { get; set; }

        [Required(ErrorMessage = "Driver name is required.")]
        [StringLength(100, ErrorMessage = "Driver name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "License number is required.")]
        [StringLength(50, ErrorMessage = "License number cannot exceed 50 characters.")]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string Phone { get; set; }

        [ValidateNever]
        public ICollection<TransportUnit> TransportUnits { get; set; } = new List<TransportUnit>();
    }
}