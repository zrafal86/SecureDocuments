using DialogHostAvalonia;
using SecureDocuments.Services;

namespace SecureDocuments.Avalonia.Services;

public sealed class DialogService : IDialogService
{
    public async Task<object?> ShowDialog(object content)
    {
        return await DialogHost.Show(content, IDialogService.DialogIdentifier);
    }

    public async Task<object?> ShowDialog(string title, string message, bool isVisible = false)
    {
        if (!DialogHost.IsDialogOpen(IDialogService.DialogIdentifier))
        {
            return await DialogHost.Show(new MessageViewModel(title, message, isVisible), IDialogService.DialogIdentifier);
        }
        return null;
    }

    public async Task<string> ShowWaitDialog()
    {
        if (!DialogHost.IsDialogOpen(IDialogService.DialogIdentifier))
        {
            _ = DialogHost.Show(new WaitViewModel(), IDialogService.DialogIdentifier);
        }
        return await Task.FromResult(IDialogService.DialogIdentifier);
    }

    public void Close(string identifier)
    {
        if (DialogHost.IsDialogOpen(identifier))
        {
            DialogHost.Close(identifier);
        }
    }
}

public class WaitViewModel
{
}

public class MessageViewModel
{
    public MessageViewModel(string title, string message, bool showCancel = false)
    {
        Title = title;
        Message = message;
        ShowCancel = showCancel;
    }

    public string Title { get; }
    public string Message { get; }
    public bool ShowCancel { get; }
}
