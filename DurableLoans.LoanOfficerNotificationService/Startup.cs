using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableLoans.LoanOfficerNotificationService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace DurableLoans.LoanOfficerNotificationService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(c => c.AddDefaultPolicy(builder => 
            {
                builder.AllowAnyOrigin();
            }));
            services.AddGrpc();
            services.AddControllers();
            services.AddSingleton<LoanApplicationProxy>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Loan Officer Approval Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Loan Officer API v1");
            });

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<LoanApplicationReceivedNotifierService>();
                endpoints.MapGet("/proto", async req =>
                {
                    await req.Response.SendFileAsync("Protos/LoanOffice.proto", req.RequestAborted);
                });
                endpoints.MapGet("/", async req => await req.Response.WriteAsync("Healthy"));
                endpoints.MapControllers();
            });
        }
    }
}
