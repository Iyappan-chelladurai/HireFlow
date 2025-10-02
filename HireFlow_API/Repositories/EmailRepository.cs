using System.Net;
using System.Net.Mail;
using static HireFlow_API.Repositories.EmailRepository;

namespace HireFlow_API.Repositories
{
    public class EmailRequestDTO  
    {
        public string FromEmailAddress { get; set; }
        public List<string> ToEmailAddresses { get; set; }
        public List<string> BCCEmailAddresses { get; set; }
        public List<string> CCEmailAddresses { get; set; }
        public string EmailSubject { get; set; }
        public string HtmlEmailBody { get; set; }
        public List<Attachment> EmailAttachments { get; set; }
        public string EmailType { get; set; }

    }

        public interface IEmailRepository
    {
        Task SendEmail(EmailRequestDTO emailRequest);
    }

    public class EmailRepository : IEmailRepository
    {
        public EmailRepository()
        {

        }
        public async Task SendEmail(EmailRequestDTO emailRequest)
        {
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(emailRequest.FromEmailAddress);

                    foreach (var EmailAddress in emailRequest.ToEmailAddresses)
                    {
                        message.To.Add(EmailAddress);
                    }

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("hireflowofficial@gmail.com", "iswz peow ayvi kqjc");
                        smtp.EnableSsl = true;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;

                        if (emailRequest.EmailAttachments != null)
                        {
                            foreach (var EmailAttachment in emailRequest.EmailAttachments)
                            {
                                message.Attachments.Add(EmailAttachment);
                            }
                        }

                        message.Subject = emailRequest.EmailSubject;
                        message.IsBodyHtml = true;
                        message.Body = emailRequest.HtmlEmailBody;

                        // Force TLS 1.2 (important if targeting .NET Framework)
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        smtp.Send(message);
                    }
                }

            }
            catch (Exception ex) 
            {
            
            }
        }

    }
}
