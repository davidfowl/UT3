using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UTT
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();            
            services.AddSignalR();

            services.AddDbContext<IdentityDbContext>(
                options => options.UseInMemoryDatabase("MyDatabase"));

            services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<IdentityDbContext>();
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
                routes.MapHub<Chat>("/hubs/chat");
            });
        }
    }
}
