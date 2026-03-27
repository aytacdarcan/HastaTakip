using System.Linq;
using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PuberteFiziklerController : ControllerBase
{
    private readonly HastaDbContext _db;
    public PuberteFiziklerController(HastaDbContext db) => _db = db;

    // GET /api/PuberteFizikler/by-ziyaret/6
    [HttpGet("by-visit/{ziyaretId:int}")]
    public async Task<ActionResult<List<PuberteFizikListDto>>> ByZiyaret(int ziyaretId)
    {
        var ziyaretVar = await _db.Ziyaretler.AsNoTracking().AnyAsync(z => z.ZiyaretID == ziyaretId);
        if (!ziyaretVar) return NotFound($"Ziyaret bulunamadı: {ziyaretId}");

        var list = await _db.PuberteFizikler
            .AsNoTracking()
            .Where(p => p.ZiyaretID == ziyaretId)
            .OrderByDescending(p => p.PuberteID)
            .Select(p => new PuberteFizikListDto
            {
                PuberteID = p.PuberteID,
                ZiyaretID = p.ZiyaretID,
                PuberteNotu = p.PuberteNotu,
                PatolojikFizik = p.PatolojikFizik
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/PuberteFizikler/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PuberteFizikDetailDto>> GetById(int id)
    {
        var p = await _db.PuberteFizikler.AsNoTracking()
            .Where(x => x.PuberteID == id)
            .Select(x => new PuberteFizikDetailDto
            {
                PuberteID = x.PuberteID,
                ZiyaretID = x.ZiyaretID,
                PuberteNotu = x.PuberteNotu,
                PatolojikFizik = x.PatolojikFizik
            })
            .FirstOrDefaultAsync();

        return p is null ? NotFound() : Ok(p);
    }

    // POST /api/PuberteFizikler
    [HttpPost]
    public async Task<ActionResult<PuberteFizikDetailDto>> Create([FromBody] PuberteFizikCreateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");
        if (input.ZiyaretID <= 0) return BadRequest("ZiyaretID zorunludur.");

        var ziyaretVar = await _db.Ziyaretler.AsNoTracking().AnyAsync(z => z.ZiyaretID == input.ZiyaretID);
        if (!ziyaretVar) return NotFound($"Ziyaret bulunamadı: {input.ZiyaretID}");

        var e = new PuberteFizik
        {
            ZiyaretID = input.ZiyaretID,
            PuberteNotu = string.IsNullOrWhiteSpace(input.PuberteNotu) ? null : input.PuberteNotu.Trim(),
            PatolojikFizik = string.IsNullOrWhiteSpace(input.PatolojikFizik) ? null : input.PatolojikFizik.Trim()
        };

        _db.PuberteFizikler.Add(e);
        await _db.SaveChangesAsync();

        var dto = new PuberteFizikDetailDto
        {
            PuberteID = e.PuberteID,
            ZiyaretID = e.ZiyaretID,
            PuberteNotu = e.PuberteNotu,
            PatolojikFizik = e.PatolojikFizik
        };

        return CreatedAtAction(nameof(GetById), new { id = e.PuberteID }, dto);
    }

    // PUT /api/PuberteFizikler/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PuberteFizikDetailDto>> Update(int id, [FromBody] PuberteFizikUpdateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");

        var e = await _db.PuberteFizikler.FirstOrDefaultAsync(x => x.PuberteID == id);
        if (e is null) return NotFound();

        if (input.PuberteNotu != null) e.PuberteNotu = string.IsNullOrWhiteSpace(input.PuberteNotu) ? null : input.PuberteNotu.Trim();
        if (input.PatolojikFizik != null) e.PatolojikFizik = string.IsNullOrWhiteSpace(input.PatolojikFizik) ? null : input.PatolojikFizik.Trim();

        await _db.SaveChangesAsync();

        var dto = new PuberteFizikDetailDto
        {
            PuberteID = e.PuberteID,
            ZiyaretID = e.ZiyaretID,
            PuberteNotu = e.PuberteNotu,
            PatolojikFizik = e.PatolojikFizik
        };
        return Ok(dto);
    }

    // DELETE /api/PuberteFizikler/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.PuberteFizikler.FindAsync(id);
        if (e is null) return NotFound();

        _db.PuberteFizikler.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
    // POST /api/PuberteFizikler/by-visit/{ziyaretId}
    [HttpPost("by-visit/{ziyaretId:int}")]
    public async Task<ActionResult<int>> CreateForVisit(int ziyaretId, [FromBody] PuberteFizikCreateDto dto)
    {
        // Ziyaret var mı?
        var z = await _db.Ziyaretler.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ZiyaretID == ziyaretId);
        if (z is null) return NotFound($"Ziyaret bulunamadı: {ziyaretId}");

        // (İSTEĞE BAĞLI) Ziyaret başına tek kayıt kısıtı
        var exists = await _db.PuberteFizikler.AnyAsync(p => p.ZiyaretID == ziyaretId);
        if (exists) return Conflict("Bu ziyaret için zaten puberte fizik kaydı var.");

        var ent = new PuberteFizik
        {
            ZiyaretID = ziyaretId,
            PuberteNotu = string.IsNullOrWhiteSpace(dto.PuberteNotu) ? null : dto.PuberteNotu.Trim(),
            PatolojikFizik = string.IsNullOrWhiteSpace(dto.PatolojikFizik) ? null : dto.PatolojikFizik.Trim()
        };

        _db.PuberteFizikler.Add(ent);
        await _db.SaveChangesAsync();

        // UI bu endpoint’ten int bekliyor
        return Ok(ent.PuberteID);
    }

}
