using System.Text.Json;

static class SettingsManager
{
    private static readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WallpaperPicker", "settings.json");

    public static int ImageCount { get; private set; } = 9;

    public static void Load()
    {
        try
        {
            if (!File.Exists(_path)) return;
            var data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_path));
            if (data != null) ImageCount = data.ImageCount;
        }
        catch { }
    }

    public static void SetImageCount(int count)
    {
        ImageCount = count;
        Save();
    }

    private static void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(new SettingsData { ImageCount = ImageCount }));
    }

    private class SettingsData
    {
        public int ImageCount { get; set; } = 9;
    }
}
