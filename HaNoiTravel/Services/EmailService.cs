using HaNoiTravel.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace HaNoiTravel.Services
{
    public class EmailService:IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            _logger.LogInformation($"Attempting to send email to: {toEmail}");
            _logger.LogInformation($"Subject: {subject}");
            _logger.LogInformation($"Content: {message}");

            try
            {
                // ------------- BẮT ĐẦU PHẦN GỬI EMAIL THỰC TẾ (SỬ DỤNG MailKit) -------------
                // Cần cài đặt gói NuGet: MailKit
                // dotnet add package MailKit

                var emailSettings = _configuration.GetSection("SmtpSettings");
                var host = emailSettings["Host"];
                var port = int.Parse(emailSettings["Port"] ?? "587");
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
                var fromEmail = emailSettings["FromEmail"] ?? username; // Email gửi đi

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(fromEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = message };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, port, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                // ------------- KẾT THÚC PHẦN GỬI EMAIL THỰC TẾ -------------

                // Nếu không dùng MailKit, chỉ giả lập thành công
                await Task.CompletedTask;
                _logger.LogInformation($"Email sent successfully to {toEmail}.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}.");
                // Tùy chọn: ném lại lỗi hoặc xử lý khác
                throw;
            }
        }
    }
}
