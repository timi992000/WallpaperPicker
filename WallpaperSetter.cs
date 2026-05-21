using System.Diagnostics;
using System.Runtime.InteropServices;

static class WallpaperSetter
{
    public static void Set(string filePath)
    {
        if (OperatingSystem.IsMacOS())
            SetMacOS(filePath);
        else if (OperatingSystem.IsWindows())
            SetWindows(filePath);
        else
            SetLinux(filePath);
    }

    private static void SetMacOS(string filePath)
    {
        var script = $@"tell application ""System Events""
            set allDesktops to a reference to every desktop
            repeat with d in allDesktops
                set picture of d to ""{filePath}""
            end repeat
        end tell";
        Exec("osascript", "-e", script);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private static void SetWindows(string filePath) =>
        SystemParametersInfo(0x0014, 0, filePath, 0x01 | 0x02);

    private static void SetLinux(string filePath)
    {
        var desktop = Env("XDG_CURRENT_DESKTOP").ToLowerInvariant();
        var session = Env("DESKTOP_SESSION").ToLowerInvariant();
        var fileUri = $"file://{filePath}";

        if (Contains(desktop, session, "gnome", "unity", "budgie", "ubuntu"))
        {
            Exec("gsettings", "set", "org.gnome.desktop.background", "picture-uri", fileUri);
            Exec("gsettings", "set", "org.gnome.desktop.background", "picture-uri-dark", fileUri);
        }
        else if (Contains(desktop, session, "kde", "plasma"))
            LinuxKde(filePath);
        else if (Contains(desktop, session, "xfce"))
            LinuxXfce(filePath);
        else if (Contains(desktop, session, "mate"))
            Exec("gsettings", "set", "org.mate.background", "picture-filename", filePath);
        else if (Contains(desktop, session, "cinnamon", "x-cinnamon"))
            Exec("gsettings", "set", "org.cinnamon.desktop.background", "picture-uri", fileUri);
        else if (Contains(desktop, session, "lxqt"))
            Exec("pcmanfm-qt", "--set-wallpaper", filePath);
        else if (Contains(desktop, session, "lxde"))
            Exec("pcmanfm", "--set-wallpaper", filePath);
        else if (Contains(desktop, session, "deepin", "dde"))
            Exec("gsettings", "set", "com.deepin.wrap.gnome.desktop.background", "picture-uri", fileUri);
        else if (Contains(desktop, session, "enlightenment"))
            Exec("enlightenment_remote", "-desktop-bg-set", "0", "0", "0", "0", filePath);
        else if (Contains(desktop, session, "sway") || !string.IsNullOrEmpty(Env("SWAYSOCK")))
            Exec("swaymsg", "output", "*", "bg", filePath, "fill");
        else if (Contains(desktop, session, "hyprland") || !string.IsNullOrEmpty(Env("HYPRLAND_INSTANCE_SIGNATURE")))
        {
            if (!Exec("swww", "img", filePath).Ok)
                Exec("hyprctl", "hyprpaper", "wallpaper", $",{filePath}");
        }
        else if (Contains(desktop, session, "i3", "openbox", "fluxbox", "icewm", "jwm"))
        {
            if (!Exec("feh", "--bg-fill", filePath).Ok)
                Exec("nitrogen", "--set-zoom-fill", filePath);
        }
        else
        {
            if (Exec("feh", "--bg-fill", filePath).Ok) return;
            if (Exec("nitrogen", "--set-zoom-fill", filePath).Ok) return;
            if (Exec("gsettings", "set", "org.gnome.desktop.background", "picture-uri", fileUri).Ok) return;
            Exec("plasma-apply-wallpaperimage", filePath);
        }
    }

    private static void LinuxKde(string filePath)
    {
        if (!Exec("plasma-apply-wallpaperimage", filePath).Ok)
        {
            var script = $@"var all = desktops(); for (var i = 0; i < all.length; i++) {{ var d = all[i]; d.wallpaperPlugin = 'org.kde.image'; d.currentConfigGroup = ['Wallpaper', 'org.kde.image', 'General']; d.writeConfig('Image', 'file://{filePath}'); }}";
            Exec("qdbus", "org.kde.plasmashell", "/PlasmaShell", "org.kde.PlasmaShell.evaluateScript", script);
        }
    }

    private static void LinuxXfce(string filePath)
    {
        var (ok, output) = Exec("xfconf-query", "-c", "xfce4-desktop", "-l");
        if (ok && output != null)
        {
            bool any = false;
            foreach (var line in output.Split('\n'))
            {
                var prop = line.Trim();
                if (prop.EndsWith("/last-image", StringComparison.Ordinal))
                {
                    Exec("xfconf-query", "-c", "xfce4-desktop", "-p", prop, "-s", filePath);
                    any = true;
                }
            }
            if (any) return;
        }
        Exec("xfconf-query", "-c", "xfce4-desktop", "-p", "/backdrop/screen0/monitor0/workspace0/last-image", "-s", filePath);
    }

    private static bool Contains(string desktop, string session, params string[] keywords)
    {
        foreach (var kw in keywords)
            if (desktop.Contains(kw) || session.Contains(kw))
                return true;
        return false;
    }

    private static string Env(string key) => Environment.GetEnvironmentVariable(key) ?? string.Empty;

    private static (bool Ok, string? Output) Exec(string cmd, params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (var a in args) psi.ArgumentList.Add(a);
            var p = Process.Start(psi);
            var output = p?.StandardOutput.ReadToEnd();
            p?.WaitForExit();
            return (p?.ExitCode == 0, output);
        }
        catch { return (false, null); }
    }
}
