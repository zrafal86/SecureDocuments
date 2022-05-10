using SecureDocuments.Services;
using System.Windows.Forms;

namespace SecureDocuments.WPF.Services
{
    internal class FileChooser : IFileChooser
    {
        public string[] ChooseFiles()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
                return dialog.FileNames;
            return Array.Empty<string>();
        }

        public string ChooseFolder()
        {
            using var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK) return dialog.SelectedPath;
            return string.Empty;
        }
    }
}