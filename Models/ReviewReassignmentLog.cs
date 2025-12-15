public class ReviewReassignmentLog
{
    public int Id { get; set; }

    public int AssessmentId { get; set; }

    public string FromLeadAssessorId { get; set; }
    public string ToLeadAssessorId { get; set; }

    public DateTime ReassignedAt { get; set; }
}
