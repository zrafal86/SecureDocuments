#nullable enable

namespace SecureDocuments.Models.Common
{
    public record Result<T>
    {
        public Result(T value, Error[]? errors = null)
        {
            Value = value;
            if (errors != null) Errors = errors;
        }

        public T Value { get; init; }
        public Error[]? Errors { get; init; }
    }
}