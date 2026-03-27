using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiyetlerController : ControllerBase
{
    private readonly HastaDbContext _db;
    public DiyetlerController(HastaDbContext db) => _db = db;

    // GET /api/Diyetler/by-hasta/{hastaId}
    [HttpGet("by-hasta/{hastaId:int}")]
    public async Task<ActionResult<List<DiyetListDto>>> ByHasta(int hastaId, [FromQuery] int take = 200)
    {
        var hastaVar = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.Id == hastaId);
        if (!hastaVar) return NotFound($"Hasta bulunamadı: {hastaId}");

        take = Math.Clamp(take, 1, 500);

        var list = await _db.Diyetler
            .AsNoTracking()
            .Where(d => d.HastaID == hastaId)
            .OrderByDescending(d => d.Tarih)
            .Take(take)
            .Select(d => new DiyetListDto
            {
                DiyetID = d.DiyetID,
                HastaID = d.HastaID,
                Tarih = d.Tarih,

                Ekmek = d.Ekmek,
                Tahil = d.Tahil,
                Et = d.Et,
                Peynir = d.Peynir,
                Sut = d.Sut,
                Yogurt = d.Yogurt,
                Meyve = d.Meyve,
                Sebze = d.Sebze,
                SiviGida = d.SiviGida,
                AburCubur = d.AburCubur,
                EkranSuresi = d.EkranSuresi
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/Diyetler/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DiyetDetailDto>> GetById(int id)
    {
        var d = await _db.Diyetler.AsNoTracking()
            .FirstOrDefaultAsync(x => x.DiyetID == id);

        if (d is null) return NotFound();

        return Ok(MapDetail(d));
    }

    // POST /api/Diyetler
    [HttpPost]
    public async Task<ActionResult<DiyetDetailDto>> Create([FromBody] DiyetCreateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");
        if (input.HastaID <= 0) return BadRequest("HastaID zorunludur.");

        var tarih = input.Tarih.Date;
        if (tarih > DateTime.Today)
            return BadRequest("Tarih gelecekte olamaz.");

        var exists = await _db.Diyetler.AsNoTracking()
            .AnyAsync(d => d.HastaID == input.HastaID && d.Tarih == tarih);
        if (exists)
            return Conflict("Bu hasta için bu tarihte bir diyet kaydı zaten var.");

        var e = new Diyet
        {
            HastaID = input.HastaID,
            Tarih = tarih,

            Ekmek = string.IsNullOrWhiteSpace(input.Ekmek) ? null : input.Ekmek.Trim(),
            Tahil = string.IsNullOrWhiteSpace(input.Tahil) ? null : input.Tahil.Trim(),
            Et = string.IsNullOrWhiteSpace(input.Et) ? null : input.Et.Trim(),
            Peynir = string.IsNullOrWhiteSpace(input.Peynir) ? null : input.Peynir.Trim(),
            Sut = string.IsNullOrWhiteSpace(input.Sut) ? null : input.Sut.Trim(),
            Yogurt = string.IsNullOrWhiteSpace(input.Yogurt) ? null : input.Yogurt.Trim(),
            Meyve = string.IsNullOrWhiteSpace(input.Meyve) ? null : input.Meyve.Trim(),
            Sebze = string.IsNullOrWhiteSpace(input.Sebze) ? null : input.Sebze.Trim(),
            SiviGida = string.IsNullOrWhiteSpace(input.SiviGida) ? null : input.SiviGida.Trim(),
            AburCubur = string.IsNullOrWhiteSpace(input.AburCubur) ? null : input.AburCubur.Trim(),
            EkranSuresi = string.IsNullOrWhiteSpace(input.EkranSuresi) ? null : input.EkranSuresi.Trim()
        };

        _db.Diyetler.Add(e);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = e.DiyetID }, MapDetail(e));
    }

    // PUT /api/Diyetler/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DiyetDetailDto>> Update(int id, [FromBody] DiyetUpdateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");

        var e = await _db.Diyetler.FirstOrDefaultAsync(x => x.DiyetID == id);
        if (e is null) return NotFound();

        if (input.Tarih.HasValue)
        {
            var t = input.Tarih.Value.Date;
            if (t > DateTime.Today)
                return BadRequest("Tarih gelecekte olamaz.");

            var clash = await _db.Diyetler.AsNoTracking()
                .AnyAsync(d => d.HastaID == e.HastaID && d.Tarih == t && d.DiyetID != id);
            if (clash)
                return Conflict("Bu hasta için bu tarihte başka bir diyet kaydı var.");

            e.Tarih = t;
        }

        if (input.Ekmek != null) e.Ekmek = string.IsNullOrWhiteSpace(input.Ekmek) ? null : input.Ekmek.Trim();
        if (input.Tahil != null) e.Tahil = string.IsNullOrWhiteSpace(input.Tahil) ? null : input.Tahil.Trim();
        if (input.Et != null) e.Et = string.IsNullOrWhiteSpace(input.Et) ? null : input.Et.Trim();
        if (input.Peynir != null) e.Peynir = string.IsNullOrWhiteSpace(input.Peynir) ? null : input.Peynir.Trim();
        if (input.Sut != null) e.Sut = string.IsNullOrWhiteSpace(input.Sut) ? null : input.Sut.Trim();
        if (input.Yogurt != null) e.Yogurt = string.IsNullOrWhiteSpace(input.Yogurt) ? null : input.Yogurt.Trim();
        if (input.Meyve != null) e.Meyve = string.IsNullOrWhiteSpace(input.Meyve) ? null : input.Meyve.Trim();
        if (input.Sebze != null) e.Sebze = string.IsNullOrWhiteSpace(input.Sebze) ? null : input.Sebze.Trim();
        if (input.SiviGida != null) e.SiviGida = string.IsNullOrWhiteSpace(input.SiviGida) ? null : input.SiviGida.Trim();
        if (input.AburCubur != null) e.AburCubur = string.IsNullOrWhiteSpace(input.AburCubur) ? null : input.AburCubur.Trim();
        if (input.EkranSuresi != null) e.EkranSuresi = string.IsNullOrWhiteSpace(input.EkranSuresi) ? null : input.EkranSuresi.Trim();

        await _db.SaveChangesAsync();

        return Ok(MapDetail(e));
    }

    // DELETE /api/Diyetler/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Diyetler.FindAsync(id);
        if (e is null) return NotFound();

        _db.Diyetler.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ---------------- helper ----------------

    private static DiyetDetailDto MapDetail(Diyet d) => new()
    {
        DiyetID = d.DiyetID,
        HastaID = d.HastaID,
        Tarih = d.Tarih,

        Ekmek = d.Ekmek,
        Tahil = d.Tahil,
        Et = d.Et,
        Peynir = d.Peynir,
        Sut = d.Sut,
        Yogurt = d.Yogurt,
        Meyve = d.Meyve,
        Sebze = d.Sebze,
        SiviGida = d.SiviGida,
        AburCubur = d.AburCubur,
        EkranSuresi = d.EkranSuresi
    };
}