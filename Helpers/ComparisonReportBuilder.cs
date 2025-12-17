using CAT.AID.Models;
using CAT.AID.Models.DTO;
using System.Text.Json;

namespace CAT.AID.Web.Helpers
{
    public static class ComparisonReportBuilder
    {
        public static ComparisonReportVM Build(List<Assessment> assessments)
        {
            var model = new ComparisonReportVM
            {
                CandidateId = assessments.First().CandidateId,
                CandidateName = assessments.First().Candidate.FullName,
                Assessments = assessments,
                AssessmentIds = assessments.Select(a => a.Id).ToList(),
                Rows = new List<ComparisonRowVM>()
            };

            var questionsFile = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/data/assessment_questions.json"
            );

            var sections = JsonSerializer.Deserialize<List<AssessmentSection>>(
                File.ReadAllText(questionsFile)
            ) ?? new();

            foreach (var section in sections)
            {
                foreach (var q in section.Questions)
                {
                    var scores = new List<int>();

                    foreach (var a in assessments)
                    {
                        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(
                            a.AssessmentResultJson ?? "{}"
                        );

                        data.TryGetValue($"SCORE_{q.Id}", out var val);
                        scores.Add(int.TryParse(val, out var s) ? s : 0);
                    }

                    model.Rows.Add(new ComparisonRowVM
                    {
                        Domain = section.Category,
                        QuestionText = q.Text,
                        Scores = scores,
                        Difference = scores.Last() - scores.First()
                    });
                }
            }

            return model;
        }
    }
}
