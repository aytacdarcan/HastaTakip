using HastaTakip.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HastaDbContext _db;
    public HealthController(HastaDbContext db) => _db = db;

    // GET /api/Health/db  
    [HttpGet("db")]
    public async Task<ActionResult<object>> CheckDb()
    {
        try
        {
            var can = await _db.Database.CanConnectAsync();
            if (!can)
                return StatusCode(503, new { ok = false, db = false, message = "Veritabanına ulaşılamıyor." });

            
            var sampleId = await _db.Hastalar.AsNoTracking()
                               .Select(x => x.Id)
                               .FirstOrDefaultAsync();

            return Ok(new
            {
                ok = true,
                db = true,
                samplePatientId = sampleId,     
                serverTime = DateTime.Now.ToString("O")
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, db = false, error = ex.Message });
        }
    }
}
