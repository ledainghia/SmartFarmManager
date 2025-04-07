﻿using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Org.BouncyCastle.Asn1.Pkcs;
using SmartFarmManager.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class EmailService
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettingsOptions)
        {
            _mailSettings = mailSettingsOptions.Value;
        }

        public async Task<bool> SendEmailAsync(MailData mailData)
        {
            Console.WriteLine(_mailSettings.SenderName);
            Console.WriteLine(_mailSettings.SenderEmail);
            Console.WriteLine(_mailSettings.UserName);
            Console.WriteLine(_mailSettings.Password);
            try
            {
                using (var emailMessage = new MimeMessage())
                {
                    MailboxAddress emailFrom = new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail);
                    emailMessage.From.Add(emailFrom);
                    MailboxAddress emailTo = new MailboxAddress(mailData.EmailToName, mailData.EmailToId);
                    emailMessage.To.Add(emailTo);
                    emailMessage.Subject = mailData.EmailSubject;
                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = mailData.EmailBody;
                    emailMessage.Body = bodyBuilder.ToMessageBody();
                    using (var client = new SmtpClient())
                    {
                        client.Connect(_mailSettings.Server, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                        //client.Authenticate(_mailSettings.UserName, _mailSettings.Password);
                        client.Authenticate(_mailSettings.UserName, "ngsd qlcf yfli nbfw");
                        client.Send(emailMessage);
                        client.Disconnect(true);
                    }
                };

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // Log the exception or handle it as per your application's requirement
                return false;
            }
        }

        public async Task SendReminderEmailAsync(string emailToId, string emailToName, string emailSubject, string emailBody)
        {
            var mailData = new MailData
            {
                EmailToId = emailToId,
                EmailToName = emailToName,
                EmailSubject = emailSubject,
                EmailBody = emailBody
            };
           bool success= await SendEmailAsync(mailData);
            Console.WriteLine(success
               ? $"Email sent successfully to {emailToId}"
               : $"Failed to send email to {emailToId}");
        }


    }
}
