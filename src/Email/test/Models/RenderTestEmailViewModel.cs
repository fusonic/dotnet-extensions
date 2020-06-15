namespace Fusonic.Extensions.Email.Tests.Models
{
    [EmailView("Emails/RenderTest")]
    public class RenderTestEmailViewModel
    {
        public string SomeField { get; set; } = null!;
    }
}