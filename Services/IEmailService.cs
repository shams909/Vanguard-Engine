using System.Threading.Tasks;

namespace Vanguard_Engine.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string username, string verificationLink);
    Task SendPasswordResetOtpEmailAsync(string toEmail, string username, string otpCode);
}
