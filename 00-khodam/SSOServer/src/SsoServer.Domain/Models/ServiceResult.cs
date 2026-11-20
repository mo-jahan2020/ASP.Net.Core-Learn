namespace SsoServer.Domain.Models;

/// <summary>Operation result with Persian error messages.</summary>
public class ServiceResult
{
    protected ServiceResult(bool succeeded, IReadOnlyList<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ServiceResult Ok() => new(true, Array.Empty<string>());

    public static ServiceResult Fail(params string[] errors) => new(false, errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray());

    public static ServiceResult Fail(IEnumerable<string> errors) => new(false, errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray());
}

/// <summary>Operation result carrying a value on success.</summary>
public sealed class ServiceResult<T> : ServiceResult
{
    private ServiceResult(bool succeeded, IReadOnlyList<string> errors, T? value)
        : base(succeeded, errors)
        => Value = value;

    public T? Value { get; }

    public static ServiceResult<T> Ok(T value) => new(true, Array.Empty<string>(), value);

    public static new ServiceResult<T> Fail(params string[] errors) => new(false, errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray(), default);

    public static new ServiceResult<T> Fail(IEnumerable<string> errors) => new(false, errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray(), default);
}
