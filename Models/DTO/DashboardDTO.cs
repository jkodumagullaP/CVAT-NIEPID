namespace CAT.AID.Models.DTO
{
    public class DashboardDTO
    {
        // KPI COUNTS
        public int TotalAssessments { get; set; }

        public int SubmittedCount { get; set; }

        public int PendingCount { get; set; }

        public int ApprovedCount { get; set; }


        // MONTHLY TREND
        public List<string> MonthLabels { get; set; } = new();

        public List<int> MonthCounts { get; set; } = new();


        // ASSESSOR PERFORMANCE
        public List<string> AssessorNames { get; set; } = new();

        public List<int> AssessorCounts { get; set; } = new();


        // LOW DOMAINS
        public Dictionary<string, double> LowDomains { get; set; } = new();


        // RECENT ACTIVITY
        public List<string> RecentDates { get; set; } = new();

        public List<int> RecentCounts { get; set; } = new();
    }
}
