#nullable enable

namespace SecureDocuments.Services
{
    public interface IFileChooser
    {
        public string ChooseFolder();

        public string[] ChooseFiles();
    }
}