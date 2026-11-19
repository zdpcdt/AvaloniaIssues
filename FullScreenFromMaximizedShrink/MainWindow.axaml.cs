using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FullScreenFromMaximizedShrink;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        UpdateStateText(WindowState);

        // `--auto`: run the repro sequence shortly after startup, no clicks needed.
        if (Environment.GetCommandLineArgs().Contains("--auto"))
            Opened += async (_, _) => await RunReproSequence();
    }

    private async System.Threading.Tasks.Task RunReproSequence()
    {
        await System.Threading.Tasks.Task.Delay(600);
        WindowState = WindowState.Normal;
        Width = 1440;
        Height = 733;
        await System.Threading.Tasks.Task.Delay(400);
        WindowState = WindowState.Maximized;
        await System.Threading.Tasks.Task.Delay(400);
        WindowState = WindowState.FullScreen;

        // Give the WM time to settle, then dump the live state.
        await System.Threading.Tasks.Task.Delay(800);
        Console.WriteLine(
            $"[repro] WindowState={WindowState}  ClientSize={ClientSize.Width:F0}x{ClientSize.Height:F0}  Width={Width:F0} Height={Height:F0}");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty || change.Property == ClientSizeProperty)
            UpdateStateText(WindowState);
    }

    // Mimics what WindowDrawnDecorations' fullscreen caption button does:
    // simply set WindowState = FullScreen while the window is maximized.
    private async void OnReproduce(object? sender, RoutedEventArgs e) => await RunReproSequence();

    private void OnReset(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Normal;
        Width = 1440;
        Height = 733;
    }

    private void OnMaximize(object? sender, RoutedEventArgs e) => WindowState = WindowState.Maximized;

    private void OnFullScreen(object? sender, RoutedEventArgs e) => WindowState = WindowState.FullScreen;

    private void UpdateStateText(WindowState state)
    {
        if (StateText is null) return;
        StateText.Text = $"WindowState: {state,-10}  ClientSize: {ClientSize.Width:F0}x{ClientSize.Height:F0}";
    }
}
