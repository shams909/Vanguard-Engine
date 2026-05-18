using System.Threading.Tasks;

namespace Vanguard_Engine.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string username, string verificationLink);
    Task SendPasswordResetEmailAsync(string toEmail, string username, string resetLink);
}
