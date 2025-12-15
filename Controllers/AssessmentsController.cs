using CAT.AID.Models;
using CAT.AID.Models.DTO;
using CAT.AID.Web.Data;
using CAT.AID.Web.Models;
using CAT.AID.Web.Services.PDF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CAT.AID.Web.Controllers
{
    [Authorize]
    public class AssessmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _user;
        private readonly IWebHostEnvironment _environment;

        public AssessmentsController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> user,
            IWebHostEnvironment env)
        {
            _db = db;
            _user = user;
            _environment = env;
        }

        // =========================================================
        // 1. MY TASKS
        // =========================================================
        [Authorize(Roles = "LeadAssessor,Assessor")]
        public async Task<IActionResult> MyTasks()
        {
            var uid = _user.GetUserId(User)!;

            var tasks = await _db.Assessments
                .Include(a => a.Candidate)
                .Where(a => a.AssessorId == uid)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            if (!tasks.Any())
                return View(new List<CandidateAssessmentPivotVM>());

            var timestamps = tasks
                .Select(a => a.CreatedAt)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var grouped = tasks
                .GroupBy(a => a.CandidateId)
                .Select(g => new CandidateAssessmentPivotVM
                {
                    CandidateId = g.Key,
                    CandidateName = g.First().Candidate.FullName,
                    AssessmentIds = timestamps.ToDictionary(
                        ts => ts,
                        ts => g.FirstOrDefault(a => a.CreatedAt == ts)?.Id
                    ),
                    StatusMapping = g.ToDictionary(
                        a => a.Id,
                        a => a.Status.ToString()
                    )
                })
                .ToList();

            ViewBag.Timestamps = timestamps;
            return View(grouped);
        }

        // =========================================================
        // 2. PERFORM ASSESSMENT (GET)
        // =========================================================
        [Authorize(Roles = "Assessor,LeadAssessor,Admin")]
        public async Task<IActionResult> Perform(int id)
        {
            var a = await _db.Assessments
                .Include(x => x.Candidate)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a == null) return NotFound();

            var qFile = Path.Combine(_environment.WebRootPath, "data", "assessment_questions.json");
            var sections = JsonSerializer.Deserialize<List<AssessmentSection>>(System.IO.File.ReadAllText(qFile))
                           ?? new List<AssessmentSection>();

            ViewBag.Sections = sections;
            return View(a);
        }

        // =========================================================
        // 3. PERFORM ASSESSMENT (POST)
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "Assessor,Lead")]
        public async Task<IActionResult> Perform(int id, string actionType)
        {
            var a = await _db.Assessments.FindAsync(id);
            if (a == null) return NotFound();
            if (!a.IsEditableByAssessor) return Unauthorized();

            // Read answers
            var data = new Dictionary<string, string>();
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("ANS_") ||
                    key.StartsWith("SCORE_") ||
                    key.StartsWith("CMT_") ||
                    key == "SUMMARY_COMMENTS")
                {
                    data[key] = Request.Form[key];
                }
            }

            // Upload files
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in Request.Form.Files)
            {
                if (file.Length == 0) continue;

                var name = $"{Guid.NewGuid()}_{file.FileName}";
                var path = Path.Combine(uploadFolder, name);
                using var fs = System.IO.File.Create(path);
                await file.CopyToAsync(fs);
                data[file.Name] = name;
            }

            a.AssessmentResultJson = JsonSerializer.Serialize(data);

            // Build score
            var qFile = Path.Combine(_environment.WebRootPath, "data", "assessment_questions.json");
            var sections = JsonSerializer.Deserialize<List<AssessmentSection>>(System.IO.File.ReadAllText(qFile))
                           ?? new();

            var scoreDto = new AssessmentScoreDTO();
            int totalMax = 0;

            foreach (var sec in sections)
            {
                int secTotal = 0;
                var qScores = new Dictionary<string, int>();

                foreach (var q in sec.Questions)
                {
                    totalMax += 3;
                    if (data.TryGetValue($"SCORE_{q.Id}", out var scr) &&
                        int.TryParse(scr, out int val))
                    {
                        secTotal += val;
                        qScores[q.Text] = val;
                    }
                }

                scoreDto.SectionScores[sec.Category] = secTotal;
                scoreDto.SectionQuestionScores[sec.Category] = qScores;
            }

            scoreDto.TotalScore = scoreDto.SectionScores.Sum(x => x.Value);
            scoreDto.MaxScore = totalMax;
            a.ScoreJson = JsonSerializer.Serialize(scoreDto);

            if (actionType == "submit")
            {
                a.Status = AssessmentStatus.Submitted;
                a.SubmittedAt = DateTime.UtcNow;
            }
            else
            {
                a.Status = AssessmentStatus.InProgress;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(MyTasks));
        }

        // =========================================================
        // 4. SUMMARY (FIXED weakList ISSUE)
        // =========================================================
        public IActionResult Summary(int id)
        {
            var a = _db.Assessments
                .Include(x => x.Candidate)
                .Include(x => x.Assessor)
                .FirstOrDefault(x => x.Id == id);

            if (a == null) return NotFound();

            var qFile = Path.Combine(_environment.WebRootPath, "data", "assessment_questions.json");
            var sections = JsonSerializer.Deserialize<List<AssessmentSection>>(System.IO.File.ReadAllText(qFile))
                           ?? new();

            var score = JsonSerializer.Deserialize<AssessmentScoreDTO>(a.ScoreJson)
                        ?? new AssessmentScoreDTO();

            var recFile = Path.Combine(_environment.WebRootPath, "data", "recommendations.json");
            var recLib = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
                             System.IO.File.ReadAllText(recFile))
                         ?? new();

            var saved = JsonSerializer.Deserialize<Dictionary<string, string>>(a.AssessmentResultJson)
                        ?? new();

            var sectionMax = sections.ToDictionary(s => s.Category, s => s.Questions.Count * 3);

            Dictionary<string, List<string>> recommendations = new();
            Dictionary<string, List<(string Question, int Score)>> weakDetails = new();

            foreach (var sec in score.SectionScores)
            {
                double pct = (sec.Value / sectionMax[sec.Key]) * 100;

                if (pct < 100 && recLib.ContainsKey(sec.Key))
                    recommendations[sec.Key] = recLib[sec.Key];

                var weakList = new List<(string Question, int Score)>();
                var section = sections.FirstOrDefault(s => s.Category == sec.Key);
                if (section == null) continue;

                foreach (var q in section.Questions)
                {
                    saved.TryGetValue($"SCORE_{q.Id}", out var scr);
                    int sc = int.TryParse(scr, out int x) ? x : 0;
                    if (sc < 3)
                        weakList.Add((q.Text, sc));
                }

                if (weakList.Any())
                    weakDetails[sec.Key] = weakList;
            }

            ViewBag.Recommendations = recommendations;
            ViewBag.WeakDetails = weakDetails;
            ViewBag.Sections = sections;

            return View(a);
        }

        // =========================================================
        // 5. EXPORT PDF
        // =========================================================
        [Authorize(Roles = "Assessor,LeadAssessor,Admin")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var a = await _db.Assessments
                .Include(x => x.Candidate)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a == null) return NotFound();

            var pdf = ReportGenerator.BuildAssessmentReport(a);
            return File(pdf, "application/pdf", $"Assessment_{a.Id}.pdf");
        }
    }
}
