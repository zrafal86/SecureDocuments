using SecureDocuments.Models;
using SecureDocuments.Models.File;

namespace SecureDocuments.Factories
{
    public interface ICategoryNamesSource
    {
        IList<CategoryNameLookup> GetCategories(FolderPurpose folderPurpose);
    }

    public class FileCategoryNamesSource : ICategoryNamesSource
    {
        public IList<CategoryName> Categories { get; init; }

        public FileCategoryNamesSource()
        {
            Categories = ((IList<CategoryName>)Enum.GetValues(typeof(CategoryName))).ToList();
        }

        public IList<CategoryNameLookup> GetCategories(FolderPurpose folderPurpose)
        {
            return folderPurpose switch
            {
                FolderPurpose.Normal => GetNormalCategories(),
                FolderPurpose.Invoices => GetInvoiceCategories(),
                _ => throw new NotImplementedException(),
            };
        }

        private static IList<CategoryNameLookup> GetInvoiceCategories()
        {
            return ((IList<CategoryName>)Enum.GetValues(typeof(CategoryName))).ToList()
                    .Where(category => (int)category is > (int)CategoryName.Unknown and <= (int)CategoryName.OrderSettlement)
                    .Select(category => new CategoryNameLookup(category)).ToList();
        }

        private static IList<CategoryNameLookup> GetNormalCategories()
        {
            return ((IList<CategoryName>)Enum.GetValues(typeof(CategoryName)))
                .Where(category => (int)category is >= 1 and <= 9)
                .Select(category => new CategoryNameLookup(category)).ToList();
        }
    }
}
