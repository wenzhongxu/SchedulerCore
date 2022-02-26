using Newtonsoft.Json;
using SchedulerCore.Host.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Common
{
    public static class FileConfig
    {
        private static readonly string filePath = "File/Mail.txt";
        private static readonly string mqttFilePath = "File/Mqtt.json";
        private static readonly string rabbitFilePath = "File/rabbitmq.json";
        private static MailEntity mailData = null;

        public static async Task<MailEntity> GetMailInfoAsync()
        {
            if (mailData == null)
            {
                if (!System.IO.File.Exists(filePath))
                {
                    return new MailEntity();
                }
                var mail = await System.IO.File.ReadAllTextAsync(filePath);
                mailData = JsonConvert.DeserializeObject<MailEntity>(mail);
            }
            //深拷贝
            return JsonConvert.DeserializeObject<MailEntity>(JsonConvert.SerializeObject(mailData));
        }

    }
}
