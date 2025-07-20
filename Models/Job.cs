using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShift123.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [Required(ErrorMessage = "Customer ID is required.")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Required(ErrorMessage = "Start location is required.")]
        [StringLength(100, ErrorMessage = "Start location cannot exceed 100 characters.")]
        public string StartLocation { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        [StringLength(100, ErrorMessage = "Destination cannot exceed 100 characters.")]
        public string Destination { get; set; }

        [Required(ErrorMessage = "Job date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Job Date")]
        public DateTime JobDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Job status is required.")]
        [Display(Name = "Job Status")]
        public JobStatus Status { get; set; } = JobStatus.InProgress;

        public ICollection<Load> Loads { get; set; } = new List<Load>();
    }

    public enum JobStatus
    {
        InProgress,
        Completed,
        Cancelled
    }
}