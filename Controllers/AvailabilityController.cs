[Authorize(Roles = "LeadAssessor")]
public class AvailabilityController : Controller
{
    private readonly ApplicationDbContext _db;

    public AvailabilityController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableAssessors(
        DateOnly date, TimeSpan from, TimeSpan to)
    {
        var available = await _db.AssessorAvailabilities
            .Include(a => a.Assessor)
            .Where(a =>
                a.Date == date &&
                a.SlotFrom <= from &&
                a.SlotTo >= to &&
                !a.IsBooked)
            .Select(a => new
            {
                a.AssessorId,
                a.Assessor.FullName
            })
            .ToListAsync();

        return Json(available);
    }
}
