namespace SecureDocuments.Models
{
    public record Offer
    {
        public string Id { get; init; }
        public string? OfferNumber { get; set; }
        public string? Name { get; set; }
        public string? CompanyName { get; set; }
        public string? Subject { get; set; }
        public OfferType Type { get; set; }
        public Status Status { get; set; }
        public User? Manager { get; set; }
        public User? Applicant { get; set; }
        public User? Builder { get; set; }
        public User? Technologist { get; set; }
        public Customer? Customer { get; set; }
        public string? Description { get; set; }
        public string? InitialGrossAmount { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset RealEndDate { get; set; }
        public DateTimeOffset EstimatedEndDate { get; set; }
        public DateTimeOffset AcceptedDate { get; set; }
        public CurrencySymbol? CurrencySymbol { get; init; }
    }
}