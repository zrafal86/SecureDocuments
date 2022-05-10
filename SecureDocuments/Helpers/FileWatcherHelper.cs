using SecureDocuments.Models;
using Splat;

namespace SecureDocuments.Helpers
{
    public class FileWatcherHelper : IEnableLogger
    {
        public IObservable<WatchedFile> WatchFilesInOfferFolder(string directory)
        {
            if (directory == null)
            {
                return Observable.Empty<WatchedFile>();
            }
            try
            {
                var fileWatcher = new FileSystemWatcher(directory)
                {
                    EnableRaisingEvents = true
                };
                return Observable
                    .Using(() => fileWatcher,
                        watcher =>
                            Observable.Merge(
                                Observable
                                    .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                                        ev => watcher.Created += ev,
                                        ev => watcher.Created -= ev)
                                    .Select(x => new WatchedFile
                                    {
                                        Name = x.EventArgs.Name,
                                        FullPath = x.EventArgs.FullPath,
                                        ChangeType = x.EventArgs.ChangeType,
                                        OldName = null,
                                        OldFullPath = null
                                    }),
                                Observable
                                    .FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
                                        ev => watcher.Renamed += ev,
                                        ev => watcher.Renamed -= ev)
                                    .Select(x => new WatchedFile
                                    {
                                        Name = x.EventArgs.Name,
                                        FullPath = x.EventArgs.FullPath,
                                        ChangeType = x.EventArgs.ChangeType,
                                        OldName = x.EventArgs.OldName,
                                        OldFullPath = x.EventArgs.OldFullPath
                                    }),
                                Observable
                                    .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                                        ev => watcher.Deleted += ev,
                                        ev => watcher.Deleted -= ev)
                                    .Select(x => new WatchedFile
                                    {
                                        Name = x.EventArgs.Name,
                                        FullPath = x.EventArgs.FullPath,
                                        ChangeType = x.EventArgs.ChangeType,
                                        OldName = null,
                                        OldFullPath = null
                                    }),
                                Observable
                                    .FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                                        ev => watcher.Changed += ev,
                                        ev => watcher.Changed -= ev)
                                    .Select(x => new WatchedFile
                                    {
                                        Name = x.EventArgs.Name,
                                        FullPath = x.EventArgs.FullPath,
                                        ChangeType = x.EventArgs.ChangeType,
                                        OldName = null,
                                        OldFullPath = null
                                    })
                            )).Delay(TimeSpan.FromMilliseconds(500));
            }
            catch (ArgumentNullException e)
            {
                this.Log().Error(e);
                return Observable.Empty<WatchedFile>();
            }
            catch (ArgumentException e)
            {
                this.Log().Error(e);
                return Observable.Empty<WatchedFile>();
            }
            catch (PathTooLongException e)
            {
                this.Log().Error(e);
                return Observable.Empty<WatchedFile>();
            }
        }
    }
}
