// EShift123.Models/TransportUnit.cs
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShift123.Models
{
    public class TransportUnit
    {
        [Key]
        public int TransportUnitId { get; set; }

        [Required(ErrorMessage = "Unit number is required.")]
        [StringLength(50, ErrorMessage = "Unit number cannot exceed 50 characters.")]
        [Display(Name = "Unit Number")]
        public string UnitNumber { get; set; }

        [Required(ErrorMessage = "Lorry selection is required.")]
        [Display(Name = "Lorry")]
        public int LorryId { get; set; }
        [ForeignKey("LorryId")]
        public Lorry Lorry { get; set; }

        [Required(ErrorMessage = "Driver selection is required.")]
        [Display(Name = "Driver")]
        public int DriverId { get; set; }
        [ForeignKey("DriverId")]
        public Driver Driver { get; set; }

        [Display(Name = "Assistant")]
        public int? AssistantId { get; set; }
        [ForeignKey("AssistantId")]
        public Assistant Assistant { get; set; }

        [Required(ErrorMessage = "Container selection is required.")]
        [Display(Name = "Container")]
        public int ContainerId { get; set; }
        [ForeignKey("ContainerId")]
        public Container Container { get; set; }

        [ValidateNever]
        public ICollection<Load> Loads { get; set; } = new List<Load>();
    }
}