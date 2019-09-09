using System.Threading;
using System.Threading.Tasks;

namespace Fusonic.Extensions.Validation
{
    public interface IValidator<T>
        where T : notnull
    {
        Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken);
    }
}