using System.Linq;
using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class YorumPlanlarController : ControllerBase
{
    private readonly HastaDbContext _db;
    public YorumPlanlarController(HastaDbContext db) => _db = db;

    // GET /api/YorumPlanlar/by-ziyaret/6
    [HttpGet("by-visit/{ziyaretId:int}")]
    public async Task<ActionResult<List<YorumPlanListDto>>> ByZiyaret(int ziyaretId)
    {
        var ziyaretVar = await _db.Ziyaretler.AsNoTracking().AnyAsync(z => z.ZiyaretID == ziyaretId);
        if (!ziyaretVar) return NotFound($"Ziyaret bulunamadı: {ziyaretId}");

        var list = await _db.YorumPlanlar
            .AsNoTracking()
            .Where(y => y.ZiyaretID == ziyaretId)
            .OrderByDescending(y => y.OlusturmaTarihi)
            .Select(y => new YorumPlanListDto
            {
                YorumID = y.YorumID,
                ZiyaretID = y.ZiyaretID,
                TedaviBeslenmeSpor = y.TedaviBeslenmeSpor,
                YorumNotlar = y.YorumNotlar,
                OlusturmaTarihi = y.OlusturmaTarihi
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/YorumPlanlar/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<YorumPlanDetailDto>> GetById(int id)
    {
        var y = await _db.YorumPlanlar.AsNoTracking()
            .Where(x => x.YorumID == id)
            .Select(x => new YorumPlanDetailDto
            {
                YorumID = x.YorumID,
                ZiyaretID = x.ZiyaretID,
                TedaviBeslenmeSpor = x.TedaviBeslenmeSpor,
                YorumNotlar = x.YorumNotlar,
                OlusturmaTarihi = x.OlusturmaTarihi
            })
            .FirstOrDefaultAsync();

        return y is null ? NotFound() : Ok(y);
    }

    // POST /api/YorumPlanlar
    [HttpPost]
    public async Task<ActionResult<YorumPlanDetailDto>> Create([FromBody] YorumPlanCreateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");
        if (input.ZiyaretID <= 0) return BadRequest("ZiyaretID zorunludur.");

        var ziyaretVar = await _db.Ziyaretler.AsNoTracking().AnyAsync(z => z.ZiyaretID == input.ZiyaretID);
        if (!ziyaretVar) return NotFound($"Ziyaret bulunamadı: {input.ZiyaretID}");

        var e = new YorumPlan
        {
            ZiyaretID = input.ZiyaretID,
            TedaviBeslenmeSpor = string.IsNullOrWhiteSpace(input.TedaviBeslenmeSpor) ? null : input.TedaviBeslenmeSpor.Trim(),
            YorumNotlar = string.IsNullOrWhiteSpace(input.YorumNotlar) ? null : input.YorumNotlar.Trim(),
            OlusturmaTarihi = (input.OlusturmaTarihi ?? DateTime.Now)
        };

        _db.YorumPlanlar.Add(e);
        await _db.SaveChangesAsync();

        var dto = new YorumPlanDetailDto
        {
            YorumID = e.YorumID,
            ZiyaretID = e.ZiyaretID,
            TedaviBeslenmeSpor = e.TedaviBeslenmeSpor,
            YorumNotlar = e.YorumNotlar,
            OlusturmaTarihi = e.OlusturmaTarihi
        };

        return CreatedAtAction(nameof(GetById), new { id = e.YorumID }, dto);
    }

    // PUT /api/YorumPlanlar/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<YorumPlanDetailDto>> Update(int id, [FromBody] YorumPlanUpdateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");

        var e = await _db.YorumPlanlar.FirstOrDefaultAsync(x => x.YorumID == id);
        if (e is null) return NotFound();

        if (input.TedaviBeslenmeSpor != null)
            e.TedaviBeslenmeSpor = string.IsNullOrWhiteSpace(input.TedaviBeslenmeSpor) ? null : input.TedaviBeslenmeSpor.Trim();
        if (input.YorumNotlar != null)
            e.YorumNotlar = string.IsNullOrWhiteSpace(input.YorumNotlar) ? null : input.YorumNotlar.Trim();
        if (input.OlusturmaTarihi.HasValue)
            e.OlusturmaTarihi = input.OlusturmaTarihi.Value;

        await _db.SaveChangesAsync();

        var dto = new YorumPlanDetailDto
        {
            YorumID = e.YorumID,
            ZiyaretID = e.ZiyaretID,
            TedaviBeslenmeSpor = e.TedaviBeslenmeSpor,
            YorumNotlar = e.YorumNotlar,
            OlusturmaTarihi = e.OlusturmaTarihi
        };
        return Ok(dto);
    }

    // DELETE /api/YorumPlanlar/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.YorumPlanlar.FindAsync(id);
        if (e is null) return NotFound();

        _db.YorumPlanlar.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
    // POST /api/YorumPlanlar/by-visit/{ziyaretId}
    [HttpPost("by-visit/{ziyaretId:int}")]
    public async Task<ActionResult<int>> CreateForVisit(int ziyaretId, [FromBody] YorumPlanCreateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");

        // Ziyaret var mı?
        var ziyaret = await _db.Ziyaretler.AsNoTracking()
            .FirstOrDefaultAsync(z => z.ZiyaretID == ziyaretId);
        if (ziyaret is null) return NotFound($"Ziyaret bulunamadı: {ziyaretId}");

        // (İsteğe bağlı) Ziyaret başına tek kayıt kuralı
        var exists = await _db.YorumPlanlar.AnyAsync(y => y.ZiyaretID == ziyaretId);
        if (exists) return Conflict("Bu ziyaret için zaten yorum/plan kaydı var.");

        var e = new YorumPlan
        {
            ZiyaretID = ziyaretId,
            TedaviBeslenmeSpor = string.IsNullOrWhiteSpace(input.TedaviBeslenmeSpor) ? null : input.TedaviBeslenmeSpor.Trim(),
            YorumNotlar = string.IsNullOrWhiteSpace(input.YorumNotlar) ? null : input.YorumNotlar.Trim(),
            OlusturmaTarihi = input.OlusturmaTarihi ?? DateTime.Now
        };

        _db.YorumPlanlar.Add(e);
        await _db.SaveChangesAsync();

        // UI bu endpoint’ten int bekliyor
        return Ok(e.YorumID);
    }

}
