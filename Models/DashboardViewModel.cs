namespace EShift123.Models
{
    public class DashboardViewModel
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }     // Jobs InProgress
        public int CompletedJobs { get; set; }
        public int CancelledJobs { get; set; }
    }
}
