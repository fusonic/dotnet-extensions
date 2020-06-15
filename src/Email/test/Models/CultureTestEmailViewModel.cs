using System;

namespace Fusonic.Extensions.Email.Tests.Models
{
    [EmailView("Emails/CultureTest")]
    public class CultureTestEmailViewModel
    {
        public DateTime Date { get; set; } = new DateTime(2019, 1, 9);
        public double Number { get; set; } = 1234567.89;
    }
}