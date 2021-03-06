﻿using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extension class for the IServiceCollection type to provide supporting methods for the AuthenticationProviderFactory service
    /// </summary>

    public static class AuthenticationProviderFactoryCollectionExtensions
    {
        public static IServiceCollection AddAuthenticationProviderFactory(this IServiceCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            // Add a SharePoint Online Context Factory service instance
            return collection
                .AddSettings()
                .AddAuthenticationServices();
        }

        public static IServiceCollection AddAuthenticationProviderFactory(this IServiceCollection collection, Action<OAuthAuthenticationProviderOptions> options)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            collection.Configure(options);

            // Add an Authentication Provider Factory service instance
            return collection
                .AddSettings()
                .AddAuthenticationServices();
        }

        private static IServiceCollection AddAuthenticationServices(this IServiceCollection collection)
        {
            return collection
                .AddOAuthAuthenticationProvider()
                .AddScoped<IAuthenticationProviderFactory, AuthenticationProviderFactory>();
        }
    }
}
