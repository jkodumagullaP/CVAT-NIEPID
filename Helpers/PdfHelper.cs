using DinkToPdf;
using DinkToPdf.Contracts;

namespace CAT.AID.Web.Helpers
{
    public static class PdfHelper
    {
        public static byte[] GeneratePdfFromHtml(string html)
        {
            var converter = new SynchronizedConverter(new PdfTools());

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return converter.Convert(doc);
        }
    }
}
