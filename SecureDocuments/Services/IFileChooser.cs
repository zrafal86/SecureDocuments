#nullable enable

namespace SecureDocuments.Services
{
    public interface IFileChooser
    {
        public string ChooseFolder() => Task.Run(ChooseFolderAsync).GetAwaiter().GetResult();

        public string[] ChooseFiles() => Task.Run(ChooseFilesAsync).GetAwaiter().GetResult();

        public Task<string> ChooseFolderAsync();

        public Task<string[]> ChooseFilesAsync();
    }
}