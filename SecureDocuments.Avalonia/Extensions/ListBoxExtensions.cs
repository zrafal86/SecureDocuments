using Avalonia.Controls;
using System.Reactive.Linq;

namespace SecureDocuments.Avalonia.Extensions;

public static class ListBoxExtensions
{
    public static IObservable<IList<T>> SelectionChanged<T>(this ListBox listBox)
    {
        return Observable
            .FromEventPattern<SelectionChangedEventArgs>(
                h => listBox.SelectionChanged += h,
                h => listBox.SelectionChanged -= h)
            .Select(_ => listBox.SelectedItems?.OfType<T>().ToList() ?? new List<T>())
            .Select(list => (IList<T>)list);
    }
}
