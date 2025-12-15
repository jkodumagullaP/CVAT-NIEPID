using CAT.AID.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAT.AID.Web.Controllers
{
    [Authorize]
    public class ProgressController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProgressController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _db.Assessments
                .Include(a => a.Candidate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(data);
        }
    }
}
