using HastaTakip.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HastaTakip.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrowthController : ControllerBase
    {
        private readonly HastaDbContext _db;
        private readonly IGrowthLmsService _growth;

        public GrowthController(HastaDbContext db, IGrowthLmsService growth)
        {
            _db = db;
            _growth = growth;
        }

       
        // HASTA SERİSİ (ZİYARET NOKTALARI)
        // GET /api/Growth/series?hastaId=123&measure=BoyCm
        [HttpGet("series")]
        public async Task<IActionResult> Series(
     [FromQuery] int hastaId,
     [FromQuery] string measure = "BoyCm")
        {
            var hasta = await _db.Hastalar
                .AsNoTracking()
                .Where(h => h.Id == hastaId)
                .Select(h => new { h.Cinsiyet, h.BirthDate })
                .FirstOrDefaultAsync();

            if (hasta is null)
                return NotFound("Hasta bulunamadı.");

            var list = await _db.Antropometriler
                .AsNoTracking()
                .Include(a => a.Ziyaret)
                .Where(a => a.Ziyaret.HastaID == hastaId)
                .OrderBy(a => a.Ziyaret.Tarih)
                .ToListAsync();

            var points = new List<object>();

            foreach (var a in list)
            {
                decimal? value = measure switch
                {
                    "BoyCm" => a.BoyCm,
                    "KiloKg" => a.KiloKg,
                    "BasCevresiCm" => a.BasCevresiCm,
                    "BKI" => a.BKI,
                    _ => null
                };

                decimal? z = measure switch
                {
                    "BoyCm" => a.BoySDS,
                    "KiloKg" => a.KiloSDS,
                    "BasCevresiCm" => a.BasCevresiSDS,
                    "BKI" => a.BKISDS,
                    _ => null
                };

                if (value is null) continue;

                
                var ageMonths =
                    (a.Ziyaret.Tarih.Year - hasta.BirthDate.Year) * 12
                    + (a.Ziyaret.Tarih.Month - hasta.BirthDate.Month);

                points.Add(new
                {
                    ageMonths,
                    value,
                    z
                });
            }

            return Ok(new
            {
                measure,
                points
            });
        }


        
        // REFERANS EĞRİLERİ (P3–P97)
        
        // GET /api/Growth/refs?measure=BoyCm&sex=E
        [HttpGet("refs")]
        public async Task<IActionResult> Refs(
            [FromQuery] string measure,
            [FromQuery] char sex,
            [FromQuery] int maxMonth = 216,
            [FromQuery] int step = 1)
        {
            var range = await _db.GrowthLMS
                .AsNoTracking()
                .Where(r => r.Kaynak == "NEYZI"
                         && r.Olcum == measure
                         && r.Cinsiyet == sex.ToString())
                .GroupBy(_ => 1)
                .Select(g => new {
                    MinAy = g.Min(x => x.YasAy),
                    MaxAy = g.Max(x => x.YasAy)
                })
                .FirstOrDefaultAsync();

            if (range is null)
                return Ok(Array.Empty<object>());

            maxMonth = Math.Min(maxMonth, range.MaxAy);

            var zLevels = new[] { -1.881m, -1.282m, -0.674m, 0m, 0.674m, 1.282m, 1.881m };
            var grid = new List<Dictionary<string, object?>>();

            for (int m = range.MinAy; m <= maxMonth; m += step)
            {
                var row = new Dictionary<string, object?> { ["ageMonths"] = m };

                foreach (var z in zLevels)
                {
                    var key = "z" + z.ToString("+0.###;-0.###;0", CultureInfo.InvariantCulture);
                    row[key] = await _growth.InverseAsync(measure, sex.ToString(), m, z);
                }

                grid.Add(row);
            }

            return Ok(grid);
        }

    }
}
