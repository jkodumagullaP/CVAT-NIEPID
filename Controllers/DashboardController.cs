using CAT.AID.Models;
using CAT.AID.Web.Data;
using CAT.AID.Models.DTO;
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

        // ---------------------------------------------------
        // OVERALL COUNTS
        // ---------------------------------------------------
        dto.TotalAssessments = await _db.Assessments.CountAsync();

        dto.SubmittedCount = await _db.Assessments
            .CountAsync(a => a.Status == AssessmentStatus.Submitted);

        dto.PendingCount = await _db.Assessments
            .CountAsync(a =>
                a.Status == AssessmentStatus.Assigned ||
                a.Status == AssessmentStatus.InProgress);

        dto.ApprovedCount = await _db.Assessments
            .CountAsync(a => a.Status == AssessmentStatus.Approved);

        // ---------------------------------------------------
        // MONTHLY TREND (LAST 6 MONTHS)
        // ---------------------------------------------------
        var trendData = await _db.Assessments
            .GroupBy(a => new
            {
                a.CreatedAt.Year,
                a.CreatedAt.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .Take(6)
            .ToListAsync();

        var trend = trendData
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .Select(x => new
            {
                Label = $"{x.Month:D2}-{x.Year}",   // CLIENT-SIDE FORMATTING
                x.Count
            })
            .ToList();

        dto.MonthLabels = trend.Select(x => x.Label).ToList();
        dto.MonthCounts = trend.Select(x => x.Count).ToList();

        // ---------------------------------------------------
        // ASSESSOR PERFORMANCE
        // ---------------------------------------------------
        var assessors = await _db.Assessments
            .Where(a => a.Assessor != null)
            .GroupBy(a => a.Assessor!.FullName)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        dto.AssessorNames = assessors.Select(a => a.Name).ToList();
        dto.AssessorCounts = assessors.Select(a => a.Count).ToList();

        // ---------------------------------------------------
        // LOW PERFORMING DOMAINS (AVG < 60)
        // ---------------------------------------------------
        var scoreJsonList = await _db.Assessments
            .Where(a => a.ScoreJson != null)
            .Select(a => a.ScoreJson!)
            .ToListAsync();

        var domainScores = new Dictionary<string, List<double>>();

        foreach (var json in scoreJsonList)
        {
            var score = JsonSerializer.Deserialize<AssessmentScoreDTO>(json);
            if (score == null) continue;

            foreach (var sec in score.SectionScores)
            {
                if (!domainScores.ContainsKey(sec.Key))
                    domainScores[sec.Key] = new List<double>();

                domainScores[sec.Key].Add(sec.Value);
            }
        }

        dto.LowDomains = domainScores
            .ToDictionary(
                x => x.Key,
                x => x.Value.Count > 0 ? x.Value.Average() : 0
            )
            .Where(x => x.Value < 60)
            .OrderBy(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

        // ---------------------------------------------------
        // ACTIVITY TIMELINE (LAST 30 DAYS)
        // ---------------------------------------------------
        var recentData = await _db.Assessments
            .Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(a => a.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        dto.RecentDates = recentData
            .Select(r => r.Date.ToString("dd MMM")) // CLIENT-SIDE FORMATTING
            .ToList();

        dto.RecentCounts = recentData
            .Select(r => r.Count)
            .ToList();

        return View(dto);
    }
}
