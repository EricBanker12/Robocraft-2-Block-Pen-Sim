using BlockPenSimWPF.Shared.Models;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace BlockPenSimWPF.Data
{
    internal static class ThemeData
    {
        public static Theme GetCurrentTheme()
        {
            var settings = new UISettings();
            var foreground = settings.GetColorValue(UIColorType.Foreground);
            if (foreground.Equals(new Color() { R = 255, G = 255, B = 255, A = 255 }))
                return Theme.Dark;
            else
                return Theme.Light;
        }
    }
}
