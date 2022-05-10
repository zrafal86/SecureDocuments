#nullable enable

namespace SecureDocuments.Models
{
    public class Customer
    {
        public string? Name { get; set; }
        public int? CountryId { get; set; }
        public string? Address { get; set; }
        public int? UnitFlagId { get; set; }
        public string? Description { get; set; }
    }
}