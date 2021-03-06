﻿using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extension class for the IServiceCollection type to provide supporting methods for the PnPContextFactory service
    /// </summary>
    public static class PnPContextFactoryCollectionExtensions
    {
        public static IServiceCollection AddPnPContextFactory(this IServiceCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            // Add a SharePoint Online Context Factory service instance
            return collection
                .AddSettings()
                .AddTelemetryServices()
                .AddHttpClients()
                .AddPnPServices();
        }

        public static IServiceCollection AddPnPContextFactory(this IServiceCollection collection, Action<PnPContextFactoryOptions> options)
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

            // Add a PnP Context Factory service instance
            return collection
                .AddSettings()
                .AddTelemetryServices()
                .AddHttpClients()
                .AddPnPServices();
        }

        private static IServiceCollection AddHttpClients(this IServiceCollection collection)
        {
            collection.AddHttpClient<SharePointRestClient>();
            collection.AddHttpClient<MicrosoftGraphClient>();

            return collection;
        }

        private static IServiceCollection AddPnPServices(this IServiceCollection collection)
        {
            return collection
                   .AddScoped<IPnPContextFactory, PnPContextFactory>();
        }

        private static IServiceCollection AddTelemetryServices(this IServiceCollection collection)
        {
            var settingsService = collection.BuildServiceProvider().GetRequiredService<ISettings>();

            // Setup Azure App Insights
            // See https://github.com/microsoft/ApplicationInsights-Home/tree/master/Samples/WorkerServiceSDK/WorkerServiceSampleWithApplicationInsights as example
            return collection.AddApplicationInsightsTelemetryWorkerService(options =>
            {
                if (!settingsService.DisableTelemetry)
                {
                    // Production AppInsights
                    options.InstrumentationKey = "ffe6116a-bda0-4f0a-b0cf-d26f1b0d84eb";
                }
            });
        }

    }
}
