using AutoMapper;
using SchedulerCore.Host.Entities;
using SchedulerCore.Host.Models;

namespace SchedulerCore.Host.Profiles
{
    public class SendMailProfile : Profile
    {
        public SendMailProfile()
        {
            CreateMap<SendMail, SendMailDto>();
            CreateMap<SendMailDto, SendMail>();
        }
    }
}
