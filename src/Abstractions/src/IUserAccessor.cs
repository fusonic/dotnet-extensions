using System.Security.Claims;

namespace Fusonic.Extensions.Abstractions
{
    /// <summary>Allows accessing the current user.</summary>
    public interface IUserAccessor
    {
        /// <summary>Gets the current user.</summary>
        ClaimsPrincipal User { get; }
    }
}