namespace Fusonic.Extensions.AspNetCore.Validation;

public class RequestValidationException : Exception
{
    public IDictionary<string, List<string>> Errors { get; }
    public RequestValidationException(IDictionary<string, List<string>> errors) : base("One or more validation errors occurred.") => Errors = errors;
}