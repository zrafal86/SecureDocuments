namespace SecureDocuments.Models.File
{
    public record UploadFileDetails(string DestFolder, Role Role, CategoryName Category, string[] Tags);
}