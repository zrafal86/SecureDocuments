using System.Reflection;
using SecureDocuments.Services;

namespace SecureDocuments.Avalonia.Services;

public sealed class ResourceService : IResourceService
{
    public string GetEmailTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "SecureDocuments.Avalonia.Assets.email_template.html";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return string.Empty;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
