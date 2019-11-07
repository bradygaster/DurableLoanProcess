using DurableLoans.Web.Extensions;
using DurableLoans.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;

namespace DurableLoans.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            IMvcBuilder builder = services.AddRazorPages();

#if DEBUG
            if (Env.IsDevelopment() && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                builder.AddRazorRuntimeCompilation();
            }
#endif

            services.AddServerSideBlazor();
            services.AddSingleton<CurrencyConversionService>();
            services.AddAutoMapper();
            services.AddLoanOfficerClient(options => 
            {
                options.Address = new Uri(Configuration["LoanApprovalService:BaseAddress"]);
            });

            IConfigurationSection loanServiceConfig = Configuration.GetSection("LendingService");

            services.AddHttpClient<LendingService>(config =>
                config.BaseAddress = new Uri(
                    $"{loanServiceConfig["BaseAddress"]}{loanServiceConfig["Routes:HttpStart"]}"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
