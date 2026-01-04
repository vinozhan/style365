namespace Style365.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; private set; }
    public string[] Errors { get; private set; } = [];

    protected Result(bool isSuccess, string[] errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, []);

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result<T> Success<T>(T data) => new(true, data, []);

    public static Result<T> Failure<T>(params string[] errors) => new(false, default, errors);
}

public class Result<T> : Result
{
    public T? Data { get; private set; }

    internal Result(bool isSuccess, T? data, string[] errors) : base(isSuccess, errors)
    {
        Data = data;
    }

    public static implicit operator Result<T>(T data) => Success(data);

    public static implicit operator Result<T>(string error) => Failure<T>(error);
}