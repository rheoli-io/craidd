using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Swashbuckle.AspNetCore.Swagger;

using Craidd.Data;
using Craidd.Models;
using Craidd.Services;
using Craidd.Helpers;
using Craidd.Extensions;
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
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var dbPath = System.IO.Path.GetFullPath(Configuration["Db:Path"]);
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=" + dbPath));

            services.AddTransient<IApiResponseHelper, ApiResponseHelper>();
            services.AddSingleton<ITemplatesService>(service => new TemplatesService(templatePath: System.IO.Path.Combine(Environment.CurrentDirectory, "Views")));

            // services.AddScoped<IEmailsService>(service =>
            // {
            //     var options = Configuration.GetSection("Email:Smtp")
            //                                .GetChildren().ToDictionary(x => x.Key, x => x.Value);
            //     return new EmailsService(options: options, templatesService: service.GetService<ITemplatesService>());
            // });

            services.AddEmail(options =>
            {
                options.host = Configuration["Email:Smtp:Host"];
                options.port = Int32.Parse(Configuration["Email:Smtp:Port"]);
                options.useSSL = Boolean.Parse(Configuration["Email:Smtp:UseSSL"]);
                options.username = Configuration["Email:Smtp:Username"];
                options.password = Configuration["Email:Smtp:Password"];
                options.fromName = Configuration["Email:Smtp:FromName"];
                options.fromEmail = Configuration["Email:Smtp:FromEmail"];
            });

            services.AddScoped<TasksService>();
            services.AddScoped<IUsersService, UsersService>();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = Configuration["Jwt:Issuer"],
                            ValidateAudience = true,
                            ValidAudience = Configuration["Jwt:Issuer"],
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                            ValidateLifetime = true,
                            RequireExpirationTime = false,
                            ClockSkew = TimeSpan.Zero
                        };
                        options.SaveToken = true;
                        options.ClaimsIssuer = Configuration["Jwt:Issuer"];
                    });

            services.AddIdentity<User, IdentityRole>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;

                    // Lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.AllowedForNewUsers = true;

                    // User settings
                    options.User.RequireUniqueEmail = true;

                    // Require an email validation before login
                    options.SignIn.RequireConfirmedEmail = true;

                    options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
                })
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            services.AddCors();
            services.AddMvc();
            services.AddApiVersioning();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSwaggerGen(c =>
            {
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
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }

            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            );

            // System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimFilter.Clear();
            app.UseAuthentication();
            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rheoli V1");
            });
        }
    }
}
