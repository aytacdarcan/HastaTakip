using HastaTakip.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly HastaDbContext _db;
    public DashboardController(HastaDbContext db) => _db = db;

    // GET /api/Dashboard/summary?days=30
    [HttpGet("summary")]
    public async Task<ActionResult<object>> Summary([FromQuery] int days = 30)
    {
        days = Math.Clamp(days, 1, 365); // güvenli aralık
        var since = DateTime.Today.AddDays(-days);

        var totalPatients = await _db.Hastalar.AsNoTracking().CountAsync();
        var totalVisits = await _db.Ziyaretler.AsNoTracking().CountAsync();
        var totalAnthros = await _db.Antropometriler.AsNoTracking().CountAsync();

        var newPatients = await _db.Hastalar.AsNoTracking()
            .Where(h => h.KayitTarihi >= since)
            .CountAsync();

        var visitsInRange = await _db.Ziyaretler.AsNoTracking()
            .Where(z => z.Tarih >= since)
            .CountAsync();

        
        var anthrosInRange = await _db.Antropometriler.AsNoTracking()
            .Where(a => a.Ziyaret.Tarih >= since)
            .CountAsync();

        return Ok(new
        {
            totalPatients,
            totalVisits,
            totalAnthros,
            rangeDays = days,
            since = since.ToString("yyyy-MM-dd"),
            newPatients,
            visitsInRange,
            anthrosInRange
        });
    }
}
