using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Azure.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UTT
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        public bool UseAzureSignalR => Configuration[ServiceOptions.ConnectionStringDefaultKey] != null;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            var builder = services.AddSignalR();

            if (UseAzureSignalR)
            {
                builder.AddAzureSignalR();
            }

            services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

            services.AddAuthentication("Cookies")
                    .AddCookie()
                    .AddTwitter(
                        options => Configuration.GetSection("twitter").Bind(options))
                    .AddGoogle(
                        options => Configuration.GetSection("google").Bind(options));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders();
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();

            if (UseAzureSignalR)
            {
                app.UseAzureSignalR(routes =>
                {
                    routes.MapHub<UTTHub>("/utt");
                });
            }
            else
            {
                app.UseSignalR(routes =>
                {
                    routes.MapHub<UTTHub>("/utt");
                });
            }
        }
    }
}
