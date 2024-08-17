using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace FilePickerOptions.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SaveButton.Click += SaveFileDialog;
    }

    private async void SaveFileDialog(object? sender, RoutedEventArgs args)
    {
        var sp = GetStorageProvider();
        if (sp is null) return;
        var result = await sp.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Open File",
            }
        );
    }

    private IStorageProvider? GetStorageProvider()
    {
        var topLevel = GetTopLevel(this);
        return topLevel?.StorageProvider;
    }
}