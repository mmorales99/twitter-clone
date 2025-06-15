using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace twitter_clone_api.utils
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}