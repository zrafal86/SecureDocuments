using SecureDocuments.Models;
using SecureDocuments.Models.Common;
using Splat;
using System.Net;
using System.Net.Mail;

namespace SecureDocuments.Services
{
    public abstract class Notification
    {
        public abstract Task<Result<NotificationResult>> SendNotification(Offer? offer, Status oldStatus, Status newStatus);
    }

    public record NotificationResult
    {
        public NotificationResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; init; }
    }

    public sealed class EmailConfigNotification : Notification
    {
        private readonly IUserService? _userService;
        private readonly IResourceService? _resourceService;

        public EmailConfigNotification(IUserService? userService)
        {
            _userService = userService;
            _resourceService = Locator.Current.GetService<IResourceService>();
        }

        private async Task<Result<NotificationResult>> Send(Message message)
        {
            try
            {
                if (_userService is not null)
                {
                    var appConfig = await _userService.GetAppConfig();
                    if (appConfig != null)
                    {
                        var emailConfig = appConfig.EmailNotification;
                        var smtpClient = new SmtpClient(emailConfig!.Host)
                        {
                            Port = emailConfig!.Port,
                            Credentials = new NetworkCredential(emailConfig.CredentialUserName, emailConfig.CredentialPassword),
                            EnableSsl = emailConfig.EnableSsl,
                            Timeout = emailConfig.Timeout
                        };

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(emailConfig.From),
                            Subject = message.Subject,
                            Body = message.Body,
                            IsBodyHtml = message.IsHtmlBody,
                        };

                        foreach (var recipient in message.Recipients)
                        {
                            if (!string.IsNullOrEmpty(recipient))
                            {
                                mailMessage.To.Add(recipient);
                            }
                        }

                        smtpClient.Send(mailMessage);
                        return new Result<NotificationResult>(new NotificationResult(true), null);
                    }
                }
                return new Result<NotificationResult>(new NotificationResult(false), new Error[] { new Error("Error has occurred during sending notification") });
            }
            catch (Exception e)
            {
                return new Result<NotificationResult>(new NotificationResult(false), new Error[] { new Error("Error has occurred during sending notification", e) });
            }
        }

        public override async Task<Result<NotificationResult>> SendNotification(Offer? offer, Status oldStatus, Status newStatus)
        {
            if (oldStatus != newStatus)
            {
                var offerId = offer?.OfferNumber;
                var offerName = offer?.Name;
                var offerStatus = newStatus.ToString();
                var applicantEmail = offer?.Applicant?.Email;
                var managerEmail = offer?.Manager?.Email;
                var builderEmail = offer?.Builder?.Email;
                var technologistEmail = offer?.Technologist?.Email;

                var recipients = GerRecipientsList(applicantEmail, managerEmail, builderEmail, technologistEmail);

                var subject = "Offer App Notifies about changed status";
                var emailTemplate = _resourceService?.GetEmailTemplate();
                emailTemplate = emailTemplate?.Replace("#title", $"The Offer '{offerName}' changed an status from '{oldStatus}' to '{newStatus}'.");
                emailTemplate = emailTemplate?.Replace("#offerID", offerId ?? "");
                emailTemplate = emailTemplate?.Replace("#offerName", offerName ?? "");
                emailTemplate = emailTemplate?.Replace("#offerStatus", offerStatus ?? "nie znany");
                emailTemplate = emailTemplate?.Replace("#manager", managerEmail ?? "brak emaila");
                emailTemplate = emailTemplate?.Replace("#builder", builderEmail ?? "brak emaila");
                emailTemplate = emailTemplate?.Replace("#technologist", technologistEmail ?? "brak emaila");

                var message = new Message(subject, emailTemplate, recipients.Distinct().ToList());
                return await Send(message);
            }
            return new Result<NotificationResult>(new NotificationResult(false), new Error[] { new Error("Error has occurred during sending notification") });
        }

        private static List<string> GerRecipientsList(string? applicantEmail, string? managerEmail, string? builderEmail, string? technologistEmail)
        {
            var recipients = new List<string>
            {
                "admin@admin.pl",
            };
            if (applicantEmail != null)
            {
                //recipients.Add(applicantEmail);
            }
            if (managerEmail != null)
            {
                recipients.Add(managerEmail);
            }
            if (builderEmail != null)
            {
                recipients.Add(builderEmail);
            }
            if (technologistEmail != null)
            {
                recipients.Add(technologistEmail);
            }

            return recipients;
        }
    }

    public record Message
    {
        public Message(string subject, string body, List<string> recipients, bool isHtmlBody = true)
        {
            Subject = subject;
            Body = body;
            Recipients = recipients;
            IsHtmlBody = isHtmlBody;
        }

        public string Subject { get; init; }
        public string Body { get; init; }
        public List<string> Recipients { get; init; }
        public bool IsHtmlBody { get; init; }
    }
}
