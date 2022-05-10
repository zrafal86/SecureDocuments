using SecureDocuments.Services;

namespace SecureDocuments.WPF.Services
{
    public sealed class ResourceService : IResourceService
    {
        public string GetEmailTemplate()
        {
            return Properties.Resources.EMAIL_TAMPLATE;
        }
    }
}
