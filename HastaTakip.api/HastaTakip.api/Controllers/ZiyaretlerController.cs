using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using HastaTakip.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ZiyaretlerController : ControllerBase
{
    private readonly IAntropometriSdsService _sds;
    private readonly HastaDbContext _db;

    public ZiyaretlerController(HastaDbContext db, IAntropometriSdsService sds)
    {
        _db = db;
        _sds = sds;
    }

    // POST /api/ziyaretler  → YENİ KAYIT
    [HttpPost]
    public async Task<ActionResult<ZiyaretDetailDto>> Create([FromBody] ZiyaretCreateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");
        if (input.HastaID <= 0) return BadRequest("HastaID zorunludur.");

        var hastaInfo = await _db.Hastalar
            .AsNoTracking()
            .Where(h => h.Id == input.HastaID)
            .Select(h => new { h.Ad, h.Soyad, h.BirthDate })
            .FirstOrDefaultAsync();

        if (hastaInfo is null)
            return NotFound($"Hasta bulunamadı: {input.HastaID}");

        var ziyaretTarihi = (input.Tarih == default) ? DateTime.Now : input.Tarih;

        // Validasyon
        if (ziyaretTarihi.Date < hastaInfo.BirthDate.Date)
            return BadRequest($"Ziyaret tarihi, doğum tarihinden önce olamaz. (Doğum: {hastaInfo.BirthDate:yyyy-MM-dd})");
        if (ziyaretTarihi.Date > DateTime.Today)
            return BadRequest("Ziyaret tarihi gelecekte olamaz.");
        if (!string.IsNullOrWhiteSpace(input.Notlar) && input.Notlar.Length > 500)
            return BadRequest("Notlar 500 karakteri geçemez.");

        var entity = new Ziyaret
        {
            HastaID = input.HastaID,
            Tarih = ziyaretTarihi,
            Notlar = string.IsNullOrWhiteSpace(input.Notlar) ? null : input.Notlar.Trim(),
            YakinmalarZiyaret = string.IsNullOrWhiteSpace(input.YakinmalarZiyaret) ? null : input.YakinmalarZiyaret.Trim()
        };

        _db.Ziyaretler.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                                           (sql.Number == 2601 || sql.Number == 2627))
        {
            // Benzersiz indeks ihlali (HastaID + Gün)
            return Conflict("Aynı hasta için aynı gün zaten bir ziyaret var.");
        }

        var dto = new ZiyaretDetailDto
        {
            ZiyaretID = entity.ZiyaretID,
            HastaID = entity.HastaID,
            Tarih = entity.Tarih,
            Notlar = entity.Notlar,
            YakinmalarZiyaret = entity.YakinmalarZiyaret,
            HastaAd = hastaInfo.Ad,
            HastaSoyad = hastaInfo.Soyad
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.ZiyaretID }, dto);
    }

    // GET /api/ziyaretler/by-hasta/{hastaId} → hastanın ziyaretleri
    [HttpGet("by-hasta/{hastaId:int}")]
    public async Task<ActionResult<List<ZiyaretListDto>>> ByHasta(int hastaId)
    {
        var list = await _db.Ziyaretler
            .Include(z => z.Hasta)
            .AsNoTracking()
            .Where(z => z.HastaID == hastaId)
            .OrderByDescending(z => z.Tarih).ThenByDescending(z => z.ZiyaretID)
            .Take(200)
            .Select(z => new ZiyaretListDto
            {
                ZiyaretID = z.ZiyaretID,
                HastaID = z.HastaID,
                Tarih = z.Tarih,
                Notlar = z.Notlar,
                YakinmalarZiyaret = z.YakinmalarZiyaret,
                HastaAd = z.Hasta != null ? z.Hasta.Ad : null,
                HastaSoyad = z.Hasta != null ? z.Hasta.Soyad : null
            })
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/ziyaretler/{id} → detay
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ZiyaretDetailDto>> GetById(int id)
    {
        var z = await _db.Ziyaretler
            .AsNoTracking()
            .Include(x => x.Hasta)
            .Where(x => x.ZiyaretID == id)
            .Select(x => new ZiyaretDetailDto
            {
                ZiyaretID = x.ZiyaretID,
                HastaID = x.HastaID,
                Tarih = x.Tarih,
                Notlar = x.Notlar,
                YakinmalarZiyaret = x.YakinmalarZiyaret,
                HastaAd = x.Hasta.Ad,
                HastaSoyad = x.Hasta.Soyad
            })
            .FirstOrDefaultAsync();

        return z is null ? NotFound() : Ok(z);
    }

    // DELETE /api/ziyaretler/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Ziyaretler.FindAsync(id);
        if (entity is null) return NotFound();

        var hasAnt = await _db.Antropometriler.AsNoTracking().AnyAsync(a => a.ZiyaretID == id);
        var hasLab = await _db.LabSonuclari.AsNoTracking().AnyAsync(l => l.ZiyaretID == id);
        if (hasAnt || hasLab)
            return Conflict("Bu ziyarete bağlı kayıtlar var (Antropometri/Lab). Önce onları silin.");

        _db.Ziyaretler.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/ziyaretler/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ZiyaretDetailDto>> Update(int id, [FromBody] ZiyaretUpdateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");

        var entity = await _db.Ziyaretler.FirstOrDefaultAsync(z => z.ZiyaretID == id);
        if (entity is null) return NotFound();

        var hastaInfo = await _db.Hastalar
            .AsNoTracking()
            .Where(h => h.Id == entity.HastaID)
            .Select(h => new { h.Ad, h.Soyad, h.BirthDate })
            .FirstOrDefaultAsync();

        if (hastaInfo is null)
            return NotFound($"Hasta bulunamadı: {entity.HastaID}");

        var yeniTarih = input.Tarih ?? entity.Tarih;

        if (yeniTarih.Date < hastaInfo.BirthDate.Date)
            return BadRequest($"Ziyaret tarihi doğumdan önce olamaz. (Doğum: {hastaInfo.BirthDate:yyyy-MM-dd})");
        if (yeniTarih.Date > DateTime.Today)
            return BadRequest("Ziyaret tarihi gelecekte olamaz.");
        if (input.Notlar != null && input.Notlar.Length > 500)
            return BadRequest("Notlar 500 karakteri geçemez.");

        entity.Tarih = yeniTarih;
        if (input.Notlar != null)
            entity.Notlar = string.IsNullOrWhiteSpace(input.Notlar) ? null : input.Notlar.Trim();

        // 🔽 Yakınmalar güncelle
        if (input.YakinmalarZiyaret != null)
            entity.YakinmalarZiyaret = string.IsNullOrWhiteSpace(input.YakinmalarZiyaret)
                ? null
                : input.YakinmalarZiyaret.Trim();

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                                           (sql.Number == 2601 || sql.Number == 2627))
        {
            return Conflict("Aynı hasta için aynı gün zaten bir ziyaret var.");
        }

        var dto = new ZiyaretDetailDto
        {
            ZiyaretID = entity.ZiyaretID,
            HastaID = entity.HastaID,
            Tarih = entity.Tarih,
            Notlar = entity.Notlar,
            YakinmalarZiyaret = entity.YakinmalarZiyaret,
            HastaAd = hastaInfo.Ad,
            HastaSoyad = hastaInfo.Soyad
        };

        return Ok(dto);
    }

    // GET /api/Ziyaretler/{id}/bundle
    [HttpGet("{id:int}/Bundle")]
    public async Task<ActionResult<ZiyaretBundleDto>> GetBundle(int id)
    {
        var ziyaret = await _db.Ziyaretler
            .AsNoTracking()
            .Include(z => z.Hasta)
            .Where(z => z.ZiyaretID == id)
            .Select(z => new ZiyaretDetailDto
            {
                ZiyaretID = z.ZiyaretID,
                HastaID = z.HastaID,
                Tarih = z.Tarih,
                Notlar = z.Notlar,
                YakinmalarZiyaret = z.YakinmalarZiyaret,   // 👈 API tarafında var
                HastaAd = z.Hasta.Ad,
                HastaSoyad = z.Hasta.Soyad,
                BirthDate = z.Hasta.BirthDate
            })
            .FirstOrDefaultAsync();

        if (ziyaret is null)
            return NotFound($"Ziyaret bulunamadı: {id}");

        // 🔹 Antropometri: tek kayıt (en günceli)
        var antropometri = await _db.Antropometriler
            .AsNoTracking()
            .Where(a => a.ZiyaretID == id)
            .OrderByDescending(a => a.AntropometriID)
            .Select(a => new AntropometriListDto
            {
                AntropometriID = a.AntropometriID,
                ZiyaretID = a.ZiyaretID,
                YasAy = a.YasAy,
                BoyCm = a.BoyCm,
                KiloKg = a.KiloKg,
                BasCevresiCm = a.BasCevresiCm,
                OturmaBoyuCm = a.OturmaBoyuCm,
                ObTb = a.ObTb,
                GogusCevresiCm = a.GogusCevresiCm,
                BasPubisCm = a.BasPubisCm,
                PubisTopukCm = a.PubisTopukCm,
                BoySDS = a.BoySDS,
                KiloSDS = a.KiloSDS,
                BKISDS = a.BKISDS,
                BasCevresiSDS = a.BasCevresiSDS,
                YBHSDS = a.YBHSDS,
                BKI = a.BKI
            })
                .FirstOrDefaultAsync();


        // Puberte (son/tek)
        var puberte = await _db.PuberteFizikler
            .AsNoTracking()
            .Where(p => p.ZiyaretID == id)
            .OrderByDescending(p => p.PuberteID)
            .Select(p => new PuberteFizikViewDto
            {
                PuberteID = p.PuberteID,
                ZiyaretID = p.ZiyaretID,
                PuberteNotu = p.PuberteNotu,
                PatolojikFizik = p.PatolojikFizik
            })
            .FirstOrDefaultAsync();

        // Yorum/Plan 
        var yorumPlan = await _db.YorumPlanlar
            .AsNoTracking()
            .Where(y => y.ZiyaretID == id)
            .OrderByDescending(y => y.OlusturmaTarihi).ThenByDescending(y => y.YorumID)
            .Select(y => new YorumPlanViewDto
            {
                YorumID = y.YorumID,
                ZiyaretID = y.ZiyaretID,
                TedaviBeslenmeSpor = y.TedaviBeslenmeSpor,
                YorumNotlar = y.YorumNotlar,
                OlusturmaTarihi = y.OlusturmaTarihi
            })
            .FirstOrDefaultAsync();

        
        var ozet = await _db.OzetHesaplar
            .AsNoTracking()
            .Where(o => o.ZiyaretID == id)
            .OrderByDescending(o => o.OzetID)
            .Select(o => new OzetHesapDto
            {
                OzetID = o.OzetID,
                ZiyaretID = o.ZiyaretID,
                KulacCm = o.KulacCm,
                HedefBoyCm = o.HedefBoyCm,
                BoyaUyanTarti = o.BoyaUyanTarti
            })

            .FirstOrDefaultAsync();

        return Ok(new ZiyaretBundleDto
        {
            Ziyaret = ziyaret,
            Antropometri = antropometri,
            Puberte = puberte,
            YorumPlan = yorumPlan,
            Ozet = ozet
        });
    }
    // 🔹 POST: /api/Ziyaretler/bundle
    [HttpPost("bundle")]
    public async Task<ActionResult<object>> CreateBundle([FromBody] ZiyaretBundleCreateDto dto)
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        
        var z = new Ziyaret
        {
            HastaID = dto.Ziyaret.HastaID,
            Tarih = dto.Ziyaret.Tarih,
            Notlar = dto.Ziyaret.Notlar,
            YakinmalarZiyaret = dto.Ziyaret.YakinmalarZiyaret
        };
        _db.Ziyaretler.Add(z);
        await _db.SaveChangesAsync();
        var vid = z.ZiyaretID;

        // 2️⃣ Antropometri
        if (dto.Antropometri is not null)
        {
            var a = new Antropometri
            {
                ZiyaretID = vid,
                BoyCm = dto.Antropometri.BoyCm,
                KiloKg = dto.Antropometri.KiloKg,
                BasCevresiCm = dto.Antropometri.BasCevresiCm,
                OturmaBoyuCm = dto.Antropometri.OturmaBoyuCm,
                ObTb = dto.Antropometri.ObTb,
                GogusCevresiCm = dto.Antropometri.GogusCevresiCm,
                BasPubisCm = dto.Antropometri.BasPubisCm,
                PubisTopukCm = dto.Antropometri.PubisTopukCm
            };
            _db.Antropometriler.Add(a);
            await _db.SaveChangesAsync();
            await _db.Entry(a).ReloadAsync();
            await _sds.RecalcByAntropometriAsync(a.AntropometriID);
        }

        // 3️⃣ Puberte
        if (dto.Puberte is not null)
        {
            var p = new PuberteFizik
            {
                ZiyaretID = vid,
                PuberteNotu = dto.Puberte.PuberteNotu,
                PatolojikFizik = dto.Puberte.PatolojikFizik
            };
            _db.PuberteFizikler.Add(p);
        }

        // 4️⃣ Yorum / Plan
        if (dto.YorumPlan is not null)
        {
            var y = new YorumPlan
            {
                ZiyaretID = vid,
                TedaviBeslenmeSpor = dto.YorumPlan.TedaviBeslenmeSpor,
                YorumNotlar = dto.YorumPlan.YorumNotlar,
                OlusturmaTarihi = DateTime.Now
            };
            _db.YorumPlanlar.Add(y);
        }

        // 5️⃣ Özet
        if (dto.Ozet is not null)
        {
            var o = new OzetHesapKlinik
            {
                ZiyaretID = vid,
                KulacCm = dto.Ozet.KulacCm,
                HedefBoyCm = dto.Ozet.HedefBoyCm,
                BoyaUyanTarti = dto.Ozet.BoyaUyanTarti
            };
            _db.OzetHesaplar.Add(o);
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { id = vid });
    }
}
