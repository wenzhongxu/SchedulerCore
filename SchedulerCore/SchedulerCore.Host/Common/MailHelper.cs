using MimeKit;
using Quartz.Util;
using SchedulerCore.Host.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Common
{
    public static class MailHelper
    {
        public static async Task<bool> SendMail(string title, string content, MailEntity mailInfo = null)
        {
            if (mailInfo == null)
            {
                mailInfo = await FileConfig.GetMailInfoAsync();
                if (mailInfo.MailPwd.IsNullOrWhiteSpace() || mailInfo.MailHost.IsNullOrWhiteSpace() || mailInfo.MailFrom.IsNullOrWhiteSpace())
                {
                    throw new Exception("请先配置邮箱");
                }
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mailInfo.MailFrom, mailInfo.MailFrom));
            foreach (var mailTo in mailInfo.MailTo.Replace("；", ";").Replace(",", ";").Replace("，", ";").Split(";"))
            {
                message.To.Add(new MailboxAddress(mailTo, mailTo));
            }
            message.Subject = string.Format(title);
            message.Body = new TextPart("html")
            {
                Text = content
            };
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(mailInfo.MailHost, 465, true);
                await client.AuthenticateAsync(mailInfo.MailFrom, mailInfo.MailPwd);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            return true;
        }
    }
}
