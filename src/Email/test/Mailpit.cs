// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Net.Http.Json;
using MimeKit;

namespace Fusonic.Extensions.Email.Tests;

/// <summary>
/// Reads the emails received by Mailpit through its HTTP API.
/// Mailpit keeps a single inbox, so test classes using it must be in the "Mailpit" collection to not run in parallel.
/// </summary>
public static class Mailpit
{
    private static readonly HttpClient Api = new() { BaseAddress = TestStartup.MailpitApiAddress };

    public static async Task<IReadOnlyList<ReceivedEmail>> GetEmailsAsync()
    {
        var response = await Api.GetFromJsonAsync<MessagesResponse>("api/v1/messages");
        var emails = new List<ReceivedEmail>();
        foreach (var summary in response!.Messages)
        {
            await using var raw = await Api.GetStreamAsync($"api/v1/message/{summary.Id}/raw");
            var message = await MimeMessage.LoadAsync(raw);
            emails.Add(new ReceivedEmail(message, [.. (summary.Bcc ?? []).Select(b => b.Address)]));
        }

        return emails;
    }

    public static async Task DeleteEmailsAsync()
        => (await Api.DeleteAsync("api/v1/messages")).EnsureSuccessStatusCode();

    private sealed record MessagesResponse(List<MessageSummary> Messages);
    private sealed record MessageSummary(string Id, List<Contact>? Bcc);
    private sealed record Contact(string Address);
}

/// <param name="Message">The received message.</param>
/// <param name="BccAddresses">The BCC recipients, which are only part of the SMTP envelope and not of the message headers.</param>
public record ReceivedEmail(MimeMessage Message, IReadOnlyList<string> BccAddresses);
