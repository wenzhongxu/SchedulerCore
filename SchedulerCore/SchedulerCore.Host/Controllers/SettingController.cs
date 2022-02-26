using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchedulerCore.Host.Attributes;
using SchedulerCore.Host.Common;
using SchedulerCore.Host.Entities;
using SchedulerCore.Host.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Controllers
{
    [Route("api/[controller]/[Action]")]
    [EnableCors("AllowSameDomain")]
    public class SettingController : Controller
    {
        private readonly IMapper _mapper;

        public SettingController(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [NoLogin]
        public async Task<bool> SendMail([FromBody] SendMailDto sendMailDto)
        {
            var sendMail = _mapper.Map<SendMail>(sendMailDto);
            return await MailHelper.SendMail(sendMail.Title, sendMail.Content, sendMail.MailInfo);
        }
    }
}
