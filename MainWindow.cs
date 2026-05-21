using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

class MainWindow : Window
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private readonly WrapPanel _imageGrid;
    private readonly Button _setWallpaperBtn;
    private readonly Button _refreshBtn;
    private readonly Button _favBtn;
    private readonly TextBlock _statusText;
    private readonly ComboBox _langCombo;
    private readonly ComboBox _countCombo;
    private string? _selectedHighResUrl;
    private (string Key, string Arg) _status = ("ready", "");

    public MainWindow()
    {
        FavoritesManager.Load();
        SettingsManager.Load();

        var sysLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        L.Current = Array.IndexOf(L.Codes, sysLang) >= 0 ? sysLang : "en";

        Width = 1000;
        Height = 700;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brushes.Black;

        _refreshBtn      = new Button { Margin = new Thickness(5), Background = Brushes.DarkSlateBlue, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center };
        _setWallpaperBtn = new Button { Margin = new Thickness(5), Background = Brushes.DarkGreen,     Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, IsEnabled = false };
        _favBtn          = new Button { Margin = new Thickness(5), Background = new SolidColorBrush(Color.FromRgb(90, 70, 10)), Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center };
        _statusText      = new TextBlock { Foreground = Brushes.LightGray, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(15, 0) };

        _langCombo = new ComboBox
        {
            ItemsSource = L.Codes.Select(c => { var (f, n) = L.Languages[c]; return $"{f} {n}"; }).ToList(),
            SelectedIndex = Array.IndexOf(L.Codes, L.Current),
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center,
        };
        _langCombo.SelectionChanged += (_, _) =>
        {
            if (_langCombo.SelectedIndex >= 0)
            {
                L.Current = L.Codes[_langCombo.SelectedIndex];
                ApplyLanguage();
            }
        };

        var countOptions = new[] { 3, 6, 9, 12, 15, 18 };
        _countCombo = new ComboBox
        {
            ItemsSource = countOptions,
            SelectedItem = countOptions.Contains(SettingsManager.ImageCount) ? SettingsManager.ImageCount : 9,
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center,
            Width = 60,
        };
        _countCombo.SelectionChanged += async (_, _) =>
        {
            if (_countCombo.SelectedItem is int count)
            {
                SettingsManager.SetImageCount(count);
                await LoadRandomImages();
            }
        };

        var topBar = new DockPanel { Margin = new Thickness(10), Background = Brushes.Black };
        DockPanel.SetDock(_refreshBtn,      Dock.Left);  topBar.Children.Add(_refreshBtn);
        DockPanel.SetDock(_setWallpaperBtn, Dock.Left);  topBar.Children.Add(_setWallpaperBtn);
        DockPanel.SetDock(_favBtn,          Dock.Left);  topBar.Children.Add(_favBtn);
        DockPanel.SetDock(_langCombo,  Dock.Right); topBar.Children.Add(_langCombo);
        DockPanel.SetDock(_countCombo, Dock.Right); topBar.Children.Add(_countCombo);
        topBar.Children.Add(_statusText);

        _imageGrid = new WrapPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10) };

        var mainPanel = new DockPanel();
        DockPanel.SetDock(topBar, Dock.Top);
        mainPanel.Children.Add(topBar);
        mainPanel.Children.Add(new ScrollViewer { Content = _imageGrid });
        Content = mainPanel;

        _refreshBtn.Click      += async (_, _) => await LoadRandomImages();
        _setWallpaperBtn.Click += async (_, _) => await SetWallpaper();
        _favBtn.Click += (_, _) =>
        {
            var fw = new FavoritesWindow();
            fw.Show(this);
        };

        ApplyLanguage();
        _ = LoadRandomImages();
    }

    private void SetStatus(string key, string arg = "")
    {
        _status = (key, arg);
        _statusText.Text = L.Status(key, arg);
    }

    private void ApplyLanguage()
    {
        Title = L.Get("title");
        _refreshBtn.Content      = L.Get("loadBtn");
        _setWallpaperBtn.Content = L.Get("setBtn");
        _favBtn.Content          = L.Get("favBtn");
        _statusText.Text         = L.Status(_status.Key, _status.Arg);
    }

    private async Task LoadRandomImages()
    {
        _refreshBtn.IsEnabled      = false;
        _setWallpaperBtn.IsEnabled = false;
        _selectedHighResUrl        = null;
        _imageGrid.Children.Clear();
        var count = SettingsManager.ImageCount;
        SetStatus("loading", count.ToString());

        await Task.WhenAll(Enumerable.Range(0, count).Select(async _ =>
        {
            var seed       = Guid.NewGuid().ToString();
            var previewUrl = $"https://picsum.photos/seed/{seed}/300/200";
            var highResUrl = $"https://picsum.photos/seed/{seed}/2880/1800";

            try
            {
                var bytes = await _http.GetByteArrayAsync(previewUrl);
                using var ms = new MemoryStream(bytes);
                var bitmap = new Bitmap(ms);

                var border = new Border
                {
                    BorderBrush     = Brushes.Transparent,
                    BorderThickness = new Thickness(4),
                    CornerRadius    = new CornerRadius(5),
                    Child           = new Avalonia.Controls.Image { Source = bitmap, Width = 300, Height = 200, Stretch = Stretch.UniformToFill },
                };

                var isFav = FavoritesManager.IsFavorite(seed);
                var starBtn = new Button
                {
                    Content             = isFav ? "★" : "☆",
                    Foreground          = isFav ? Brushes.Gold : Brushes.White,
                    Background          = new SolidColorBrush(Color.FromArgb(140, 0, 0, 0)),
                    BorderThickness     = new Thickness(0),
                    Padding             = new Thickness(4, 2),
                    FontSize            = 18,
                    Cursor              = new Cursor(StandardCursorType.Hand),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment   = VerticalAlignment.Top,
                    Margin              = new Thickness(0, 4, 4, 0),
                    ZIndex              = 1,
                };

                starBtn.Click += (_, e) =>
                {
                    e.Handled = true;
                    var nowFav = FavoritesManager.Toggle(seed);
                    starBtn.Content    = nowFav ? "★" : "☆";
                    starBtn.Foreground = nowFav ? Brushes.Gold : Brushes.White;
                };

                var overlay = new Grid { Width = 300, Height = 200, Margin = new Thickness(5) };
                overlay.Children.Add(border);
                overlay.Children.Add(starBtn);

                var btn = new Button
                {
                    Content    = overlay,
                    Padding    = new Thickness(0),
                    Background = Brushes.Transparent,
                    Cursor     = new Cursor(StandardCursorType.Hand),
                };

                btn.Click += (_, _) =>
                {
                    foreach (var child in _imageGrid.Children)
                        if (child is Button b && b.Content is Grid g && g.Children[0] is Border cBorder)
                            cBorder.BorderBrush = Brushes.Transparent;

                    border.BorderBrush         = Brushes.DodgerBlue;
                    _selectedHighResUrl        = highResUrl;
                    _setWallpaperBtn.IsEnabled = true;
                    SetStatus("selected");
                };

                _imageGrid.Children.Add(btn);
            }
            catch (Exception ex)
            {
                SetStatus("errorLoad", ex.Message);
            }
        }));

        _refreshBtn.IsEnabled = true;
        SetStatus("loaded");
    }

    private async Task SetWallpaper()
    {
        if (string.IsNullOrEmpty(_selectedHighResUrl)) return;

        _setWallpaperBtn.IsEnabled = false;
        _refreshBtn.IsEnabled      = false;
        SetStatus("downloading");

        try
        {
            var bytes    = await _http.GetByteArrayAsync(_selectedHighResUrl);
            var filePath = Path.Combine(Path.GetTempPath(), $"wallpaper_{DateTimeOffset.Now.ToUnixTimeSeconds()}.jpg");
            await File.WriteAllBytesAsync(filePath, bytes);

            SetStatus("setting");
            await Task.Run(() => WallpaperSetter.Set(filePath));
            SetStatus("done");
        }
        catch (Exception ex)
        {
            SetStatus("errorSet", ex.Message);
        }
        finally
        {
            _setWallpaperBtn.IsEnabled = true;
            _refreshBtn.IsEnabled      = true;
        }
    }
}
