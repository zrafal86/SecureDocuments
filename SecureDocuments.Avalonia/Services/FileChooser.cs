using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SecureDocuments.Services;

namespace SecureDocuments.Avalonia.Services;

public class FileChooser : IFileChooser
{
    private static Window? _mainWindow;

    public static void SetMainWindow(Window window) => _mainWindow = window;

    public async Task<string[]> ChooseFilesAsync()
    {
        if (_mainWindow == null) return Array.Empty<string>();

        var files = await _mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            Title = "Select files"
        });

        return files.Select(f => f.Path.LocalPath).ToArray();
    }

    public async Task<string> ChooseFolderAsync()
    {
        if (_mainWindow == null) return string.Empty;

        var folders = await _mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select folder",
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : string.Empty;
    }
}
