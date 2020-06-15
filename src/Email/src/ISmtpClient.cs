using System.Threading.Tasks;
using MimeKit;

namespace Fusonic.Extensions.Email
{
    public interface ISmtpClient
    {
        Task SendMailAsync(MimeMessage message);
    }
}