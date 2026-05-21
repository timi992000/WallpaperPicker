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
    private readonly TextBlock _statusText;
    private readonly ComboBox _langCombo;
    private string? _selectedHighResUrl;
    private (string Key, string Arg) _status = ("ready", "");

    public MainWindow()
    {
        var sysLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        L.Current = Array.IndexOf(L.Codes, sysLang) >= 0 ? sysLang : "en";

        Width = 1000;
        Height = 700;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brushes.Black;

        _refreshBtn     = new Button { Margin = new Thickness(5), Background = Brushes.DarkSlateBlue, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center };
        _setWallpaperBtn = new Button { Margin = new Thickness(5), Background = Brushes.DarkGreen,    Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, IsEnabled = false };
        _statusText     = new TextBlock { Foreground = Brushes.LightGray, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(15, 0) };

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

        var topBar = new DockPanel { Margin = new Thickness(10), Background = Brushes.Black };
        DockPanel.SetDock(_refreshBtn,      Dock.Left);  topBar.Children.Add(_refreshBtn);
        DockPanel.SetDock(_setWallpaperBtn, Dock.Left);  topBar.Children.Add(_setWallpaperBtn);
        DockPanel.SetDock(_langCombo,       Dock.Right); topBar.Children.Add(_langCombo);
        topBar.Children.Add(_statusText);

        _imageGrid = new WrapPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10) };

        var mainPanel = new DockPanel();
        DockPanel.SetDock(topBar, Dock.Top);
        mainPanel.Children.Add(topBar);
        mainPanel.Children.Add(new ScrollViewer { Content = _imageGrid });
        Content = mainPanel;

        _refreshBtn.Click      += async (_, _) => await LoadRandomImages();
        _setWallpaperBtn.Click += async (_, _) => await SetWallpaper();

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
        _statusText.Text         = L.Status(_status.Key, _status.Arg);
    }

    private async Task LoadRandomImages()
    {
        _refreshBtn.IsEnabled      = false;
        _setWallpaperBtn.IsEnabled = false;
        _selectedHighResUrl        = null;
        _imageGrid.Children.Clear();
        SetStatus("loading");

        await Task.WhenAll(Enumerable.Range(0, 9).Select(async _ =>
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
                    Margin          = new Thickness(5),
                    CornerRadius    = new CornerRadius(5),
                    Child           = new Avalonia.Controls.Image { Source = bitmap, Width = 300, Height = 200, Stretch = Stretch.UniformToFill },
                };

                var btn = new Button
                {
                    Content    = border,
                    Padding    = new Thickness(0),
                    Background = Brushes.Transparent,
                    Cursor     = new Cursor(StandardCursorType.Hand),
                };

                btn.Click += (_, _) =>
                {
                    foreach (var child in _imageGrid.Children)
                        if (child is Button b && b.Content is Border cBorder)
                            cBorder.BorderBrush = Brushes.Transparent;

                    border.BorderBrush      = Brushes.DodgerBlue;
                    _selectedHighResUrl     = highResUrl;
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
