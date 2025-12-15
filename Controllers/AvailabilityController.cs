using CAT.AID.Web.Data;
using CAT.AID.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAT.AID.Web.Controllers
{
    [Authorize]
    public class AvailabilityController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AvailabilityController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableAssessors(DateOnly date, TimeSpan from, TimeSpan to)
        {
            var assessors = await _db.AssessorAvailabilities
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
                .Distinct()
                .ToListAsync();

            return Json(assessors);
        }
    }
}
