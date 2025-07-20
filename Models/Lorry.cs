// EShift123.Models/Lorry.cs
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace EShift123.Models
{
    public class Lorry
    {
        [Key]
        public int LorryId { get; set; }

        [Required(ErrorMessage = "Number plate is required.")]
        [StringLength(50, ErrorMessage = "Number plate cannot exceed 50 characters.")]
        [Display(Name = "Number Plate")]
        public string NumberPlate { get; set; }

        [Required(ErrorMessage = "Model is required.")]
        [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters.")]
        public string Model { get; set; }

        [ValidateNever]
        public ICollection<TransportUnit> TransportUnits { get; set; } = new List<TransportUnit>();
    }
}