using SecureDocuments.Models;

namespace SecureDocuments.Helpers
{
    public static class CurrencyPerCountry
    {
        public static List<CurrencySymbolLookup> GetCurrencyList()
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c => { try { return new RegionInfo(c.Name); } catch { return null; } })
            .Where(r => r is not null)
            .Cast<RegionInfo>()
            .Distinct()
            .Select(r => new CurrencySymbolLookup(new CurrencySymbol
            {
                CurrencyEnglishName = r.CurrencyEnglishName,
                ISOCurrencySymbol = r.ISOCurrencySymbol
            }))
            .GroupBy(x => x.Item.ISOCurrencySymbol)
            .Select(x => x.First())
            .Where(c => !string.IsNullOrEmpty(c.Item.CurrencyEnglishName))
            .OrderBy(c => c.Item.ISOCurrencySymbol)
            .ToList();
        }
    }
}
