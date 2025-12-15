using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CAT.AID.Web.Services.PDF
{
    public abstract class BasePdfTemplate : IDocument
    {
        protected string Title { get; }
        protected string LogoLeft { get; }
        protected string LogoRight { get; }

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

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.Now.ToString("dd-MMM-yyyy HH:mm"));
                });
            });
        }

        protected abstract void ComposeContent(IContainer container);

        protected virtual void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Height(50).Image(LogoLeft);
                row.RelativeItem().AlignCenter().Text(Title).FontSize(18).Bold();
                row.RelativeItem().Height(50).AlignRight().Image(LogoRight);
            });
        }
    }
}
