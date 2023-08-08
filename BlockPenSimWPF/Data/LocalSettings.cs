using Newtonsoft.Json;
using System.IO.IsolatedStorage;
using System.Windows;
using Windows.Storage;
using Windows.System;

namespace BlockPenSimWPF.Data
{
    internal static class LocalSettings
    {
        public static void Save()
        {
            Properties.Settings.Default.Save();
        }

        public static void SetValue(string key, object? value)
        {
            var json = JsonConvert.SerializeObject(value);
            Properties.Settings.Default[key] = json;
        }

        public static void SetValue(string key, string value)
        {
            Properties.Settings.Default[key] = value;
        }

        public static string? GetValue(string key)
        {
            try
            {
                return (string?)Properties.Settings.Default[key];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T? GetValue<T>(string key)
        {
            try
            {
                var json = (string?)Properties.Settings.Default[key];
                if (string.IsNullOrEmpty(json)) return default;
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
