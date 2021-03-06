﻿using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using JsonApiDotNetCore.Extensions;

using Craidd.Auth;
using Craidd.Config;
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

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var dbPath = System.IO.Path.GetFullPath(Configuration["Db:Path"]);
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=" + dbPath));

            services.AddTransient<IApiResponseHelper, ApiResponseHelper>();
            services.AddSingleton<ITemplatesService>(service => new TemplatesService(templatePath: System.IO.Path.Combine(Environment.CurrentDirectory, "Views")));

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

            var builder = services.AddIdentityCore<User>(options =>
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
            });

            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            builder.AddEntityFrameworkStores<AppDbContext>()
                   .AddDefaultTokenProviders();
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();

            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // authorization management
            services.AddAuthorization();
            // register the scope authorization handler
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, HasScopeHandler>();

            services.AddJsonApi<AppDbContext>(options =>
            {
                options.Namespace = "api";
                options.ValidateModelState = true;
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = Configuration["Project:Name"], Version = "v1" });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
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

            app.UseCors(options =>
                options.WithOrigins("http://localhost:3000")
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials()
            );

            app.UseAuthentication();
            app.UseJsonApi();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rheoli V1");
            });
        }
    }
}
