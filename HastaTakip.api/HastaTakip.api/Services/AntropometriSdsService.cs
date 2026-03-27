using System;
using System.Linq;
using System.Threading.Tasks;
using HastaTakip.Api.Data;
using HastaTakip.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Services
{
    public interface IAntropometriSdsService
    {
        Task RecalcByAntropometriAsync(int antropometriId);
        Task RecalcByZiyaretAsync(int ziyaretId);
        Task RecalcByHastaAsync(int hastaId);
    }

    public class AntropometriSdsService : IAntropometriSdsService
    {
        private readonly HastaDbContext _db;
        private readonly IGrowthLmsService _growth;

        public AntropometriSdsService(HastaDbContext db, IGrowthLmsService growth)
        {
            _db = db;
            _growth = growth;
        }

     
        public async Task RecalcByAntropometriAsync(int antropometriId)
        {
            var ent = await _db.Antropometriler
                .Include(a => a.Ziyaret)
                .ThenInclude(z => z.Hasta)
                .FirstOrDefaultAsync(a => a.AntropometriID == antropometriId);

            if (ent is null) return;

            await RecalcOneAsync(ent);
            await _db.SaveChangesAsync();
        }

    
        public async Task RecalcByZiyaretAsync(int ziyaretId)
        {
            var list = await _db.Antropometriler
                .Include(a => a.Ziyaret)
                .ThenInclude(z => z.Hasta)
                .Where(a => a.ZiyaretID == ziyaretId)
                .ToListAsync();

            foreach (var ent in list)
                await RecalcOneAsync(ent);

            await _db.SaveChangesAsync();
        }

      
        public async Task RecalcByHastaAsync(int hastaId)
        {
            var list = await _db.Antropometriler
                .Include(a => a.Ziyaret)
                .ThenInclude(z => z.Hasta)
                .Where(a => a.Ziyaret.HastaID == hastaId)
                .ToListAsync();

            foreach (var ent in list)
                await RecalcOneAsync(ent);

            await _db.SaveChangesAsync();
        }

       
        private async Task RecalcOneAsync(Antropometri ent)
        {
            var hasta = ent.Ziyaret.Hasta;
            var sex = hasta.Cinsiyet; 

            // Yaş (ay)
            ent.YasAy ??= CalcAgeMonths(hasta.BirthDate, ent.Ziyaret.Tarih);
            var yasAy = (decimal)ent.YasAy.Value;

            // SDS hesapları
            ent.BoySDS = await _growth.ComputeZAsync("BoyCm", sex, yasAy, ent.BoyCm);
            ent.KiloSDS = await _growth.ComputeZAsync("KiloKg", sex, yasAy, ent.KiloKg);
            ent.BasCevresiSDS = await _growth.ComputeZAsync("BasCevresiCm", sex, yasAy, ent.BasCevresiCm);

            
            await _db.SaveChangesAsync();
            await _db.Entry(ent).ReloadAsync();

            ent.BKISDS = await _growth.ComputeZAsync("BKI", sex, yasAy, ent.BKI);
        }

        private static int CalcAgeMonths(DateTime birth, DateTime visit)
        {
            var months = (visit.Year - birth.Year) * 12 + (visit.Month - birth.Month);
            if (visit.Day < birth.Day) months -= 1;
            return Math.Max(0, months);
        }
    }
}
