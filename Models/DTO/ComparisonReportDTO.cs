using CAT.AID.Models;

namespace CAT.AID.Models.DTO
{
    public class ComparisonReportDTO
    {
        public string CandidateName { get; set; } = string.Empty;
        public List<Assessment> Assessments { get; set; } = new();
    }
}
