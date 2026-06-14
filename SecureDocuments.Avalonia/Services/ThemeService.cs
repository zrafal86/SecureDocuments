using Avalonia;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using SecureDocuments.Services;

namespace SecureDocuments.Avalonia.Services;

public sealed class ThemeService : IThemeService
{
    public void SetTheme(bool isDark)
    {
        var theme = global::Avalonia.Application.Current?.Styles
            .OfType<MaterialTheme>()
            .FirstOrDefault();

        if (theme != null)
        {
            theme.BaseTheme = isDark ? BaseThemeMode.Dark : BaseThemeMode.Light;
        }
    }
}
