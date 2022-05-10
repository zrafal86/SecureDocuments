using SecureDocuments.Services;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace SecureDocuments.WPF.Services
{
    public sealed class ThemeService : IThemeService
    {
        public void SetTheme(bool isDark)
        {
            var paletteHelper = new PaletteHelper();

            var primaryColor = SwatchHelper.Lookup[MaterialDesignColor.Blue];
            var accentColor = SwatchHelper.Lookup[MaterialDesignColor.LightBlue];
            var themeLight = Theme.Create(new MaterialDesignLightTheme(), primaryColor, accentColor);
            var themeDark = Theme.Create(new MaterialDesignDarkTheme(), primaryColor, accentColor);
            if (isDark)
                paletteHelper.SetTheme(themeDark);
            else
                paletteHelper.SetTheme(themeLight);
        }
    }
}