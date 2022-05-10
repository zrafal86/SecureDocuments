using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Models;
using Newtonsoft.Json;

namespace SecureDocuments.Data
{
    public interface ICollectionDataSource<T>
    {
        Task<IEnumerable<T>> GetAll(string filePath);

        Task Save(List<T> list, string filePath);
    }

    public interface IDataSource<T>
    {
        Task<T?> Get(string filePath);

        Task Save(T item, string filePath);
    }

    public abstract class DataSourceBase<T> : IDataSource<T>
    {
        protected readonly ISymmetricEncryption _encryption;

        public DataSourceBase(ISymmetricEncryption encryption)
        {
            _encryption = encryption;
        }

        public virtual async Task<T?> Get(string filePath)
        {
            var content = await ReadAllTextAsync(filePath, Encoding.UTF8);
            content = await _encryption.Decrypt(content, Role.Creator);
            if (!string.IsNullOrEmpty(content))
            {
                return JsonConvert.DeserializeObject<T>(content, JsonSerializerConfig.GetConfig());
            }
            return default;
        }

        public virtual async Task Save(T item, string filePath)
        {
            var data = await Task.Run(() =>
            {
                return JsonConvert.SerializeObject(item, JsonSerializerConfig.GetConfig());
            });
            data = await _encryption.Encrypt(data, Role.Creator);
            if (!string.IsNullOrEmpty(data))
            {
                await File.WriteAllTextAsync(filePath, data);
            }
        }

        private static async Task<string> ReadAllTextAsync(string filePath, Encoding encoding)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            using var sr = new StreamReader(fileStream, encoding);
            return await sr.ReadToEndAsync();
        }
    }

    public abstract class CollectionDataSourceBase<T> : ICollectionDataSource<T>
    {
        protected readonly ISymmetricEncryption _encryption;

        public CollectionDataSourceBase(ISymmetricEncryption encryption)
        {
            _encryption = encryption;
        }

        public virtual async Task<IEnumerable<T>> GetAll(string filePath)
        {
            var content = await ReadAllTextAsync(filePath, Encoding.UTF8);
            content = await _encryption.Decrypt(content, Role.Creator);
            if (!string.IsNullOrEmpty(content))
            {
                var deserialized = JsonConvert.DeserializeObject<T[]>(content);
                if (deserialized != null)
                {
                    return deserialized;
                }
            }
            return Enumerable.Empty<T>();
        }

        public virtual async Task Save(List<T> list, string filePath)
        {
            var data = await Task.Run(() =>
            {
                return JsonConvert.SerializeObject(list.ToArray(), JsonSerializerConfig.GetConfig());
            });
            data = await _encryption.Encrypt(data, Role.Creator);
            if (!string.IsNullOrEmpty(data))
                await File.WriteAllTextAsync(filePath, data);
        }

        private static async Task<string> ReadAllTextAsync(string filePath, Encoding encoding)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            using var sr = new StreamReader(fileStream, encoding);
            return await sr.ReadToEndAsync();
        }
    }
}