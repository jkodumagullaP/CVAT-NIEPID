using Microsoft.AspNetCore.Mvc;
using CAT.AID.Models;

namespace CAT.AID.Controllers
{
public class AdminController : Controller
{
private readonly AppDbContext _context;

```
    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        var model = new DashboardViewModel();

        model.TotalCandidates = _context.Candidates.Count();
        model.TotalAssessments = _context.Assessments.Count();

        model.RecentAssessments = _context.Assessments
            .OrderByDescending(a => a.Id)
            .Take(10)
            .ToList();

        return View(model);
    }
}
```

}
