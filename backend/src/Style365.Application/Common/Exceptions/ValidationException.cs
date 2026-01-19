namespace Style365.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors) 
        : base("Validation failed")
    {
        Errors = errors;
    }

    public ValidationException(string error) 
        : base("Validation failed")
    {
        Errors = new[] { error };
    }

    public ValidationException(string message, IEnumerable<string> errors) 
        : base(message)
    {
        Errors = errors;
    }
}