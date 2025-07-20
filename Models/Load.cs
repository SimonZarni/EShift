using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShift123.Models
{
    public class Load
    {
        [Key]
        public int LoadId { get; set; }

        [Required(ErrorMessage = "Load number is required.")]
        [StringLength(50, ErrorMessage = "Load number cannot exceed 50 characters.")]
        [Display(Name = "Load Number")]
        public string LoadNumber { get; set; }

        [Required(ErrorMessage = "Job selection is required.")]
        [Display(Name = "Job")]
        public int JobId { get; set; }
        [ForeignKey("JobId")]
        public Job Job { get; set; }

        [Display(Name = "Transport Unit")]
        public int? TransportUnitId { get; set; }
        [ForeignKey("TransportUnitId")]
        public TransportUnit TransportUnit { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string Description { get; set; }

        [Range(0.01, 10000.0, ErrorMessage = "Weight must be between 0.01 and 10000.0 kg.")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "Pickup date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Pickup Date")]
        public DateTime? PickupDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Delivery Date")]
        public DateTime? DeliveryDate { get; set; }

        [Required(ErrorMessage = "Load status is required.")]
        [Display(Name = "Load Status")]
        public LoadStatus Status { get; set; } = LoadStatus.Pending;

        public ICollection<LoadProduct> LoadProducts { get; set; } = new List<LoadProduct>();
    }

    public enum LoadStatus
    {
        Pending,
        Assigned,
        PickedUp,
        Delivered,
        Cancelled
    }
}