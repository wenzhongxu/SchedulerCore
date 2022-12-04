using MimeKit;
using Scheduler.Common.Helpers.Entities;

namespace SchedulerCore.Host.Common
{
    public static class MailHelper
    {
        public static async Task<bool> SendMail(string title, string content, MailEntity mailInfo = null)
        {
            if (mailInfo == null)
            {
                mailInfo = await FileConfig.GetMailInfoAsync();
                if (string.IsNullOrWhiteSpace(mailInfo.MailPwd) ||
                    string.IsNullOrWhiteSpace(mailInfo.MailFrom) ||
                    string.IsNullOrWhiteSpace(mailInfo.MailHost))
                {
                    throw new Exception("请先在 [/seting] 页面配置邮箱设置。");
                }
            }
            else
            {
                mailInfo.MailFrom = mailInfo.MailFrom.Trim();
                mailInfo.MailHost = mailInfo.MailHost.Trim();
                mailInfo.MailTo = mailInfo.MailTo.Trim();
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mailInfo.MailFrom, mailInfo.MailFrom));
            foreach (var mailTo in mailInfo.MailTo.Replace("；", ";").Replace("，", ";").Replace(",", ";").Split(';'))
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

        public static async Task<bool> SendMail(string title, string content, string mailTo)
        {
            var info = await FileConfig.GetMailInfoAsync();
            if (string.IsNullOrWhiteSpace(info.MailPwd) || string.IsNullOrWhiteSpace(info.MailFrom) || string.IsNullOrWhiteSpace(info.MailHost))
                throw new Exception("请先在 [/seting] 页面配置邮箱设置。");
            info.MailTo = mailTo;
            return await SendMail(title, content, info);
        }
    }
}
