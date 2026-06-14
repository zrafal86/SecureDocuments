using SecureDocuments.Services;
using System.Windows.Forms;

namespace SecureDocuments.WPF.Services
{
    internal class FileChooser : IFileChooser
    {
        public Task<string[]> ChooseFilesAsync()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
                return Task.FromResult(dialog.FileNames);
            return Task.FromResult(Array.Empty<string>());
        }

        public Task<string> ChooseFolderAsync()
        {
            using var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK) return Task.FromResult(dialog.SelectedPath);
            return Task.FromResult(string.Empty);
        }
    }
}