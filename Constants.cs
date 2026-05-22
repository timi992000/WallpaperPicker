using System.IO;
using Avalonia;

namespace WallpaperPicker;

internal static class Constants
{
    internal static readonly string ApplicationConfigDirectory = Path.Combine
    (
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WallpaperPicker"
    );

    internal static readonly string FavoritesFilePath = Path.Combine
    (
        ApplicationConfigDirectory,
        "favorites.json"
    );

    internal static readonly string SettingsFilePath = Path.Combine
    (
        ApplicationConfigDirectory,
        "settings.json"
    );
}