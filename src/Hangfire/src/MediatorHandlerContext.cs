// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Globalization;
using System.Security.Claims;

namespace Fusonic.Extensions.Hangfire;

public class MediatorHandlerContext
{
    public MediatorHandlerContext(object message, string handlerType)
    {
        Message = message;
        HandlerType = handlerType;

        var type = Message.GetType();
        DisplayName = $"Message: {GetTypeName(type)} ({type.Namespace})";
    }

    public object Message { get; set; }
    public string HandlerType { get; set; }
    public CultureInfo? Culture { get; set; }
    public CultureInfo? UiCulture { get; set; }
    public HangfireUser? User { get; set; }
    public string DisplayName { get; set; }

    public override string ToString() => DisplayName;

    private string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        // Remove the `n suffix from generic type names (e.g. List`1 -> List)
        var index = type.Name.IndexOf('`', StringComparison.Ordinal);
        var typeName = index > 0 ? type.Name[..index] : type.Name;

        var genericNames = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));

        return $"{typeName}<{genericNames}>";
    }

    public class HangfireUser(List<HangfireUser.HangfireUserClaim> claims)
    {
        public List<HangfireUserClaim> Claims { get; } = claims;

        public static HangfireUser FromClaimsPrincipal(ClaimsPrincipal principal) =>
            new(principal.Claims.Select(x => new HangfireUserClaim(x.Type, x.Value, x.ValueType, x.Issuer, x.OriginalIssuer)).ToList());

        public ClaimsPrincipal ToClaimsPrincipal()
            => new(new ClaimsIdentity(Claims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer, x.OriginalIssuer))));

        public class HangfireUserClaim(string type, string value, string valueType, string issuer, string originalIssuer)
        {
            public string Type { get; } = type;
            public string Value { get; } = value;
            public string ValueType { get; } = valueType;
            public string Issuer { get; } = issuer;
            public string OriginalIssuer { get; } = originalIssuer;
        }
    }
}
