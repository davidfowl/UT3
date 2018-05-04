using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSignalR();

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseInMemoryDatabase("MyDatabase"));

            services.AddAuthentication(o =>
                    {
                        o.DefaultScheme = "Cookies";
                        o.DefaultChallengeScheme = "Twitter";
                    })
                    .AddCookie()
                    .AddTwitter(o =>
                    {
                        o.ConsumerKey = Configuration["twitter:consumerKey"];
                        o.ConsumerSecret = Configuration["twitter:consumerSecret"];
                    });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();

            app.UseSignalR(routes =>
            {
                routes.MapHub<UTTHub>("/utt");
            });
        }
    }
}
