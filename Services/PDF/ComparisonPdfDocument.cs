using CAT.AID.Models.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CAT.AID.Web.Services.PDF
{
    public class ComparisonPdfDocument : IDocument
    {
        private readonly ComparisonReportDTO _model;

        public ComparisonPdfDocument(ComparisonReportDTO model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata()
            => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text($"Assessment Comparison – {_model.CandidateName}")
                    .FontSize(18)
                    .Bold()
                    .AlignCenter();

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    foreach (var a in _model.Assessments)
                    {
                        col.Item().Border(1).Padding(10).Column(c =>
                        {
                            c.Item().Text($"Assessment ID: {a.Id}").Bold();
                            c.Item().Text($"Date: {a.CreatedAt:dd MMM yyyy}");
                            c.Item().Text($"Status: {a.Status}");
                        });
                    }
                });
            });
        }
    }
}
