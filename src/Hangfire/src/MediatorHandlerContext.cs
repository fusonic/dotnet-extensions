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
    }

    public object Message { get; set; }
    public string HandlerType { get; set; }
    public CultureInfo? Culture { get; set; }
    public CultureInfo? UiCulture { get; set; }
    public HangfireUser? User { get; set; }

    public class HangfireUser
    {
        public HangfireUser(List<HangfireUserClaim> claims)
            => Claims = claims;

        public List<HangfireUserClaim> Claims { get; }

        public static HangfireUser FromClaimsPrincipal(ClaimsPrincipal principal) =>
            new(principal.Claims.Select(x => new HangfireUserClaim(x.Type, x.Value, x.ValueType, x.Issuer, x.OriginalIssuer)).ToList());

        public ClaimsPrincipal ToClaimsPrincipal()
            => new(new ClaimsIdentity(Claims.Select(x => new Claim(x.Type, x.Value, x.ValueType, x.Issuer, x.OriginalIssuer))));

        public class HangfireUserClaim
        {
            public HangfireUserClaim(string type, string value, string valueType, string issuer, string originalIssuer)
            {
                Type = type;
                Value = value;
                ValueType = valueType;
                Issuer = issuer;
                OriginalIssuer = originalIssuer;
            }

            public string Type { get; }
            public string Value { get; }
            public string ValueType { get; }
            public string Issuer { get; }
            public string OriginalIssuer { get; }
        }
    }
}
