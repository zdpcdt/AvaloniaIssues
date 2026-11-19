using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;

namespace FullScreenFromMaximizedShrink;

// ForceDrawnDecorations is an experimental API in Avalonia 12.1.
[Experimental("AVALONIA_X11_FORCE_CSD")]
internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // The key of this repro: the window becomes client-side decorated,
            // i.e. undecorated for the WM (Mutter sees _MOTIF_WM_HINTS decorations = 0).
            .With(new X11PlatformOptions { ForceDrawnDecorations = true })
            .LogToTrace();
}
