// EShift123.Models/Customer.cs
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace EShift123.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; } 

        [StringLength(450, ErrorMessage = "User ID cannot exceed 450 characters.")]
        public string UserId { get; set; } // Foreign key to AspNetUsers

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Creation date is required.")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ValidateNever]
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}