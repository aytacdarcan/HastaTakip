using HastaTakip.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/lab")]
public class LabController : ControllerBase
{
    private readonly HastaDbContext _db;

    public LabController(HastaDbContext db)
    {
        _db = db;
    }

    // GET /api/lab/by-ziyaret?ziyaretId=36
    [HttpGet("by-ziyaret")]
    public async Task<IActionResult> GetByZiyaret([FromQuery] int ziyaretId)
    {
        var raw = await (
            from s in _db.LabSonuclari.AsNoTracking()
            join p in _db.LabParametreler.AsNoTracking()
                on s.LabParametreID equals p.LabParametreID
            where s.ZiyaretID == ziyaretId
            select new
            {
                Tarih = s.Tarih,
                p.LabParametreID,
                Param = p.Ad + (p.Birim != null ? $" ({p.Birim})" : ""),
                GosterimDegeri =
                    s.DegerSayisal != null
                        ? s.DegerSayisal.Value.ToString("0.###")
                        : s.Deger
            }
        ).ToListAsync();

        var dates = raw
            .Select(x => x.Tarih)
            .Distinct()
            .OrderBy(d => d)
            .Select(d => d.ToString("dd.MM.yyyy"))
            .ToList();

        var rows = raw
            .GroupBy(x => new { x.LabParametreID, x.Param })
            .OrderBy(g => g.Key.Param)
            .Select(g => new
            {
                param = g.Key.Param,
                values = dates.Select(d =>
                {
                    var hit = g.FirstOrDefault(x => x.Tarih.ToString("dd.MM.yyyy") == d);
                    return hit?.GosterimDegeri;
                }).ToList()
            })
            .ToList();

        return Ok(new { dates, rows });
    }
    [HttpGet("parametreler")]
    public async Task<IActionResult> GetParametreler()
    {
        var list = await _db.LabParametreler
            .AsNoTracking()
            .OrderBy(x => x.Ad)
            .Select(x => new
            {
                x.LabParametreID,
                x.Ad,
                x.Birim,
                x.Kategori
            })
            .ToListAsync();

        return Ok(list);
    }

}
