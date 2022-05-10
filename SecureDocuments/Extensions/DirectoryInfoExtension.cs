using Serilog;

namespace SecureDocuments.Extensions
{
    public static class DirectoryInfoExtension
    {

        /**
         * Grabs all interesting you files in directory structure
         */
        public static async Task<IEnumerable<string>?> WalkDirectoryTreeAsync(this DirectoryInfo root, string searchPattern = "", bool withSubDir = true)
        {
            return await Task.Run(() =>
            {
                string[]? files = null;
                try
                {
                    if (withSubDir)
                    {
                        files = Directory.GetFiles(root.FullName, searchPattern, SearchOption.AllDirectories);
                    }
                    else
                    {
                        files = Directory.GetFiles(root.FullName, searchPattern, SearchOption.TopDirectoryOnly);
                    }

                }
                catch (ArgumentOutOfRangeException e)
                {
                    Log.Logger.Error($"DirectoryInfoExtension -> WalkDirectoryTreeAsync: {e.Message}", e);
                }
                catch (UnauthorizedAccessException e)
                {
                    Log.Logger.Error($"DirectoryInfoExtension -> WalkDirectoryTreeAsync: {e.Message}", e);
                }
                catch (DirectoryNotFoundException e)
                {
                    Log.Logger.Error($"DirectoryInfoExtension -> WalkDirectoryTreeAsync: {e.Message}", e);
                }
                catch (PathTooLongException e)
                {
                    Log.Logger.Error($"DirectoryInfoExtension -> WalkDirectoryTreeAsync: {e.Message}", e);
                }
                return files?.ToArray();
            });
        }
    }
}