using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Vanguard_Engine.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string username, string verificationLink)
    {
        var host = _configuration["SmtpSettings:Host"] ?? "smtp.gmail.com";
        var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
        var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"] ?? "true");
        var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "no-reply@vanguardengine.com";
        var senderName = _configuration["SmtpSettings:SenderName"] ?? "Vanguard Engine Security";
        var password = _configuration["SmtpSettings:Password"];

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "Verify Your Vanguard Engine Account",
            Body = $@"
            <div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f6f9fc; padding: 40px 20px; color: #333333; line-height: 1.6;"">
                <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); border-top: 5px solid #b45309;"">
                    
                    <!-- Header -->
                    <div style=""padding: 30px; text-align: center; background-color: #1e293b; color: #ffffff;"">
                        <h1 style=""margin: 0; font-size: 26px; font-weight: 700; letter-spacing: 1px;"">VANGUARD ENGINE</h1>
                        <p style=""margin: 5px 0 0 0; font-size: 14px; color: #cbd5e1; text-transform: uppercase; letter-spacing: 2px;"">Security Guard & VIP Protection</p>
                    </div>

                    <!-- Content -->
                    <div style=""padding: 40px 30px;"">
                        <h2 style=""margin-top: 0; color: #1e293b; font-size: 20px; font-weight: 600;"">Welcome, {username}!</h2>
                        <p style=""color: #475569; font-size: 16px;"">
                            Thank you for registering an account with Vanguard Engine. To complete your registration and activate your secure profile, please verify your email address.
                        </p>
                        
                        <!-- CTA Button -->
                        <div style=""text-align: center; margin: 35px 0;"">
                            <a href=""{verificationLink}"" style=""background-color: #b45309; color: #ffffff; padding: 14px 30px; text-decoration: none; font-size: 16px; font-weight: 600; border-radius: 6px; display: inline-block; box-shadow: 0 4px 6px rgba(180, 83, 9, 0.2); transition: background-color 0.2s; border: none; outline: none; cursor: pointer;"">
                                Verify Account
                            </a>
                        </div>

                        <!-- Link Fallback -->
                        <p style=""color: #64748b; font-size: 13px; margin-top: 30px;"">
                            If the button above does not work, copy and paste the following URL into your web browser:
                            <br />
                            <a href=""{verificationLink}"" style=""color: #b45309; text-decoration: underline;"">{verificationLink}</a>
                        </p>

                        <!-- Expiration Notice -->
                        <div style=""margin-top: 30px; padding: 15px; background-color: #fffbeb; border-left: 4px solid #f59e0b; border-radius: 4px;"">
                            <p style=""margin: 0; font-size: 14px; color: #b45309; font-weight: 500;"">
                                <strong>Notice:</strong> This verification link will expire in 24 hours.
                            </p>
                        </div>

                        <!-- Security Note -->
                        <hr style=""border: 0; border-top: 1px solid #e2e8f0; margin: 30px 0;"" />
                        <p style=""color: #94a3b8; font-size: 12px; margin: 0;"">
                            <strong>Security Note:</strong> If you did not create a Vanguard Engine account using this email address, please ignore this email or contact support if you suspect unauthorized activity.
                        </p>
                    </div>

                    <!-- Footer -->
                    <div style=""padding: 20px 30px; background-color: #f8fafc; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #e2e8f0;"">
                        <p style=""margin: 0 0 5px 0;"">&copy; {DateTime.UtcNow.Year} Vanguard Engine. All rights reserved.</p>
                        <p style=""margin: 0;"">This is an automated operational security message. Please do not reply directly to this email.</p>
                    </div>
                </div>
            </div>
            ",
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        using var smtpClient = new SmtpClient(host, port)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(senderEmail, password),
            EnableSsl = enableSsl
        };

        await smtpClient.SendMailAsync(mailMessage);
    }

    public async Task SendPasswordResetOtpEmailAsync(string toEmail, string username, string otpCode)
    {
        var host = _configuration["SmtpSettings:Host"] ?? "smtp.gmail.com";
        var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
        var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"] ?? "true");
        var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? "no-reply@vanguardengine.com";
        var senderName = _configuration["SmtpSettings:SenderName"] ?? "Vanguard Engine Security";
        var password = _configuration["SmtpSettings:Password"];

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "Your Vanguard Engine Password Reset Code",
            Body = $@"
            <div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f6f9fc; padding: 40px 20px; color: #333333; line-height: 1.6;"">
                <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); border-top: 5px solid #b45309;"">
                    
                    <!-- Header -->
                    <div style=""padding: 30px; text-align: center; background-color: #1e293b; color: #ffffff;"">
                        <h1 style=""margin: 0; font-size: 26px; font-weight: 700; letter-spacing: 1px;"">VANGUARD ENGINE</h1>
                        <p style=""margin: 5px 0 0 0; font-size: 14px; color: #cbd5e1; text-transform: uppercase; letter-spacing: 2px;"">Security Guard &amp; VIP Protection</p>
                    </div>

                    <!-- Content -->
                    <div style=""padding: 40px 30px;"">
                        <h2 style=""margin-top: 0; color: #1e293b; font-size: 20px; font-weight: 600;"">Password Reset Request</h2>
                        <p style=""color: #475569; font-size: 16px;"">
                            Hello, {username}. We received a request to reset the password for your Vanguard Engine account. Enter the following 6-digit code to verify your identity.
                        </p>
                        
                        <!-- OTP Box -->
                        <div style=""text-align: center; margin: 40px 0;"">
                            <div style=""background-color: #f8fafc; border: 2px dashed #b45309; padding: 20px; border-radius: 8px; display: inline-block;"">
                                <span style=""font-size: 36px; font-weight: 800; letter-spacing: 8px; color: #b45309; font-family: monospace;"">{otpCode}</span>
                            </div>
                        </div>

                        <!-- Expiration Notice -->
                        <div style=""margin-top: 30px; padding: 15px; background-color: #fffbeb; border-left: 4px solid #f59e0b; border-radius: 4px;"">
                            <p style=""margin: 0; font-size: 14px; color: #b45309; font-weight: 500;"">
                                <strong>Notice:</strong> This verification code will expire in <strong>15 minutes</strong>. Do not share this code with anyone.
                            </p>
                        </div>

                        <!-- Security Note -->
                        <hr style=""border: 0; border-top: 1px solid #e2e8f0; margin: 30px 0;"" />
                        <p style=""color: #94a3b8; font-size: 12px; margin: 0;"">
                            <strong>Security Notice:</strong> If you did not request a password reset for your Vanguard Engine account, you can safely ignore this email. Your password will not be changed. If you believe this is unauthorized activity, please contact support immediately.
                        </p>
                    </div>

                    <!-- Footer -->
                    <div style=""padding: 20px 30px; background-color: #f8fafc; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #e2e8f0;"">
                        <p style=""margin: 0 0 5px 0;"">&copy; {DateTime.UtcNow.Year} Vanguard Engine. All rights reserved.</p>
                        <p style=""margin: 0;"">This is an automated operational security message. Please do not reply directly to this email.</p>
                    </div>
                </div>
            </div>
            ",
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        using var smtpClient = new SmtpClient(host, port)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(senderEmail, password),
            EnableSsl = enableSsl
        };

        await smtpClient.SendMailAsync(mailMessage);
    }
}
