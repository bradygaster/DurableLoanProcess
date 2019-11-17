using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DurableLoans.LoanOffice.ToBeApproved
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInboxStorageSupport();
            services.AddControllers();
            services.AddGrpc();
            services.AddSwaggerGen(c =>
            {
                c.DocumentFilter<AddHostDocumentFilter>();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LoanOffice",
                    Version = "v1"
                });
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
            app.UseStaticFiles();
            app.UseFileServer();

            app.UseSwagger(c =>
            {
                // PA-REQ: this needs to be done to enable PowerApps since they're V2
                c.SerializeAsV2 = true;
                // END PA-REQ
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoanOffice");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<FeedService>();
                endpoints.MapGet("/proto", async req =>
                {
                    await req.Response.SendFileAsync("Protos/LoanOffice.proto", req.RequestAborted);
                });
                endpoints.MapGet("/", async req => await req.Response.WriteAsync("Healthy"));
                endpoints.MapControllers();
            });
        }
    }

    public class AddHostDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Servers = new List<OpenApiServer>()
            {
                new OpenApiServer()
                {
                    Url = "http://localhost:5005"
                }
            };
        }
    }
}
