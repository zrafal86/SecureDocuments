namespace SecureDocuments.Services
{
    public interface IDialogService
    {
        public const string DialogIdentifier = "MainDialogHost";
        public const string OfferFilesDialogIdentifier = "OfferFilesDialogHost";
        Task<object?> ShowDialog(object content);
        Task<object?> ShowDialog(string title, string message, bool isVisible = false);
        Task<string> ShowWaitDialog();
        void Close(string identifier);
    }
}