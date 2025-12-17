using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CAT.AID.Models.DTO;

namespace CAT.AID.Web.PDF
{
    public class ComparisonPdfDocument : IDocument
    {
        private readonly ComparisonReportVM _model;

        public ComparisonPdfDocument(ComparisonReportVM model)
        {
            _model = model;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(Header);
                page.Content().Element(Content);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("NIEPID CVAT – Comparison Report").FontSize(9);
                });
            });
        }

        void Header(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("Assessment Progress Comparison")
                    .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);

                col.Item().Text($"Candidate: {_model.CandidateName}")
                    .FontSize(12).SemiBold();

                col.Item().PaddingVertical(5)
                    .LineHorizontal(1)
                    .LineColor(Colors.Grey.Lighten2);
            });
        }

        void Content(IContainer container)
        {
            var grouped = _model.Rows.GroupBy(x => x.Domain);

            container.Column(col =>
            {
                foreach (var domain in grouped)
                {
                    col.Item().PaddingTop(10).Text(domain.Key)
                        .FontSize(14).Bold().FontColor(Colors.Brown.Darken2);

                    foreach (var q in domain)
                    {
                        col.Item().PaddingVertical(5).Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(8)
                            .Column(qCol =>
                            {
                                qCol.Item().Text(q.QuestionText).Bold();

                                qCol.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(c =>
                                    {
                                        foreach (var _ in _model.Assessments)
                                            c.RelativeColumn();
                                    });

                                    table.Header(h =>
                                    {
                                        foreach (var a in _model.Assessments)
                                        {
                                            h.Cell().AlignCenter()
                                                .Text(a.SubmittedAt?.ToString("dd-MMM-yyyy") ?? "")
                                                .Bold();
                                        }
                                    });

                                    table.Cell().Row(2);

                                    table.Body(b =>
                                    {
                                        foreach (var s in q.Scores)
                                        {
                                            b.Cell().AlignCenter().Text(s.ToString());
                                        }
                                    });
                                });

                                if (q.Difference.HasValue)
                                {
                                    var diff = q.Difference.Value;
                                    var symbol = diff > 0 ? "▲" : diff < 0 ? "▼" : "•";

                                    qCol.Item().PaddingTop(4)
                                        .Text($"Difference: {symbol} {diff}")
                                        .FontColor(
                                            diff > 0 ? Colors.Green.Darken2 :
                                            diff < 0 ? Colors.Blue.Darken2 :
                                            Colors.Grey.Darken1
                                        )
                                        .Bold();
                                }
                            });
                    }
                }
            });
        }
    }
}
