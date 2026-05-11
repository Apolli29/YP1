using System;

namespace YP1.Models
{
    public class ReportModel
    {
        public int ReportId { get; set; }
        public int ReporterUserId { get; set; }
        public string ReporterName { get; set; }
        public string TargetType { get; set; }
        public int TargetId { get; set; }
        public string TargetName { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
