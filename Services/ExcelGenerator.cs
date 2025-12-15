using CAT.AID.Models;
using OfficeOpenXml;

namespace CAT.AID.Web.Services.Excel
{
    public static class ExcelGenerator
    {
        public static byte[] BuildScoreSheet(Assessment a)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var pkg = new ExcelPackage();
            var ws = pkg.Workbook.Worksheets.Add("Scores");

            ws.Cells[1, 1].Value = "Candidate";
            ws.Cells[1, 2].Value = a.Candidate.FullName;
            ws.Cells[2, 1].Value = "Total Score";
            ws.Cells[2, 2].Value = a.ScoreJson;

            return pkg.GetAsByteArray();
        }
    }
}
