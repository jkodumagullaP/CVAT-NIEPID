using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CAT.AID.Web.Services.PDF
{
    public abstract class BasePdfTemplate : IDocument
    {
        protected string Title;
        protected string LogoLeft;
        protected string LogoRight;

        protected BasePdfTemplate(string title, string logoLeft, string logoRight)
        {
            Title = title;
            LogoLeft = logoLeft;
            LogoRight = logoRight;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

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

        protected abstract void ComposeContent(IContainer container);

        private void BuildHeader(IContainer container)
        {
            container.Row(row =>
            {
                if (File.Exists(LogoLeft))
                    row.RelativeItem(1).Image(LogoLeft).FitHeight(60);

                row.RelativeItem(3).AlignCenter().Text(Title)
                    .FontSize(18).Bold();

                if (File.Exists(LogoRight))
                    row.RelativeItem(1).AlignRight().Image(LogoRight).FitHeight(60);
            });
        }

        private void BuildFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Generated on ");
                text.Span(DateTime.Now.ToString("dd-MMM-yyyy")).SemiBold();
            })
            .FontSize(9)
            .FontColor(Colors.Grey.Medium);
        }
    }
}
