using BookHaven.Web.Settings;
using Microsoft.Extensions.Options;

// Install MailKit package
using MimeKit;
using MailKit.Net.Smtp; // Using this not System.Net.Mail 
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace BookHaven.Web.Services
{
	public class EmailSender : IEmailSender
	{
		private readonly EmailSettings _emailSetttings;
		private readonly IWebHostEnvironment _webHostEnvironment;
		//IOptions<> To read data that binding from section to class
		public EmailSender(IOptions<EmailSettings> emailSetttings, IWebHostEnvironment webHostEnvironment = null)
		{
			_emailSetttings = emailSetttings.Value;
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			MailMessage mailMessage = new MailMessage()
			{
				From = new MailAddress(_emailSetttings.Email, _emailSetttings.DisplayName), //من مين
				Body = htmlMessage,
				Subject = subject,
				IsBodyHtml = true //template
			};                  //إبعت علي ايميلي كتست حاليا
			mailMessage.To.Add(_webHostEnvironment.IsDevelopment()?"mohmedzeedan2222@gmail.com":email); //إلي مين
			SmtpClient smtpClient = new SmtpClient(_emailSetttings.Host)// المسؤل عن إرسال الإميل
			{
				Port = _emailSetttings.port,
				Credentials = new NetworkCredential(_emailSetttings.Email, _emailSetttings.Passowrd),
				EnableSsl = true
			};
			await smtpClient.SendMailAsync(mailMessage);
			smtpClient.Dispose();	
		}
	}
}
