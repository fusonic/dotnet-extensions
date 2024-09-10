// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Fusonic.Extensions.Mediator;

namespace Fusonic.Extensions.Email;

public class SendEmailBccDecorator(IRequestHandler<SendEmail, Unit> innerHandler, SendEmailBccDecorator.Options options) : IRequestHandler<SendEmail, Unit>
{
    public record Options(string BccAddress);

    public Task<Unit> Handle(SendEmail request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BccRecipient))
        {
            request = request with
            {
                BccRecipient = options.BccAddress
            };
        }

        return innerHandler.Handle(request, cancellationToken);
    }
}