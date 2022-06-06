using System.Net;
using System.Net.Mail;
using System.Text;

namespace datotekica.Services;

public class MailService
{
    readonly ILogger<MailService> _logger;

    public MailService(ILogger<MailService> logger)
    {
        _logger = logger;
    }

    public bool IsSetup => C.Config.Current.SmtpPort.HasValue
        && !string.IsNullOrWhiteSpace(C.Config.Current.SmtpHost)
        && !string.IsNullOrWhiteSpace(C.Config.Current.SmtpPassword)
        && !string.IsNullOrWhiteSpace(C.Config.Current.SmtpFromName)
        && !string.IsNullOrWhiteSpace(C.Config.Current.SmtpFromAddress);

    public async Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        if (!IsSetup)
            throw new AggregateException("Mail service not setup");

        using var client = new SmtpClient(C.Config.Current.SmtpHost, C.Config.Current.SmtpPort!.Value);
        client.EnableSsl = C.Config.Current.SmtpSsl;

        if (!string.IsNullOrWhiteSpace(C.Config.Current.SmtpUser) && !string.IsNullOrWhiteSpace(C.Config.Current.SmtpPassword))
            client.Credentials = new NetworkCredential(C.Config.Current.SmtpUser, C.Config.Current.SmtpPassword);

        await client.SendMailAsync(message, cancellationToken);

        _logger.LogDebug("Mail {Subject} sent successfully to {To}", message.Subject, message.To[0].Address);
    }
    public MailMessage GetStandardMessage(MailAddress from, MailAddress to, string subject, string body)
    {
        var message = new MailMessage(from, to) { IsBodyHtml = true };
        message.BodyEncoding = message.HeadersEncoding = message.SubjectEncoding = Encoding.UTF8;
        message.Subject = subject;
        message.Body = body;

        // disable autoresponders like out-of-office
        message.Headers.Add("Auto-Submitted", "auto-generated");
        message.Headers.Add("Precedence", "bulk");
        // https://blog.mailtrap.io/list-unsubscribe-header/
        // https://certified-senders.org/wp-content/uploads/2017/07/CSA_one-click_list-unsubscribe.pdf
        //message.Headers.Add("List-Unsubscribe", "<http://www.example.com/unsubscribe.html>");

        return message;
    }
    public async Task SendTestEmailAsync(string testEmail, CancellationToken cancellationToken = default)
    {
        var from = new MailAddress(C.Config.Current.SmtpFromAddress, C.Config.Current.SmtpFromName);
        var to = new MailAddress(testEmail);
        var subject = "datotekica test";
        var body = "Your mail configuration works";
        var message = GetStandardMessage(from, to, subject, body);

        using var client = new SmtpClient(C.Config.Current.SmtpHost, C.Config.Current.SmtpPort!.Value);
        client.EnableSsl = C.Config.Current.SmtpSsl;

        if (!string.IsNullOrWhiteSpace(C.Config.Current.SmtpUser) && !string.IsNullOrWhiteSpace(C.Config.Current.SmtpPassword))
            client.Credentials = new NetworkCredential(C.Config.Current.SmtpUser, C.Config.Current.SmtpPassword);

        await client.SendMailAsync(message, cancellationToken);
    }
}