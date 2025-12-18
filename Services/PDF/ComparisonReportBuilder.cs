using CAT.AID.Models;
using CAT.AID.Models.DTO;

namespace CAT.AID.Web.Services.PDF
{
    public static class ComparisonReportBuilder
    {
        public static ComparisonReportDTO Build(List<Assessment> assessments)
        {
            var candidate = assessments.First().Candidate;

            return new ComparisonReportDTO
            {
                CandidateName = candidate?.FullName ?? "Candidate",
                Assessments = assessments
            };
        }
    }
}
