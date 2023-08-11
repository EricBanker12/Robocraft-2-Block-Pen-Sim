using BlockPenSimWPF.Shared.Models;
using Microsoft.JSInterop;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace BlockPenSimWPF.Data
{
    internal static class ThemeData
    {
        private static readonly string storageKey = "ThemeOverride";

        private static Theme? themeOverride;
        public static Theme ThemeOverride
        {
            get
            {
                if (themeOverride == null)
                    return LocalSettings.GetValue<Theme>(storageKey);
                else
                    return (Theme)themeOverride;
            }
            set
            {
                themeOverride = value;
                LocalSettings.SetValue(storageKey, value);
            }
        }

        public static async ValueTask OverridePreferredTheme(IJSRuntime js)
        {
            if (js != null)
                await js.InvokeVoidAsync("OverridePreferredTheme", GetCurrentTheme().ToString().ToLower() );
        }

        public static Theme GetCurrentTheme()
        {
            if (ThemeOverride == Theme.Default)
            {
                var settings = new UISettings();
                var foreground = settings.GetColorValue(UIColorType.Foreground);
                if (foreground.Equals(new Color() { R = 255, G = 255, B = 255, A = 255 }))
                    return Theme.Dark;
                else
                    return Theme.Light;
            }
            else { return ThemeOverride; }
        }
    }
}
