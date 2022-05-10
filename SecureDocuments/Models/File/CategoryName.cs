namespace SecureDocuments.Models.File
{

    public enum CategoryName
    {
        All = 0,
        Inquiry = 1, //zapytanie ofertowe; manager
        Correspondence = 2, // korespondencja; manager
        Specification = 3, // ;tech,builders
        Documentation = 4, // ;tech,builders
        Protocol = 5, // ;tech,builders
        Schedule = 6, // ;tech,builders
        Contract = 7,
        Offer = 8,
        Unknown = 9,
        // *********************
        Invoice = 10, // faktury
        SubcontractorOrder = 11, // zlecenia podwykonawców
        SubcontractorInvoice = 12, // faktury podwykonawców
        OrderSettlement = 13, // rozliczenie zlecenia
    }
}