using CAT.AID.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CAT.AID.Web.Services.PDF
{
    public static class ProgressReportGenerator
    {
        // USED BY ProgressReport()
        public static byte[] GeneratePdf(Candidate candidate, List<Assessment> history)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Content().Column(col =>
                    {
                        col.Item().Text("Candidate Progress Report")
                            .FontSize(18)
                            .Bold();

                        col.Item().Text($"Name: {candidate.FullName}");
                        col.Item().Text($"Total Assessments: {history.Count}");
                    });
                });
            }).GeneratePdf();
        }

        // USED BY ExportProgress()
        public static byte[] Build(Candidate candidate, List<Assessment> assessments)
        {
            // For now, reuse same logic
            return GeneratePdf(candidate, assessments);
        }
    }
}
