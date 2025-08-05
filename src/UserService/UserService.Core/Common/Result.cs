namespace UserService.Core.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Error { get; }
    public List<string> Errors { get; }

    private Result(bool isSuccess, T? value, string error, List<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty, new List<string>());

    public static Result<T> Failure(string error) => new(false, default, error, new List<string> { error });

    public static Result<T> Failure(List<string> errors) => new(false, default, string.Join(", ", errors), errors);
}

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public List<string> Errors { get; }

    private Result(bool isSuccess, string error, List<string> errors)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors;
    }

    public static Result Success() => new(true, string.Empty, new List<string>());

    public static Result Failure(string error) => new(false, error, new List<string> { error });

    public static Result Failure(List<string> errors) => new(false, string.Join(", ", errors), errors);
}
