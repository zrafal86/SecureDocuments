namespace SecureDocuments.WPF.Extensions
{
    public static class ListViewExtensions
    {
        public static IObservable<List<T>> SelectionChanged<T>(this ListView listView)
        {
            return Observable.FromEventPattern<SelectionChangedEventHandler, SelectionChangedEventArgs>(
                eh => listView.SelectionChanged += eh,
                eh => listView.SelectionChanged -= eh)
                .Select(_ => listView.SelectedItems.Cast<T>().ToList());
        }
    }
}
