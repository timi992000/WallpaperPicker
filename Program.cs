using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

AppBuilder.Configure<WallpaperApp>()
    .UsePlatformDetect()
    .StartWithClassicDesktopLifetime(args);

class WallpaperApp : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
