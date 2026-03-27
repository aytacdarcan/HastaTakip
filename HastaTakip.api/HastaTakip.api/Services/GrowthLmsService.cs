// Services/GrowthLmsService.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using HastaTakip.Api.Data;
using HastaTakip.Api.Models;
using Microsoft.EntityFrameworkCore;

public interface IGrowthLmsService
{
    Task<decimal?> ComputeZAsync(string measure, string sex, decimal ageMonths, decimal? value);
    Task<decimal?> InverseAsync(string measure, string sex, decimal ageMonths, decimal z);
}

public class GrowthLmsService : IGrowthLmsService
{
    private readonly HastaDbContext _db;
    public GrowthLmsService(HastaDbContext db) => _db = db;

    
    private readonly record struct Lms(double L, double M, double S);

    public async Task<decimal?> ComputeZAsync(string measure, string sex, decimal ageMonths, decimal? value)
    {
        if (value is null) return null;

        
        ageMonths = ClampByMeasure(measure, ageMonths);

        var lms = await GetLmsInterpolatedAsync(measure, sex, ageMonths);
        if (lms is null) return null;

        var (L, M, S) = lms.Value;
        if (M <= 0 || S <= 0) return null;

        var y = (double)value.Value;

        if (L == 0)
        {
            return (decimal)(Math.Log(y / M) / S);
        }
        else
        {
            var pow = Math.Pow(y / M, L);
            return (decimal)((pow - 1) / (L * S));
        }
    }

    public async Task<decimal?> InverseAsync(string measure, string sex, decimal ageMonths, decimal z)
    {
        // Ölçüme göre yaş sınırı
        ageMonths = ClampByMeasure(measure, ageMonths);

        var lms = await GetLmsInterpolatedAsync(measure, sex, ageMonths);
        if (lms is null) return null;

        var (L, M, S) = lms.Value;
        if (M <= 0 || S <= 0) return null;

        double result;
        if (L == 0)
        {
            result = M * Math.Exp(S * (double)z);
        }
        else
        {
            var inner = 1 + L * S * (double)z;
            if (inner <= 0) return null; 
            result = M * Math.Pow(inner, 1 / L);
        }

        return (decimal)result;
    }

    private async Task<Lms?> GetLmsInterpolatedAsync(string measure, string sex, decimal ageMonths)
    {
        
        var range = await _db.GrowthLMS
            .AsNoTracking()
            .Where(r => r.Kaynak == "NEYZI" && r.Olcum == measure && r.Cinsiyet == sex)
            .GroupBy(_ => 1)
            .Select(g => new { MinAy = g.Min(x => x.YasAy), MaxAy = g.Max(x => x.YasAy) })
            .FirstOrDefaultAsync();

        if (range is null) return null;

        var age = (double)ageMonths;
        if (ageMonths < range.MinAy) age = range.MinAy;
        if (ageMonths > range.MaxAy) age = range.MaxAy;

       
        int a0 = (int)Math.Floor(age);
        int a1 = (int)Math.Ceiling(age);

        var lower = await _db.GrowthLMS.AsNoTracking()
            .Where(r => r.Kaynak == "NEYZI" && r.Olcum == measure && r.Cinsiyet == sex && r.YasAy <= a0)
            .OrderByDescending(r => r.YasAy)
            .FirstOrDefaultAsync();

        var upper = await _db.GrowthLMS.AsNoTracking()
            .Where(r => r.Kaynak == "NEYZI" && r.Olcum == measure && r.Cinsiyet == sex && r.YasAy >= a1)
            .OrderBy(r => r.YasAy)
            .FirstOrDefaultAsync();

        if (lower is null && upper is null) return null;
        if (lower is null) return new Lms(upper!.L, upper.M, upper.S);
        if (upper is null) return new Lms(lower!.L, lower.M, lower.S);
        if (upper.YasAy == lower.YasAy) return new Lms(lower.L, lower.M, lower.S);

        
        var x0 = (double)lower.YasAy;
        var x1 = (double)upper.YasAy;
        var t = (age - x0) / (x1 - x0);

        double L = lower.L + (upper.L - lower.L) * t;
        double M = lower.M + (upper.M - lower.M) * t;
        double S = lower.S + (upper.S - lower.S) * t;

        return new Lms(L, M, S);
    }

    private static decimal ClampByMeasure(string measure, decimal ageMonths)
    {
        
        if (string.Equals(measure, "BasCevresiCm", StringComparison.OrdinalIgnoreCase))
            return Math.Min(36, Math.Max(0, ageMonths));
        return Math.Max(0, ageMonths);
    }
}
