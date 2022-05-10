using SecureDocuments.Models.File;

namespace SecureDocuments.Models
{
    public abstract class Lookup<T>
    {
        protected Lookup(T item)
        {
            Item = item;
        }

        public virtual T Item { get; init; }
        public string Display => Item.ToString();
    }

    public sealed class StatusLookup : Lookup<Status>
    {
        public StatusLookup(Status item) : base(item)
        {
        }
    }

    public sealed class OfferTypeLookup : Lookup<OfferType>
    {
        public OfferTypeLookup(OfferType item) : base(item)
        {
        }
    }

    public sealed class CategoryNameLookup : Lookup<CategoryName>
    {
        public CategoryNameLookup(CategoryName item) : base(item)
        {
        }
    }

    public sealed class RoleLookup : Lookup<Role>
    {
        public RoleLookup(Role item) : base(item)
        {
        }
    }

    public sealed class UserLookup
    {
        public UserLookup(User item)
        {
            Item = item;
        }

        public User Item { get; set; }
        public string Display => Item.FullName ?? Item.Email;
    }

    public sealed class CompanyNameLookup
    {
        public CompanyNameLookup(CompanyName item)
        {
            Item = item;
        }

        public CompanyName Item { get; set; }
        public string Display => Item.Name;
    }

    public sealed class SubjectLookup
    {
        public SubjectLookup(Subject item)
        {
            Item = item;
        }

        public Subject Item { get; set; }
        public string Display => Item.Name;
    }

    public sealed class CustomerCountriesLookup
    {
        public CustomerCountriesLookup(CustomerCountry item)
        {
            Item = item;
        }

        public CustomerCountry Item { get; set; }
        public string Display => $"{Item.ShortName} - {Item.ISO2}";
    }

    public sealed class CurrencySymbolLookup : Lookup<CurrencySymbol>
    {
        public CurrencySymbolLookup(CurrencySymbol currencySymbol) : base(currencySymbol)
        {
        }

        public new string Display => $"{Item.ISOCurrencySymbol} ({Item.CurrencyEnglishName})";
    }
}