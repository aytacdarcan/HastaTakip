using System.Linq;
using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OykulerController : ControllerBase
{
    private readonly HastaDbContext _db;
    public OykulerController(HastaDbContext db) => _db = db;

    // GET /api/Oykuler/by-hasta/2
    [HttpGet("by-hasta/{hastaId:int}")]
    public async Task<ActionResult<List<OykulerListDto>>> ByHasta(int hastaId, [FromQuery] int take = 200)
    {
        var hastaVar = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.Id == hastaId);
        if (!hastaVar) return NotFound($"Hasta bulunamadı: {hastaId}");

        take = Math.Clamp(take, 1, 500);

        var list = await _db.Oykuler
            .AsNoTracking()
            .Where(o => o.HastaID == hastaId)
            .OrderByDescending(o => o.OlusturmaTarihi)
            .Take(take)
            .Select(o => new OykulerListDto
            {
                OykulerID = o.OykulerID,
                HastaID = o.HastaID,
                OlusturmaTarihi = o.OlusturmaTarihi,
                Yakinmalar = o.Yakinmalar
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/Oykuler/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OykulerViewDto>> GetById(int id)
    {
        var o = await _db.Oykuler.AsNoTracking().FirstOrDefaultAsync(x => x.OykulerID == id);
        if (o is null) return NotFound();

        var dto = new OykulerViewDto
        {
            OykulerID = o.OykulerID,
            HastaID = o.HastaID,
            OlusturmaTarihi = o.OlusturmaTarihi,

            Yakinmalar = o.Yakinmalar,
            Oykusu = o.Oykusu,
            GebelikOykusu = o.GebelikOykusu,
            DogumSekli = o.DogumSekli,

            DogumAgirlik = o.DogumAgirlik,
            DogumBoyu = o.DogumBoyu,

            NeonatalDonem = o.NeonatalDonem,
            NorGelisim = o.NorGelisim,
            SutcocukBeslenme = o.SutcocukBeslenme,
            GecirilenHast = o.GecirilenHast,
            OperasyonKaza = o.OperasyonKaza,

            KanAkrabaligi = o.KanAkrabaligi,
            KardesSagligi = o.KardesSagligi,
            AileHastaliklar = o.AileHastaliklar
        };

        return Ok(dto);
    }

    // POST /api/Oykuler
    [HttpPost]
    public async Task<ActionResult<OykulerViewDto>> Create([FromBody] OykulerCreateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");
        if (input.HastaID <= 0) return BadRequest("HastaID zorunludur.");

        var hastaVar = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.Id == input.HastaID);
        if (!hastaVar) return NotFound($"Hasta bulunamadı: {input.HastaID}");

        var now = DateTime.Now;
        var olusturma = input.OlusturmaTarihi ?? now;
        if (olusturma > now) return BadRequest("OlusturmaTarihi gelecekte olamaz.");

        var e = new Oykuler
        {
            HastaID = input.HastaID,
            OlusturmaTarihi = olusturma,

            Yakinmalar = input.Yakinmalar,
            Oykusu = input.Oykusu,
            GebelikOykusu = input.GebelikOykusu,
            DogumSekli = string.IsNullOrWhiteSpace(input.DogumSekli) ? null : input.DogumSekli.Trim(),

            DogumAgirlik = string.IsNullOrWhiteSpace(input.DogumAgirlik) ? null : input.DogumAgirlik.Trim(),
            DogumBoyu = string.IsNullOrWhiteSpace(input.DogumBoyu) ? null : input.DogumBoyu.Trim(),

            NeonatalDonem = input.NeonatalDonem,
            NorGelisim = input.NorGelisim,
            SutcocukBeslenme = input.SutcocukBeslenme,
            GecirilenHast = input.GecirilenHast,
            OperasyonKaza = input.OperasyonKaza,

            KanAkrabaligi = string.IsNullOrWhiteSpace(input.KanAkrabaligi) ? null : input.KanAkrabaligi.Trim(),
            KardesSagligi = input.KardesSagligi,
            AileHastaliklar = input.AileHastaliklar
        };

        _db.Oykuler.Add(e);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = e.OykulerID }, new OykulerViewDto
        {
            OykulerID = e.OykulerID,
            HastaID = e.HastaID,
            OlusturmaTarihi = e.OlusturmaTarihi,

            Yakinmalar = e.Yakinmalar,
            Oykusu = e.Oykusu,
            GebelikOykusu = e.GebelikOykusu,
            DogumSekli = e.DogumSekli,

            DogumAgirlik = e.DogumAgirlik,
            DogumBoyu = e.DogumBoyu,

            NeonatalDonem = e.NeonatalDonem,
            NorGelisim = e.NorGelisim,
            SutcocukBeslenme = e.SutcocukBeslenme,
            GecirilenHast = e.GecirilenHast,
            OperasyonKaza = e.OperasyonKaza,

            KanAkrabaligi = e.KanAkrabaligi,
            KardesSagligi = e.KardesSagligi,
            AileHastaliklar = e.AileHastaliklar
        });
    }

    // PUT /api/Oykuler/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OykulerViewDto>> Update(int id, [FromBody] OykulerUpdateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");

        var e = await _db.Oykuler.FirstOrDefaultAsync(x => x.OykulerID == id);
        if (e is null) return NotFound();

        if (input.OlusturmaTarihi.HasValue)
        {
            var now = DateTime.Now;
            if (input.OlusturmaTarihi.Value > now)
                return BadRequest("OlusturmaTarihi gelecekte olamaz.");
            e.OlusturmaTarihi = input.OlusturmaTarihi.Value;
        }

        if (input.Yakinmalar != null) e.Yakinmalar = input.Yakinmalar;
        if (input.Oykusu != null) e.Oykusu = input.Oykusu;
        if (input.GebelikOykusu != null) e.GebelikOykusu = input.GebelikOykusu;
        if (input.DogumSekli != null) e.DogumSekli = string.IsNullOrWhiteSpace(input.DogumSekli) ? null : input.DogumSekli.Trim();

        if (input.DogumAgirlik != null)
            e.DogumAgirlik = string.IsNullOrWhiteSpace(input.DogumAgirlik) ? null : input.DogumAgirlik.Trim();

        if (input.DogumBoyu != null)
            e.DogumBoyu = string.IsNullOrWhiteSpace(input.DogumBoyu) ? null : input.DogumBoyu.Trim();

        if (input.NeonatalDonem != null) e.NeonatalDonem = input.NeonatalDonem;
        if (input.NorGelisim != null) e.NorGelisim = input.NorGelisim;
        if (input.SutcocukBeslenme != null) e.SutcocukBeslenme = input.SutcocukBeslenme;
        if (input.GecirilenHast != null) e.GecirilenHast = input.GecirilenHast;
        if (input.OperasyonKaza != null) e.OperasyonKaza = input.OperasyonKaza;

        if (input.KanAkrabaligi != null)
            e.KanAkrabaligi = string.IsNullOrWhiteSpace(input.KanAkrabaligi) ? null : input.KanAkrabaligi.Trim();

        if (input.KardesSagligi != null) e.KardesSagligi = input.KardesSagligi;
        if (input.AileHastaliklar != null) e.AileHastaliklar = input.AileHastaliklar;

        await _db.SaveChangesAsync();

        return Ok(new OykulerViewDto
        {
            OykulerID = e.OykulerID,
            HastaID = e.HastaID,
            OlusturmaTarihi = e.OlusturmaTarihi,

            Yakinmalar = e.Yakinmalar,
            Oykusu = e.Oykusu,
            GebelikOykusu = e.GebelikOykusu,
            DogumSekli = e.DogumSekli,

            DogumAgirlik = e.DogumAgirlik,
            DogumBoyu = e.DogumBoyu,

            NeonatalDonem = e.NeonatalDonem,
            NorGelisim = e.NorGelisim,
            SutcocukBeslenme = e.SutcocukBeslenme,
            GecirilenHast = e.GecirilenHast,
            OperasyonKaza = e.OperasyonKaza,

            KanAkrabaligi = e.KanAkrabaligi,
            KardesSagligi = e.KardesSagligi,
            AileHastaliklar = e.AileHastaliklar
        });
    }

    // DELETE /api/Oykuler/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Oykuler.FindAsync(id);
        if (e is null) return NotFound();

        _db.Oykuler.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}