using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CAT.AID.Web.Services.PDF
{
    public abstract class BasePdfTemplate : IDocument
    {
        protected readonly string Title;
        protected readonly string LogoLeftPath;
        protected readonly string LogoRightPath;

        protected BasePdfTemplate(string title, string logoLeft, string logoRight)
        {
            Title = title;
            LogoLeftPath = logoLeft;
            LogoRightPath = logoRight;
        }

        // ---------------------------------------------------------------------
        // QuestPDF document metadata
        // ---------------------------------------------------------------------
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        // ---------------------------------------------------------------------
        // Main document composition
        // ---------------------------------------------------------------------
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header().Element(BuildHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(BuildFooter);
            });
        }

        // ---------------------------------------------------------------------
        // HEADER
        // ---------------------------------------------------------------------
        private void BuildHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem(1).AlignLeft().Element(c =>
                {
                    if (!string.IsNullOrWhiteSpace(LogoLeftPath) && File.Exists(LogoLeftPath))
                        c.Image(LogoLeftPath, ImageScaling.FitHeight);
                });

                row.RelativeItem(3)
                   .AlignCenter()
                   .Text(Title)
                   .FontSize(20)
                   .Bold();

                row.RelativeItem(1).AlignRight().Element(c =>
                {
                    if (!string.IsNullOrWhiteSpace(LogoRightPath) && File.Exists(LogoRightPath))
                        c.Image(LogoRightPath, ImageScaling.FitHeight);
                });
            });
        }

        // ---------------------------------------------------------------------
        // FOOTER
        // ---------------------------------------------------------------------
        private void BuildFooter(IContainer container)
        {
            container.AlignCenter()
                .Text(text =>
                {
                    text.Span("Generated on ");
                    text.Span(DateTime.Now.ToString("dd-MMM-yyyy")).SemiBold();
                })
                .FontSize(9)
                .FontColor(Colors.Grey.Medium);
        }

        // ---------------------------------------------------------------------
        // CONTENT (implemented by child PDFs)
        // ---------------------------------------------------------------------
        public abstract void ComposeContent(IContainer container);

        // ---------------------------------------------------------------------
        // PDF GENERATION
        // ---------------------------------------------------------------------
        public byte[] GeneratePdf()
        {
            return Document.Create(container => Compose(container)).GeneratePdf();
        }
    }
}
