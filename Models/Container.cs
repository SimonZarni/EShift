// EShift123.Models/Container.cs
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace EShift123.Models
{
    public class Container
    {
        [Key]
        public int ContainerId { get; set; }

        [Required(ErrorMessage = "Container number is required.")]
        [StringLength(50, ErrorMessage = "Container number cannot exceed 50 characters.")]
        [Display(Name = "Container Number")]
        public string ContainerNumber { get; set; }

        [ValidateNever]
        public ICollection<TransportUnit> TransportUnits { get; set; } = new List<TransportUnit>();
    }
}