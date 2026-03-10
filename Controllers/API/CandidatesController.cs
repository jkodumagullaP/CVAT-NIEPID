using Microsoft.AspNetCore.Mvc;
using CVAT_NIEPID.Data;
using CVAT_NIEPID.Models;

namespace CVAT_NIEPID.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CandidatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetCandidates()
        {
            var candidates = _context.Candidates.ToList();
            return Ok(candidates);
        }

        [HttpPost]
        public IActionResult AddCandidate([FromBody] Candidate candidate)
        {
            _context.Candidates.Add(candidate);
            _context.SaveChanges();

            return Ok(candidate);
        }
    }
}
