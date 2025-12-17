using CAT.AID.Models;
using CAT.AID.Models.DTO;
using CAT.AID.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var dto = new DashboardDTO();

        // ---------------- COUNTS ----------------
        dto.TotalAssessments = await _db.Assessments.CountAsync();
        dto.SubmittedCount = await _db.Assessments.CountAsync(a => a.Status == AssessmentStatus.Submitted);
        dto.PendingCount = await _db.Assessments.CountAsync(a =>
            a.Status == AssessmentStatus.Assigned ||
            a.Status == AssessmentStatus.InProgress);
        dto.ApprovedCount = await _db.Assessments.CountAsync(a => a.Status == AssessmentStatus.Approved);

        // ---------------- MONTHLY TREND ----------------
        var trend = await _db.Assessments
            .Where(a => a.CreatedAt > DateTime.MinValue)
            .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month })
            .Select(g => new
            {
                Label = $"{g.Key.Month:D2}-{g.Key.Year}",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Label)
            .Take(6)
            .ToListAsync();

        dto.MonthLabels = trend.Select(x => x.Label).Reverse().ToList();
        dto.MonthCounts = trend.Select(x => x.Count).Reverse().ToList();

        // ---------------- ASSESSOR PERFORMANCE ----------------
        var assessors = await _db.Assessments
            .Include(a => a.Assessor)
            .Where(a => a.Assessor != null && !string.IsNullOrWhiteSpace(a.Assessor.FullName))
            .GroupBy(a => a.Assessor!.FullName!)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        dto.AssessorNames = assessors.Select(a => a.Name).ToList();
        dto.AssessorCounts = assessors.Select(a => a.Count).ToList();

        // ---------------- LOW PERFORMING DOMAINS ----------------
        var scoreJsons = await _db.Assessments
            .Where(a => !string.IsNullOrWhiteSpace(a.ScoreJson))
            .Select(a => a.ScoreJson!)
            .ToListAsync();

        var domainResults = new Dictionary<string, List<double>>();

        foreach (var json in scoreJsons)
        {
            AssessmentScoreDTO? score;

            try
            {
                score = JsonSerializer.Deserialize<AssessmentScoreDTO>(json);
            }
            catch
            {
                continue; // skip bad JSON
            }

            if (score?.SectionScores == null)
                continue;

            foreach (var sec in score.SectionScores)
            {
                if (!domainResults.ContainsKey(sec.Key))
                    domainResults[sec.Key] = new List<double>();

                domainResults[sec.Key].Add(sec.Value);
            }
        }

        dto.LowDomains = domainResults
            .Where(x => x.Value.Any())
            .Select(x => new
            {
                Domain = x.Key,
                Avg = x.Value.Average()
            })
            .Where(x => x.Avg < 60)
            .OrderBy(x => x.Avg)
            .ToDictionary(x => x.Domain, x => x.Avg);

        // ---------------- ACTIVITY TIMELINE ----------------
        var recent = await _db.Assessments
            .Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(a => a.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key.ToString("dd MMM"),
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        dto.RecentDates = recent.Select(r => r.Date).ToList();
        dto.RecentCounts = recent.Select(r => r.Count).ToList();

        return View(dto);
    }
}
