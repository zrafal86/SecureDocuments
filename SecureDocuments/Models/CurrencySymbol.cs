namespace SecureDocuments.Models
{
    public record CurrencySymbol
    {
        public string? CurrencyEnglishName { get; init; }
        public string? ISOCurrencySymbol { get; init; }

        public override string? ToString()
        {
            return $"{ISOCurrencySymbol} (CurrencyEnglishName)";
        }
    }
}