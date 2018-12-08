using Craidd.Config;
using Craidd.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Craidd.Extensions
{
    public static class EmailsSenderExtensions
    {
        /// <summary>
        /// Using Email Middleware
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> passed to the configuration method.</param>
        /// <param name="setupAction">The middleware configuration options.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddEmail(this IServiceCollection services, Action<EmailsServiceOptions> setupAction = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction != null)
            {
                services.Configure(setupAction); // IOptions<EmailsServiceOptions>
            }

            services.TryAddTransient<IEmailsService, EmailsService>();

            return services;
        }
    }
}
