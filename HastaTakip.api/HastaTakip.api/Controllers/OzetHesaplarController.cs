using HastaTakip.api.Dtos;
using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OzetHesaplarController : ControllerBase
{
    private readonly HastaDbContext _db;
    public OzetHesaplarController(HastaDbContext db) => _db = db;

    // GET api/OzetHesaplar/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OzetHesapDto>> GetById(int id)
    {
        var dto = await _db.OzetHesaplar.AsNoTracking()
            .Where(x => x.OzetID == id)
            .Select(x => new OzetHesapDto
            {
                OzetID = x.OzetID,
                ZiyaretID = x.ZiyaretID,
                KulacCm = x.KulacCm,
                HedefBoyCm = x.HedefBoyCm,
                BoyaUyanTarti = x.BoyaUyanTarti
            })
            .FirstOrDefaultAsync();

        return dto is null ? NotFound() : Ok(dto);
    }

    // GET api/OzetHesaplar/by-ziyaret/5
    [HttpGet("by-visit/{ziyaretId:int}")]
    public async Task<ActionResult<IEnumerable<OzetHesapDto>>> GetByZiyaret(int ziyaretId)
    {
        var list = await _db.OzetHesaplar.AsNoTracking()
            .Where(x => x.ZiyaretID == ziyaretId)
            .Select(x => new OzetHesapDto
            {
                OzetID = x.OzetID,
                ZiyaretID = x.ZiyaretID,
                KulacCm = x.KulacCm,
                HedefBoyCm = x.HedefBoyCm,
                BoyaUyanTarti = x.BoyaUyanTarti
            })
            .ToListAsync();

        return Ok(list);
    }

    // POST api/OzetHesaplar
    [HttpPost]
    public async Task<ActionResult<OzetHesapDto>> Create(OzetHesapCreateDto input)
    {
        if (input == null) return BadRequest("Body boş olamaz.");
        if (input.ZiyaretID <= 0) return BadRequest("ZiyaretID zorunludur.");
        if (input.BoyaUyanTarti is { Length: > 50 }) return BadRequest("BoyaUyanTarti en fazla 50 karakter olabilir.");

        var ziyaretVar = await _db.Ziyaretler.AnyAsync(z => z.ZiyaretID == input.ZiyaretID);
        if (!ziyaretVar) return NotFound($"Ziyaret bulunamadı: {input.ZiyaretID}");

        var entity = new OzetHesapKlinik
        {
            ZiyaretID = input.ZiyaretID,
            KulacCm = input.KulacCm,
            HedefBoyCm = input.HedefBoyCm,
            BoyaUyanTarti = string.IsNullOrWhiteSpace(input.BoyaUyanTarti) ? null : input.BoyaUyanTarti.Trim()
        };

        _db.OzetHesaplar.Add(entity);
        await _db.SaveChangesAsync();

        var dto = new OzetHesapDto
        {
            OzetID = entity.OzetID,
            ZiyaretID = entity.ZiyaretID,
            KulacCm = entity.KulacCm,
            HedefBoyCm = entity.HedefBoyCm,
            BoyaUyanTarti = entity.BoyaUyanTarti
        };

        return CreatedAtAction(nameof(GetById), new { id = dto.OzetID }, dto);
    }

    // PUT api/OzetHesaplar/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OzetHesapDto>> Update(int id, OzetHesapUpdateDto input)
    {
        var entity = await _db.OzetHesaplar.FirstOrDefaultAsync(x => x.OzetID == id);
        if (entity == null) return NotFound();

        if (input.BoyaUyanTarti is { Length: > 50 })
            return BadRequest("BoyaUyanTarti en fazla 50 karakter olabilir.");

        entity.KulacCm = input.KulacCm;
        entity.HedefBoyCm = input.HedefBoyCm;
        entity.BoyaUyanTarti = string.IsNullOrWhiteSpace(input.BoyaUyanTarti) ? null : input.BoyaUyanTarti.Trim();

        await _db.SaveChangesAsync();

        return Ok(new OzetHesapDto
        {
            OzetID = entity.OzetID,
            ZiyaretID = entity.ZiyaretID,
            KulacCm = entity.KulacCm,
            HedefBoyCm = entity.HedefBoyCm,
            BoyaUyanTarti = entity.BoyaUyanTarti
        });
    }

    // DELETE api/OzetHesaplar/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.OzetHesaplar.FindAsync(id);
        if (entity == null) return NotFound();

        _db.OzetHesaplar.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
