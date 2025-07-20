// EShift123.Models/Assistant.cs
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace EShift123.Models
{
    public class Assistant
    {
        [Key]
        public int AssistantId { get; set; } 

        [Required(ErrorMessage = "Assistant name is required.")]
        [StringLength(100, ErrorMessage = "Assistant name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string Phone { get; set; }

        [ValidateNever]
        public ICollection<TransportUnit> TransportUnits { get; set; } = new List<TransportUnit>();
    }
}