using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SchedulerCore.Host.Managers;
using SchedulerCore.Host.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerCore.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 跨域     
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSameDomain", policyBuilder =>
                {
                    policyBuilder
                        .AllowAnyMethod()
                        .AllowAnyHeader();

                    var allowedHosts = Configuration.GetSection("AllowedHosts").Get<List<string>>();
                    if (allowedHosts?.Any(t => t == "*") ?? false)
                        policyBuilder.AllowAnyOrigin(); //允许任何来源的主机访问
                    else if (allowedHosts?.Any() ?? false)
                        policyBuilder.AllowCredentials().WithOrigins(allowedHosts.ToArray()); //允许类似http://localhost:8080等主机访问
                });
            });

            #endregion

            services.AddHostedService<HostedService>(); // 注册到hosted
            //services.AddSingleton<SchedulerManager>(); // 单例模式
            services.AddSingleton<SchedulerCenter>();

            services.AddControllers().AddXmlDataContractSerializerFormatters();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SchedulerCore.Host", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SchedulerCore.Host v1"));
            }

            app.UseCors(options =>
            {
                options.AllowAnyMethod();
                options.SetIsOriginAllowed(_ => true);
                options.AllowAnyHeader();
                options.AllowCredentials();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowSameDomain");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
