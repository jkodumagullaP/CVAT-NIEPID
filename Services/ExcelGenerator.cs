using CAT.AID.Models;
using OfficeOpenXml;

namespace CAT.AID.Web.Services.Excel
{
    public static class ExcelGenerator
    {
        public static byte[] BuildScoreSheet(Assessment assessment)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var pkg = new ExcelPackage();
            var ws = pkg.Workbook.Worksheets.Add("Scores");

            ws.Cells[1, 1].Value = "Assessment ID";
            ws.Cells[1, 2].Value = assessment.Id;

            ws.Cells[2, 1].Value = "Candidate";
            ws.Cells[2, 2].Value = assessment.Candidate?.FullName;

            ws.Cells[3, 1].Value = "Status";
            ws.Cells[3, 2].Value = assessment.Status.ToString();

            return pkg.GetAsByteArray();
        }
    }
}
