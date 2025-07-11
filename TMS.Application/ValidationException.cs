using FluentValidation.Results;

namespace TMS.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("Wyst�pi� jeden lub wi�cej b��d�w walidacji.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}