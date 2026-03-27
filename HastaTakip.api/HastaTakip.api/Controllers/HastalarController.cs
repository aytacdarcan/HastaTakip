using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using HastaTakip.Api.Models;
using HastaTakip.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HastalarController : ControllerBase
{
    private readonly HastaDbContext _db;
    private readonly IAntropometriSdsService _sds;

    public HastalarController(
    HastaDbContext db,
    IAntropometriSdsService sds)
    {
        _db = db;
        _sds = sds;
    }

    // GET /api/Hastalar  → tüm hastalar (yeni → eski)
    [HttpGet]
    public async Task<ActionResult<List<PatientListDto>>> GetAll()
    {
        var list = await _db.Hastalar
            .AsNoTracking()
            .OrderByDescending(h => h.KayitTarihi)
            .Select(h => new PatientListDto
            {
                Id = h.Id,
                Ad = h.Ad,
                Soyad = h.Soyad,
                BirthDate = h.BirthDate
            })
            .ToListAsync();

        return Ok(list); 
    }

    // GET /api/Hastalar/{id}  → tek hasta detayı
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PatientDetailDto>> GetById(int id)
    {
        var h = await _db.Hastalar
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PatientDetailDto
            {
                Id = x.Id,
                Ad = x.Ad,
                Soyad = x.Soyad,
                BirthDate = x.BirthDate,
                Cinsiyet = x.Cinsiyet,
                
                TcKimlikNo = x.TcKimlikNo,
                Telefon = x.Telefon,
                Email = x.Email,
                Adres = x.Adres
            })
            .FirstOrDefaultAsync();

        if (h is null)
            return NotFound($"Hasta bulunamadı: {id}");

        return Ok(h);
    }

    // GET /api/Hastalar/search?q=ahmet
    [HttpGet("search")]
    public async Task<ActionResult<List<PatientListDto>>> Search([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("q parametresi zorunlu.");

        q = q.Trim();

        var list = await _db.Hastalar
            .AsNoTracking()
            .Where(h =>
                EF.Functions.Like(h.Ad, $"%{q}%") ||
                EF.Functions.Like(h.Soyad, $"%{q}%") ||
                (h.TcKimlikNo != null && EF.Functions.Like(h.TcKimlikNo, $"%{q}%"))
            )
            .OrderByDescending(h => h.KayitTarihi)
            .Select(h => new PatientListDto
            {
                Id = h.Id,
                Ad = h.Ad,
                Soyad = h.Soyad,
                BirthDate = h.BirthDate
            })
            .Take(50)
            .ToListAsync();

        return Ok(list);
    }

    // POST /api/Hastalar  
    [HttpPost]
    public async Task<ActionResult<PatientDetailDto>> Create([FromBody] PatientCreateDto input)
    {
        if (input is null) return BadRequest("Gövde (body) boş olamaz.");

        if (input.BirthDate.Date > DateTime.Today)
            return BadRequest("Doğum tarihi gelecekte olamaz.");

        var cins = (input.Cinsiyet ?? "").Trim().ToUpperInvariant();
        if (cins is not ("E" or "K"))
            return BadRequest("Cinsiyet 'E' veya 'K' olmalıdır.");

        
        var tc = (input.TcKimlikNo ?? "").Trim();
        if (tc.Length != 11 || !tc.All(char.IsDigit) || tc[0] == '0')
            return BadRequest("TcKimlikNo 11 haneli, rakamlardan oluşmalı ve 0 ile başlamamalıdır.");

        var exists = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.TcKimlikNo == tc);
        if (exists)
            return Conflict("Bu TC Kimlik Numarası zaten kayıtlı.");

        var entity = new Patient
        {
            Ad = input.Ad.Trim(),
            Soyad = input.Soyad.Trim(),
            BirthDate = input.BirthDate.Date,
            Cinsiyet = cins,
            TcKimlikNo = tc,
            Telefon = string.IsNullOrWhiteSpace(input.Telefon) ? null : input.Telefon.Trim(),
            Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim(),
            Adres = string.IsNullOrWhiteSpace(input.Adres) ? null : input.Adres.Trim(),
            KayitTarihi = DateTime.Now
        };

        _db.Hastalar.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                                           (sql.Number == 2627 || sql.Number == 2601))
        {
            return Conflict("TC Kimlik Numarası benzersiz olmalıdır.");
        }

        var dto = new PatientDetailDto
        {
            Id = entity.Id,
            Ad = entity.Ad,
            Soyad = entity.Soyad,
            BirthDate = entity.BirthDate,
            Cinsiyet = entity.Cinsiyet
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, dto);
    }

    // PUT /api/Hastalar/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PatientDetailDto>> Update(int id, [FromBody] PatientUpdateDto input)
    {
        bool sexChanged = false;
        bool birthDateChanged = false;

        if (input is null)
            return BadRequest("Gövde (body) boş olamaz.");

        var entity = await _db.Hastalar.FirstOrDefaultAsync(h => h.Id == id);
        if (entity is null)
            return NotFound($"Hasta bulunamadı: {id}");

        
        if (input.BirthDate.HasValue && input.BirthDate.Value.Date > DateTime.Today)
            return BadRequest("Doğum tarihi gelecekte olamaz.");

        
        if (!string.IsNullOrWhiteSpace(input.Cinsiyet))
        {
            var c = input.Cinsiyet.Trim().ToUpperInvariant();
            if (c is not ("E" or "K"))
                return BadRequest("Cinsiyet 'E' veya 'K' olmalıdır.");

            if (entity.Cinsiyet != c)
            {
                entity.Cinsiyet = c;
                sexChanged = true;
            }
        }

        
        if (input.TcKimlikNo != null)
        {
            var tc = input.TcKimlikNo.Trim();
            if (tc.Length != 11 || !tc.All(char.IsDigit) || tc[0] == '0')
                return BadRequest("TcKimlikNo 11 haneli, rakamlardan oluşmalı ve 0 ile başlamamalıdır.");

            if (!string.Equals(tc, entity.TcKimlikNo, StringComparison.Ordinal))
            {
                var exists = await _db.Hastalar.AsNoTracking()
                    .AnyAsync(h => h.TcKimlikNo == tc && h.Id != id);

                if (exists)
                    return Conflict("Bu TC Kimlik Numarası zaten kayıtlı.");

                entity.TcKimlikNo = tc;
            }
        }

        
        if (!string.IsNullOrWhiteSpace(input.Ad))
            entity.Ad = input.Ad.Trim();

        if (!string.IsNullOrWhiteSpace(input.Soyad))
            entity.Soyad = input.Soyad.Trim();

        if (input.BirthDate.HasValue)
        {
            var newBirth = input.BirthDate.Value.Date;
            if (entity.BirthDate != newBirth)
            {
                entity.BirthDate = newBirth;
                birthDateChanged = true;
            }
        }

       
        if (input.Telefon != null)
            entity.Telefon = string.IsNullOrWhiteSpace(input.Telefon) ? null : input.Telefon.Trim();

        if (input.Email != null)
            entity.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();

        if (input.Adres != null)
            entity.Adres = string.IsNullOrWhiteSpace(input.Adres) ? null : input.Adres.Trim();

        try
        {
            await _db.SaveChangesAsync();

            
            if (sexChanged || birthDateChanged)
            {
                await _sds.RecalcByHastaAsync(entity.Id);
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                                           (sql.Number == 2627 || sql.Number == 2601))
        {
            return Conflict("TC Kimlik Numarası benzersiz olmalıdır.");
        }

        var dto = new PatientDetailDto
        {
            Id = entity.Id,
            Ad = entity.Ad,
            Soyad = entity.Soyad,
            BirthDate = entity.BirthDate,
            Cinsiyet = entity.Cinsiyet
        };

        return Ok(dto);
    }


    // DELETE /api/Hastalar/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Hastalar.FindAsync(id);
        if (entity is null)
            return NotFound($"Hasta bulunamadı: {id}");

        // Bu hastaya ait ziyaret var mı? Varsa silmeye izin verme
        var hasVisits = await _db.Ziyaretler.AsNoTracking().AnyAsync(z => z.HastaID == id);
        if (hasVisits)
            return Conflict("Hastanın ziyaret kayıtları var. Önce ilgili ziyaret/alt verileri silin.");

        _db.Hastalar.Remove(entity);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sql && sql.Number == 547)
        {
            return Conflict("Hasta başka kayıtlarla ilişkili olduğu için silinemedi (FK kısıtı).");
        }

        return NoContent(); // 204
    }

    // GET /api/Hastalar/{id}/ziyaretler 
    [HttpGet("{id:int}/ziyaretler")]
    public async Task<ActionResult<List<ZiyaretListWithCountDto>>> GetZiyaretlerByHasta(int id)
    {
        var hastaVarMi = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.Id == id);
        if (!hastaVarMi) return NotFound($"Hasta bulunamadı: {id}");

        var list = await _db.Ziyaretler
            .AsNoTracking()
            .Where(z => z.HastaID == id)
            .GroupJoin(
                _db.Antropometriler.AsNoTracking(),
                z => z.ZiyaretID,
                a => a.ZiyaretID,
                (z, aGroup) => new { z, adet = aGroup.Count() }
            )
            .OrderByDescending(x => x.z.Tarih).ThenByDescending(x => x.z.ZiyaretID)
            .Select(x => new ZiyaretListWithCountDto
            {
                ZiyaretID = x.z.ZiyaretID,
                HastaID = x.z.HastaID,
                Tarih = x.z.Tarih,
                Notlar = x.z.Notlar,
                HastaAd = x.z.Hasta.Ad,     
                HastaSoyad = x.z.Hasta.Soyad,
                AntropometriAdet = x.adet
            })
            .ToListAsync();

        return Ok(list); // yoksa []
    }

    // GET /api/Hastalar/paged?page=1&pageSize=25&q=ahmet&sort=birthdate&dir=asc
    [HttpGet("paged")]
    public async Task<ActionResult<object>> GetPaged(
        int page = 1,
        int pageSize = 25,
        string? q = null,
        string? sort = null,   
        string? dir = null)    
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var baseQuery = _db.Hastalar.AsNoTracking();

        
        if (!string.IsNullOrWhiteSpace(q))
        {
            var tokens = q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var t in tokens)
            {
                var token = t; 
                baseQuery = baseQuery.Where(h =>
                    EF.Functions.Like(EF.Functions.Collate(h.Ad, "Turkish_CI_AI"), $"%{token}%") ||
                    EF.Functions.Like(EF.Functions.Collate(h.Soyad, "Turkish_CI_AI"), $"%{token}%") ||
                    EF.Functions.Like(EF.Functions.Collate(h.Ad + " " + h.Soyad, "Turkish_CI_AI"), $"%{token}%") ||
                    (h.TcKimlikNo != null && h.TcKimlikNo.StartsWith(token))
                );
            }
        }

        
        bool asc = string.Equals(dir, "asc", StringComparison.OrdinalIgnoreCase);
        switch (sort?.ToLowerInvariant())
        {
            case "birthdate":
                baseQuery = asc
                    ? baseQuery.OrderBy(h => (DateTime?)h.BirthDate ?? DateTime.MaxValue).ThenBy(h => h.Id)
                    : baseQuery.OrderByDescending(h => (DateTime?)h.BirthDate ?? DateTime.MinValue).ThenByDescending(h => h.Id);
                break;


            case "kayittarihi":
                baseQuery = asc
                    ? baseQuery.OrderBy(h => h.KayitTarihi).ThenBy(h => h.Id)
                    : baseQuery.OrderByDescending(h => h.KayitTarihi).ThenByDescending(h => h.Id);
                break;

            case "ad":
                baseQuery = asc
                    ? baseQuery.OrderBy(h => h.Ad).ThenBy(h => h.Soyad).ThenBy(h => h.Id)
                    : baseQuery.OrderByDescending(h => h.Ad).ThenByDescending(h => h.Soyad).ThenByDescending(h => h.Id);
                break;

            case "soyad":
                baseQuery = asc
                    ? baseQuery.OrderBy(h => h.Soyad).ThenBy(h => h.Ad).ThenBy(h => h.Id)
                    : baseQuery.OrderByDescending(h => h.Soyad).ThenByDescending(h => h.Ad).ThenByDescending(h => h.Id);
                break;

            case "cinsiyet":
                baseQuery = asc
                    ? baseQuery.OrderBy(h => h.Cinsiyet).ThenBy(h => h.Ad).ThenBy(h => h.Id)
                    : baseQuery.OrderByDescending(h => h.Cinsiyet).ThenByDescending(h => h.Ad).ThenByDescending(h => h.Id);
                break;

            case "id":
                baseQuery = asc
                    ? baseQuery.OrderBy(h => h.Id)
                    : baseQuery.OrderByDescending(h => h.Id);
                break;

            default:
                
                baseQuery = baseQuery.OrderByDescending(h => h.KayitTarihi).ThenByDescending(h => h.Id);
                break;
        }

        var total = await baseQuery.CountAsync();

        var items = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new PatientListDto
            {
                Id = h.Id,
                Ad = h.Ad,
                Soyad = h.Soyad,
                BirthDate = h.BirthDate,
                Cinsiyet = h.Cinsiyet,
                KayitTarihi = h.KayitTarihi
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }


    // GET /api/Hastalar/by-tc/12345678901
    [HttpGet("by-tc/{tc}")]
    public async Task<ActionResult<PatientDetailDto>> GetByTc(string tc)
    {
        tc = (tc ?? "").Trim();

        if (tc.Length != 11 || !tc.All(char.IsDigit) || tc[0] == '0')
            return BadRequest("TcKimlikNo 11 haneli, rakamlardan oluşmalı ve 0 ile başlamamalıdır.");

        var h = await _db.Hastalar
            .AsNoTracking()
            .Where(x => x.TcKimlikNo == tc)
            .Select(x => new PatientDetailDto
            {
                Id = x.Id,
                Ad = x.Ad,
                Soyad = x.Soyad,
                BirthDate = x.BirthDate,
                Cinsiyet = x.Cinsiyet
            })
            .FirstOrDefaultAsync();

        if (h is null)
            return NotFound($"TC ile hasta bulunamadı: {tc}");

        return Ok(h);
    }

    // GET /api/Hastalar/{id}/ozet
    [HttpGet("{id:int}/ozet")]
    public async Task<ActionResult<PatientSummaryDto>> GetSummary(int id)
    {
        var hasta = await _db.Hastalar.AsNoTracking()
            .Where(h => h.Id == id)
            .Select(h => new { h.Id, h.Ad, h.Soyad, h.BirthDate })
            .FirstOrDefaultAsync();

        if (hasta is null) return NotFound($"Hasta bulunamadı: {id}");

        var visitQuery = _db.Ziyaretler.AsNoTracking().Where(z => z.HastaID == id);
        var ziyaretSayisi = await visitQuery.CountAsync();
        DateTime? sonZiyaret = null;
        if (ziyaretSayisi > 0)
            sonZiyaret = await visitQuery.MaxAsync(z => (DateTime?)z.Tarih);

        var sonAnt = await _db.Antropometriler.AsNoTracking()
            .Where(a => a.Ziyaret.HastaID == id)
            .OrderByDescending(a => a.Ziyaret.Tarih)
            .ThenByDescending(a => a.AntropometriID)
            .Select(a => new { a.BoyCm, a.KiloKg, a.BKI })
            .FirstOrDefaultAsync();

        var dto = new PatientSummaryDto
        {
            Id = hasta.Id,
            Ad = hasta.Ad,
            Soyad = hasta.Soyad,
            BirthDate = hasta.BirthDate,
            ZiyaretSayisi = ziyaretSayisi,
            SonZiyaretTarihi = sonZiyaret,
            SonBoyCm = sonAnt?.BoyCm,
            SonKiloKg = sonAnt?.KiloKg,
            SonBKI = sonAnt?.BKI
        };

        return Ok(dto);
    }

    // GET /api/Hastalar/{id}/ziyaretler-detay?page=1&pageSize=10&dateFrom=2025-09-01&dateTo=2025-09-30
    [HttpGet("{id:int}/ziyaretler-detay")]
    public async Task<ActionResult<object>> GetZiyaretlerDetay(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var hastaVar = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.Id == id);
        if (!hastaVar) return NotFound($"Hasta bulunamadı: {id}");

        // 1) Tarih aralığı filtreli base query
        var baseQuery = _db.Ziyaretler.AsNoTracking().Where(z => z.HastaID == id);

        if (dateFrom.HasValue)
            baseQuery = baseQuery.Where(z => z.Tarih >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            baseQuery = baseQuery.Where(z => z.Tarih < dateTo.Value.Date.AddDays(1));

        var total = await baseQuery.CountAsync();

        // 2) Sayfalı ziyaretler
        var visitPage = await baseQuery
            .OrderByDescending(z => z.Tarih).ThenByDescending(z => z.ZiyaretID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(z => new ZiyaretWithAnthrosDto
            {
                ZiyaretID = z.ZiyaretID,
                Tarih = z.Tarih,
                Notlar = z.Notlar,
                
                YakinmalarZiyaret = z.YakinmalarZiyaret
            })
            .ToListAsync();

        if (visitPage.Count == 0)
            return Ok(new { total, page, pageSize, items = Array.Empty<ZiyaretWithAnthrosDto>() });

        // 3) Bu sayfadaki ziyaretlerin antropometrileri
        var visitIds = visitPage.Select(v => v.ZiyaretID).ToList();

        var antropList = await _db.Antropometriler.AsNoTracking()
            .Where(a => visitIds.Contains(a.ZiyaretID))
            .OrderByDescending(a => a.AntropometriID)
            .Select(a => new
            {
                a.ZiyaretID,
                Mini = new AntropometriMiniDto
                {
                    AntropometriID = a.AntropometriID,
                    BoyCm = a.BoyCm,
                    KiloKg = a.KiloKg,
                    BKI = a.BKI,
                    BasCevresiCm = a.BasCevresiCm
                }
            })
            .ToListAsync();

        var byVisit = antropList
            .GroupBy(x => x.ZiyaretID)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Mini).ToList());

        foreach (var v in visitPage)
        {
            if (byVisit.TryGetValue(v.ZiyaretID, out var minis))
            {
                v.Antropometriler = minis;
                v.AntropometriAdet = minis.Count;
            }
            else
            {
                v.Antropometriler = new();
                v.AntropometriAdet = 0;
            }
        }

        return Ok(new { total, page, pageSize, items = visitPage });
    }

    // POST /api/Hastalar/register
    [HttpPost("register")]
    public async Task<ActionResult<object>> Register([FromBody] PatientRegisterDto input)
    {
        if (input is null || input.Hasta is null)
            return BadRequest("Hasta bilgisi zorunludur.");

        
        if (input.Hasta.BirthDate.Date > DateTime.Today)
            return BadRequest("Doğum tarihi gelecekte olamaz.");
        var cins = (input.Hasta.Cinsiyet ?? "").Trim().ToUpperInvariant();
        if (cins is not ("E" or "K"))
            return BadRequest("Cinsiyet 'E' veya 'K' olmalıdır.");

        if (string.IsNullOrWhiteSpace(input.Hasta.TcKimlikNo))
            return BadRequest("TcKimlikNo zorunludur.");

        var tc = input.Hasta.TcKimlikNo!.Trim();
        if (tc.Length != 11 || !tc.All(char.IsDigit) || tc[0] == '0')
            return BadRequest("TcKimlikNo 11 haneli, rakamlardan oluşmalı ve 0 ile başlamamalıdır.");

        var exists = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.TcKimlikNo == tc);
        if (exists) return Conflict("Bu TC Kimlik Numarası zaten kayıtlı.");

        using var trx = await _db.Database.BeginTransactionAsync();
        try
        {
            // 1) Hasta
            var hasta = new Patient
            {
                Ad = input.Hasta.Ad.Trim(),
                Soyad = input.Hasta.Soyad.Trim(),
                BirthDate = input.Hasta.BirthDate.Date,
                Cinsiyet = cins,
                TcKimlikNo = tc,
                Telefon = string.IsNullOrWhiteSpace(input.Hasta.Telefon) ? null : input.Hasta.Telefon.Trim(),
                Email = string.IsNullOrWhiteSpace(input.Hasta.Email) ? null : input.Hasta.Email.Trim(),
                Adres = string.IsNullOrWhiteSpace(input.Hasta.Adres) ? null : input.Hasta.Adres.Trim(),
                KayitTarihi = DateTime.Now
            };
            _db.Hastalar.Add(hasta);
            await _db.SaveChangesAsync();

            // 2) Ziyaret (opsiyonel; ama alt veriler için gerekecek)
            int? ziyaretId = null;
            DateTime? ziyaretTarih = null;

            if (input.Ziyaret is not null
                || input.Antropometri is not null
                || input.PuberteFizik is not null
                || input.YorumPlan is not null
                || input.OzetHesap is not null) 
            {
                var zDate = (input.Ziyaret?.Tarih ?? DateTime.Today).Date;
                if (zDate < hasta.BirthDate.Date) return BadRequest("Ziyaret tarihi doğumdan önce olamaz.");
                if (zDate > DateTime.Today) return BadRequest("Ziyaret tarihi gelecekte olamaz.");

                var zEntity = new Ziyaret
                {
                    HastaID = hasta.Id,
                    Tarih = zDate,
                    Notlar = (input.Ziyaret?.Notlar?.Length ?? 0) > 500 ? input.Ziyaret!.Notlar!.Substring(0, 500) : input.Ziyaret?.Notlar,
                    YakinmalarZiyaret = string.IsNullOrWhiteSpace(input.Ziyaret?.YakinmalarZiyaret) ? null : input.Ziyaret!.YakinmalarZiyaret!.Trim()
                };
                _db.Ziyaretler.Add(zEntity);
                try
                {
                    await _db.SaveChangesAsync();
                    ziyaretId = zEntity.ZiyaretID;
                    ziyaretTarih = zEntity.Tarih;
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
                {
                    var existing = await _db.Ziyaretler.AsNoTracking()
                                     .FirstOrDefaultAsync(z => z.HastaID == hasta.Id && z.Tarih == zDate);
                    if (existing is null) throw;
                    ziyaretId = existing.ZiyaretID;
                    ziyaretTarih = existing.Tarih;
                }
            }

            // 3) (ops) Aile üyeleri
            int aileCount = 0;
            if (input.AileUyeleri is { Count: > 0 })
            {
                if (input.AileUyeleri.Any(u => string.IsNullOrWhiteSpace(u.Iliski)))
                    return BadRequest("Aile üyesi 'Iliski' alanı zorunludur.");

                var aileEntities = input.AileUyeleri.Select(a => new AileUyesi
                {
                    HastaID = hasta.Id,
                    Iliski = a.Iliski!.Trim(),
                    Ad = string.IsNullOrWhiteSpace(a.Ad) ? null : a.Ad.Trim(),
                    DogumTarihi = a.DogumTarihi,
                    BoyCm = a.BoyCm,
                    AgirlikKg = a.AgirlikKg,
                    PuberteYasiYil = a.PuberteYasiYil,
                    SaglikDurumu = a.SaglikDurumu,
                    Meslek = a.Meslek
                }).ToList();

                _db.AileUyeleri.AddRange(aileEntities);
                aileCount = await _db.SaveChangesAsync();
            }

            // 4) (ops) Öyküler
            int? oykulerId = null;
            if (input.Oykuler is not null)
            {
                var oyku = new Oykuler
                {
                    HastaID = hasta.Id,
                    OlusturmaTarihi = input.Oykuler.OlusturmaTarihi ?? DateTime.Now,
                    Yakinmalar = input.Oykuler.Yakinmalar,
                    Oykusu = input.Oykuler.Oykusu,
                    GebelikOykusu = input.Oykuler.GebelikOykusu,
                    DogumSekli = string.IsNullOrWhiteSpace(input.Oykuler.DogumSekli) ? null : input.Oykuler.DogumSekli.Trim(),
                    DogumAgirlik = string.IsNullOrWhiteSpace(input.Oykuler.DogumAgirlik) ? null : input.Oykuler.DogumAgirlik.Trim(),
                    DogumBoyu = string.IsNullOrWhiteSpace(input.Oykuler.DogumBoyu) ? null : input.Oykuler.DogumBoyu.Trim(),
                    NeonatalDonem = input.Oykuler.NeonatalDonem,
                    NorGelisim = input.Oykuler.NorGelisim,
                    SutcocukBeslenme = input.Oykuler.SutcocukBeslenme,
                    GecirilenHast = input.Oykuler.GecirilenHast,
                    OperasyonKaza = input.Oykuler.OperasyonKaza,
                    KanAkrabaligi = input.Oykuler.KanAkrabaligi,
                    KardesSagligi = input.Oykuler.KardesSagligi,
                    AileHastaliklar = input.Oykuler.AileHastaliklar,
                    
                };
                _db.Oykuler.Add(oyku);
                await _db.SaveChangesAsync();
                oykulerId = oyku.OykulerID;
            }

            // 5) (ops) Diyetler
            int diyetCount = 0;
            if (input.Diyetler is { Count: > 0 })
            {
                var diyetEntities = input.Diyetler.Select(d => new Diyet
                {
                    HastaID = hasta.Id,
                    Tarih = d.Tarih == default ? DateTime.Today : d.Tarih.Date,

                    Ekmek = string.IsNullOrWhiteSpace(d.Ekmek) ? null : d.Ekmek.Trim(),
                    Tahil = string.IsNullOrWhiteSpace(d.Tahil) ? null : d.Tahil.Trim(),

                    Et = string.IsNullOrWhiteSpace(d.Et) ? null : d.Et.Trim(),
                    Peynir = string.IsNullOrWhiteSpace(d.Peynir) ? null : d.Peynir.Trim(),

                    Sut = string.IsNullOrWhiteSpace(d.Sut) ? null : d.Sut.Trim(),
                    Yogurt = string.IsNullOrWhiteSpace(d.Yogurt) ? null : d.Yogurt.Trim(),

                    Meyve = string.IsNullOrWhiteSpace(d.Meyve) ? null : d.Meyve.Trim(),
                    Sebze = string.IsNullOrWhiteSpace(d.Sebze) ? null : d.Sebze.Trim(),

                    SiviGida = string.IsNullOrWhiteSpace(d.SiviGida) ? null : d.SiviGida.Trim(),
                    AburCubur = string.IsNullOrWhiteSpace(d.AburCubur) ? null : d.AburCubur.Trim(),
                    EkranSuresi = string.IsNullOrWhiteSpace(d.EkranSuresi) ? null : d.EkranSuresi.Trim()
                }).ToList();

                _db.Diyetler.AddRange(diyetEntities);
                diyetCount = await _db.SaveChangesAsync();
            }

            // 6) (ops) Antropometri / PuberteFizik / YorumPlan / OzetHesap 
            int? antropometriId = null;
            int? puberteId = null;
            int? yorumId = null;
            int? ozetId = null; 

            if (input.Antropometri is not null
                || input.PuberteFizik is not null
                || input.YorumPlan is not null
                || input.OzetHesap is not null) 
            {
                if (ziyaretId is null)
                    return BadRequest("Antropometri / PuberteFizik / YorumPlan / OzetHesap eklemek için Ziyaret gerekiyor.");

                // Antropometri
                if (input.Antropometri is not null)
                {
                    if (input.Antropometri.BoyCm is < 30 or > 220) return BadRequest("BoyCm 30–220 aralığında olmalıdır.");
                    if (input.Antropometri.KiloKg is < 1 or > 200) return BadRequest("KiloKg 1–200 aralığında olmalıdır.");
                    if (input.Antropometri.BasCevresiCm is < 25 or > 65) return BadRequest("BasCevresiCm 25–65 aralığında olmalıdır.");

                    int? yasAy = null;
                    if (ziyaretTarih.HasValue)
                    {
                        var days = (ziyaretTarih.Value.Date - hasta.BirthDate.Date).TotalDays;
                        yasAy = (int)Math.Round(days / 30.4375, MidpointRounding.AwayFromZero);
                    }

                    var ant = new Antropometri
                    {
                        ZiyaretID = ziyaretId.Value,
                        YasAy = yasAy,
                        BoyCm = input.Antropometri.BoyCm,
                        KiloKg = input.Antropometri.KiloKg,
                        BasCevresiCm = input.Antropometri.BasCevresiCm,
                        OturmaBoyuCm = input.Antropometri.OturmaBoyuCm,
                        ObTb = input.Antropometri.ObTb,
                        GogusCevresiCm = input.Antropometri.GogusCevresiCm,
                        BasPubisCm = input.Antropometri.BasPubisCm,
                        PubisTopukCm = input.Antropometri.PubisTopukCm
                    };
                    _db.Antropometriler.Add(ant);
                    await _db.SaveChangesAsync();
                    await _db.Entry(ant).ReloadAsync();
                    await _sds.RecalcByAntropometriAsync(ant.AntropometriID);
                    antropometriId = ant.AntropometriID;
                }

                // PuberteFizik
                if (input.PuberteFizik is not null)
                {
                    var pf = new PuberteFizik
                    {
                        ZiyaretID = ziyaretId.Value,
                        PuberteNotu = input.PuberteFizik.PuberteNotu,
                        PatolojikFizik = input.PuberteFizik.PatolojikFizik
                    };
                    _db.PuberteFizikler.Add(pf);
                    await _db.SaveChangesAsync();
                    puberteId = pf.PuberteID;
                }

                // YorumPlan
                if (input.YorumPlan is not null)
                {
                    var yp = new YorumPlan
                    {
                        ZiyaretID = ziyaretId.Value,
                        TedaviBeslenmeSpor = input.YorumPlan.TedaviBeslenmeSpor,
                        YorumNotlar = input.YorumPlan.YorumNotlar,
                        OlusturmaTarihi = input.YorumPlan.OlusturmaTarihi ?? DateTime.Now
                    };
                    _db.YorumPlanlar.Add(yp);
                    await _db.SaveChangesAsync();
                    yorumId = yp.YorumID;
                }

                //  (klinik.OzetHesaplar)
                if (input.OzetHesap is not null)
                {
                    if (input.OzetHesap.BoyaUyanTarti is { Length: > 50 })
                        return BadRequest("BoyaUyanTarti en fazla 50 karakter olabilir.");

                    var oh = new OzetHesapKlinik
                    {
                        ZiyaretID = ziyaretId.Value,
                        KulacCm = input.OzetHesap.KulacCm,
                        HedefBoyCm = input.OzetHesap.HedefBoyCm,
                        BoyaUyanTarti = string.IsNullOrWhiteSpace(input.OzetHesap.BoyaUyanTarti) ? null : input.OzetHesap.BoyaUyanTarti.Trim()
                    };
                    _db.OzetHesaplar.Add(oh);
                    await _db.SaveChangesAsync();
                    ozetId = oh.OzetID;
                }
            }

            await trx.CommitAsync();

            return CreatedAtAction(nameof(GetById), new { id = hasta.Id }, new
            {
                hastaId = hasta.Id,
                ziyaretId,
                aileUyesiEklenen = aileCount,
                oykulerId,
                diyetEklenen = diyetCount,
                antropometriId,
                puberteId,
                yorumId,
                ozetId 
            });
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2627 || sql.Number == 2601))
        {
            await trx.RollbackAsync();
            return Conflict("Benzersizlik ihlali. (TC veya aynı gün ziyaret olabilir.)");
        }
        catch
        {
            await trx.RollbackAsync();
            throw;
        }
    }



    // GET /api/Hastalar/{id}/paket
    [HttpGet("{id:int}/paket")]
    public async Task<ActionResult<PatientBundleDto>> GetPaket(int id)
    {
        // 1) Hasta
        var hasta = await _db.Hastalar
            .AsNoTracking()
            .Where(h => h.Id == id)
            .Select(h => new PatientDetailDto
            {
                Id = h.Id,
                Ad = h.Ad,
                Soyad = h.Soyad,
                BirthDate = h.BirthDate,
                Cinsiyet = h.Cinsiyet,
                TcKimlikNo = h.TcKimlikNo,
                Telefon = h.Telefon,
                Email = h.Email,
                Adres = h.Adres
            })
            .FirstOrDefaultAsync();

        if (hasta is null)
            return NotFound($"Hasta bulunamadı: {id}");

        // 2) Aile üyeleri
        var aile = await _db.AileUyeleri
            .AsNoTracking()
            .Where(a => a.HastaID == id)
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

        // 3) Öyküler (en güncel tek kayıt)
        var oykuler = await _db.Oykuler
            .AsNoTracking()
            .Where(o => o.HastaID == id)
            .OrderByDescending(o => o.OlusturmaTarihi)
            .Select(o => new OykulerDetailDto
            {
                OykulerID = o.OykulerID,
                HastaID = o.HastaID,
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
                AileHastaliklar = o.AileHastaliklar,
                OlusturmaTarihi = o.OlusturmaTarihi
            })
            .FirstOrDefaultAsync();

        // 4) Diyetler (son 10)
        var diyetler = await _db.Diyetler
            .AsNoTracking()
            .Where(d => d.HastaID == id)
            .OrderByDescending(d => d.Tarih)
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
            .Take(10)
            .ToListAsync();

        // 5) SON ANTROPOMETRİ 
        var sonAnt = await (
            from a in _db.Antropometriler.AsNoTracking()
            join z in _db.Ziyaretler.AsNoTracking() on a.ZiyaretID equals z.ZiyaretID
            where z.HastaID == id
            orderby z.Tarih descending, a.AntropometriID descending
            select new AntropometriSummaryDto
            {
                AntropometriID = a.AntropometriID,
                ZiyaretID = a.ZiyaretID,
                ZiyaretTarihi = z.Tarih,
                BoyCm = a.BoyCm,
                KiloKg = a.KiloKg,
                BasCevresiCm = a.BasCevresiCm,
                BKI = a.BKI
            }
        ).FirstOrDefaultAsync();

        var paket = new PatientBundleDto
        {
            Hasta = hasta,
            AileUyeleri = aile,
            Oykuler = oykuler,
            Diyetler = diyetler,
        };

        return Ok(paket);
    }
    // PUT /api/Hastalar/{id}/oyku  → upsert (tek kayıt)
    [HttpPut("{id:int}/oyku")]
    public async Task<ActionResult<OykulerDetailDto>> UpsertOyku(int id, [FromBody] OykulerUpdateDto input)
    {
        var hastaVar = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.Id == id);
        if (!hastaVar) return NotFound($"Hasta bulunamadı: {id}");

        var now = DateTime.Now;
        var entity = await _db.Oykuler.FirstOrDefaultAsync(o => o.HastaID == id);
        if (entity is null)
        {
            entity = new Oykuler { HastaID = id, OlusturmaTarihi = input.OlusturmaTarihi ?? now };
            _db.Oykuler.Add(entity);
        }

        static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        if (input.Yakinmalar != null) entity.Yakinmalar = Clean(input.Yakinmalar);
        if (input.Oykusu != null) entity.Oykusu = Clean(input.Oykusu);
        if (input.GebelikOykusu != null) entity.GebelikOykusu = Clean(input.GebelikOykusu);
        if (input.DogumSekli != null) entity.DogumSekli = Clean(input.DogumSekli);
        if (input.DogumAgirlik != null) entity.DogumAgirlik = Clean(input.DogumAgirlik);
        if (input.DogumBoyu != null) entity.DogumBoyu = Clean(input.DogumBoyu);
        if (input.NeonatalDonem != null) entity.NeonatalDonem = Clean(input.NeonatalDonem);
        if (input.NorGelisim != null) entity.NorGelisim = Clean(input.NorGelisim);
        if (input.SutcocukBeslenme != null) entity.SutcocukBeslenme = Clean(input.SutcocukBeslenme);
        if (input.GecirilenHast != null) entity.GecirilenHast = Clean(input.GecirilenHast);
        if (input.OperasyonKaza != null) entity.OperasyonKaza = Clean(input.OperasyonKaza);
        if (input.KanAkrabaligi != null) entity.KanAkrabaligi = Clean(input.KanAkrabaligi);
        if (input.KardesSagligi != null) entity.KardesSagligi = Clean(input.KardesSagligi);
        if (input.AileHastaliklar != null) entity.AileHastaliklar = Clean(input.AileHastaliklar);
        if (input.OlusturmaTarihi.HasValue) entity.OlusturmaTarihi = input.OlusturmaTarihi.Value;

        await _db.SaveChangesAsync();

        var dto = new OykulerDetailDto
        {
            OykulerID = entity.OykulerID,
            HastaID = entity.HastaID,
            OlusturmaTarihi = entity.OlusturmaTarihi,

            Yakinmalar = entity.Yakinmalar,
            Oykusu = entity.Oykusu,
            GebelikOykusu = entity.GebelikOykusu,
            DogumSekli = entity.DogumSekli,

            
            DogumAgirlik = entity.DogumAgirlik,
            DogumBoyu = entity.DogumBoyu,

            NeonatalDonem = entity.NeonatalDonem,
            NorGelisim = entity.NorGelisim,
            SutcocukBeslenme = entity.SutcocukBeslenme,
            GecirilenHast = entity.GecirilenHast,
            OperasyonKaza = entity.OperasyonKaza,

            KanAkrabaligi = entity.KanAkrabaligi,
            KardesSagligi = entity.KardesSagligi,
            AileHastaliklar = entity.AileHastaliklar
        };

        return Ok(dto);
    }

    // DELETE /api/Hastalar/{id}/tam-sil  → hastayı TÜM ilişkileriyle birlikte siler
    [HttpDelete("{id:int}/tam-sil")]
    public async Task<IActionResult> DeleteCascade(int id)
    {
        using var trx = await _db.Database.BeginTransactionAsync();

        var hastaVar = await _db.Hastalar.AsNoTracking().AnyAsync(h => h.Id == id);
        if (!hastaVar) return NotFound($"Hasta bulunamadı: {id}");

        // İlgili ziyaret ID’lerini tek seferde alalım
        var visitIds = await _db.Ziyaretler
            .AsNoTracking()
            .Where(z => z.HastaID == id)
            .Select(z => z.ZiyaretID)
            .ToListAsync();

        if (visitIds.Count > 0)
        {
            
            await _db.Antropometriler
                .Where(a => visitIds.Contains(a.ZiyaretID))
                .ExecuteDeleteAsync();

            await _db.PuberteFizikler
                .Where(p => visitIds.Contains(p.ZiyaretID))
                .ExecuteDeleteAsync();

            await _db.YorumPlanlar
                .Where(y => visitIds.Contains(y.ZiyaretID))
                .ExecuteDeleteAsync();

            
            if (_db.GetService<Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptions>() != null)
            {
                
                try
                {
                    await _db.LabSonuclari
                        .Where(l => visitIds.Contains(l.ZiyaretID))
                        .ExecuteDeleteAsync();
                }
                catch {  }

                
                try
                {
                    await _db.OzetHesaplar
                        .Where(o => visitIds.Contains(o.ZiyaretID))
                        .ExecuteDeleteAsync();
                }
                catch {  }
            }

            // ZİYARETLER
            await _db.Ziyaretler
                .Where(z => z.HastaID == id)
                .ExecuteDeleteAsync();
        }

        // HASTA-BAZLI TABLOLAR
        await _db.Diyetler
            .Where(d => d.HastaID == id)
            .ExecuteDeleteAsync();

        await _db.AileUyeleri
            .Where(a => a.HastaID == id)
            .ExecuteDeleteAsync();

        await _db.Oykuler
            .Where(o => o.HastaID == id)
            .ExecuteDeleteAsync();

        // SON: HASTA
        await _db.Hastalar
            .Where(h => h.Id == id)
            .ExecuteDeleteAsync();

        await trx.CommitAsync();
        return NoContent();
    }



}
