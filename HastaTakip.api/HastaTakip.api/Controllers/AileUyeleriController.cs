using System.Linq;
using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AileUyeleriController : ControllerBase
{
    private readonly HastaDbContext _db;
    public AileUyeleriController(HastaDbContext db) => _db = db;

    // GET /api/AileUyeleri/by-hasta/2
    [HttpGet("by-hasta/{hastaId:int}")]
    public async Task<ActionResult<List<AileUyesiListDto>>> ByHasta(int hastaId)
    {
        var hastaVar = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.Id == hastaId);
        if (!hastaVar) return NotFound($"Hasta bulunamadı: {hastaId}");

        var list = await _db.AileUyeleri
            .AsNoTracking()
            .Where(a => a.HastaID == hastaId)
            .OrderBy(a => a.Iliski).ThenBy(a => a.Ad)
            .Select(a => new AileUyesiListDto
            {
                AileUyesiID = a.AileUyesiID,
                HastaID = a.HastaID,
                Iliski = a.Iliski,
                Ad = a.Ad,
                DogumTarihi = a.DogumTarihi,
                BoyCm = a.BoyCm,
                AgirlikKg = a.AgirlikKg,
                PuberteYasiYil = a.PuberteYasiYil,
                SaglikDurumu = a.SaglikDurumu,
                Meslek = a.Meslek
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/AileUyeleri/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AileUyesiListDto>> GetById(int id)
    {
        var a = await _db.AileUyeleri.AsNoTracking()
            .Where(x => x.AileUyesiID == id)
            .Select(x => new AileUyesiListDto
            {
                AileUyesiID = x.AileUyesiID,
                HastaID = x.HastaID,
                Iliski = x.Iliski,
                Ad = x.Ad,
                DogumTarihi = x.DogumTarihi,
                BoyCm = x.BoyCm,
                AgirlikKg = x.AgirlikKg,
                PuberteYasiYil = x.PuberteYasiYil,
                SaglikDurumu = x.SaglikDurumu,
                Meslek = x.Meslek
            })
            .FirstOrDefaultAsync();

        return a is null ? NotFound() : Ok(a);
    }

    // POST /api/AileUyeleri
    [HttpPost]
    public async Task<ActionResult<AileUyesiListDto>> Create([FromBody] AileUyesiCreateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");
        if (string.IsNullOrWhiteSpace(input.Iliski)) return BadRequest("İlişki zorunludur.");

        var hastaVar = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.Id == input.HastaID);
        if (!hastaVar) return NotFound($"Hasta bulunamadı: {input.HastaID}");

        var e = new AileUyesi
        {
            HastaID = input.HastaID,
            Iliski = input.Iliski.Trim(),
            Ad = string.IsNullOrWhiteSpace(input.Ad) ? null : input.Ad.Trim(),
            DogumTarihi = input.DogumTarihi,
            BoyCm = input.BoyCm,
            AgirlikKg = input.AgirlikKg,
            PuberteYasiYil = input.PuberteYasiYil,
            SaglikDurumu = string.IsNullOrWhiteSpace(input.SaglikDurumu) ? null : input.SaglikDurumu.Trim(),
            Meslek = string.IsNullOrWhiteSpace(input.Meslek) ? null : input.Meslek.Trim()
        };

        _db.AileUyeleri.Add(e);
        await _db.SaveChangesAsync();

        var dto = new AileUyesiListDto
        {
            AileUyesiID = e.AileUyesiID,
            HastaID = e.HastaID,
            Iliski = e.Iliski,
            Ad = e.Ad,
            DogumTarihi = e.DogumTarihi,
            BoyCm = e.BoyCm,
            AgirlikKg = e.AgirlikKg,
            PuberteYasiYil = e.PuberteYasiYil,
            SaglikDurumu = e.SaglikDurumu,
            Meslek = e.Meslek
        };

        return CreatedAtAction(nameof(GetById), new { id = e.AileUyesiID }, dto);
    }

    // PUT /api/AileUyeleri/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AileUyesiListDto>> Update(int id, [FromBody] AileUyesiUpdateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");

        var e = await _db.AileUyeleri.FirstOrDefaultAsync(x => x.AileUyesiID == id);
        if (e is null) return NotFound();

        if (string.IsNullOrWhiteSpace(input.Iliski))
            return BadRequest("İlişki zorunludur.");

        e.Iliski = input.Iliski.Trim();
        e.Ad = string.IsNullOrWhiteSpace(input.Ad) ? null : input.Ad.Trim();
        e.DogumTarihi = input.DogumTarihi;
        e.BoyCm = input.BoyCm;
        e.AgirlikKg = input.AgirlikKg;
        e.PuberteYasiYil = input.PuberteYasiYil;
        e.SaglikDurumu = string.IsNullOrWhiteSpace(input.SaglikDurumu) ? null : input.SaglikDurumu.Trim();
        e.Meslek = string.IsNullOrWhiteSpace(input.Meslek) ? null : input.Meslek.Trim();

        await _db.SaveChangesAsync();

        var dto = new AileUyesiListDto
        {
            AileUyesiID = e.AileUyesiID,
            HastaID = e.HastaID,
            Iliski = e.Iliski,
            Ad = e.Ad,
            DogumTarihi = e.DogumTarihi,
            BoyCm = e.BoyCm,
            AgirlikKg = e.AgirlikKg,
            PuberteYasiYil = e.PuberteYasiYil,
            SaglikDurumu = e.SaglikDurumu,
            Meslek = e.Meslek
        };
        return Ok(dto);
    }

    // DELETE /api/AileUyeleri/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.AileUyeleri.FindAsync(id);
        if (e is null) return NotFound();

        _db.AileUyeleri.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
