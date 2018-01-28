using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.Swagger;

namespace Craidd
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
            var basePath = AppContext.BaseDirectory;
            var dbPath = System.IO.Path.Combine(basePath, "AppDb.db");
            services.AddDbContext<Craidd.Services.AppDbContext>(opt => opt.UseSqlite("Data Source=" + dbPath));
            services.AddMvc();

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info { Title = "Rheoli API", Version = "v1" });
                // Set the comments path for the Swagger JSON and UI.
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Craidd.xml");
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rheoli V1");
            });

            app.UseMvc();
        }
    }
}
