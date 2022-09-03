﻿using Immense.RemoteControl.Desktop.Shared.Abstractions;
using System.Threading.Tasks;
using System.Threading;
using System;
using Immense.RemoteControl.Desktop.Windows;
using Remotely.Desktop.Win.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remotely.Shared.Services;
using Immense.RemoteControl.Desktop.Shared.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Immense.RemoteControl.Desktop.Shared.Enums;
using Immense.RemoteControl.Desktop.UI.Services;

var fallbackUri = "https://localhost:5001";

var provider = await Startup.UseRemoteControlClient(
    args,
    config =>
    {
        config.AddBrandingProvider<BrandingProvider>();
    },
    services =>
    {
        services.AddLogging(builder =>
        {
#if DEBUG
            builder.SetMinimumLevel(LogLevel.Debug);
#endif
            builder.AddProvider(new FileLoggerProvider());
        });

        services.RemoveAll<IAppState>();
        services.AddSingleton<IAppStateEx, AppStateEx>();
        services.AddSingleton<IAppState>(s => s.GetRequiredService<IAppStateEx>());
    },
    fallbackUri);


var brandingProvider = provider.GetRequiredService<IBrandingProvider>();
if (brandingProvider is BrandingProvider branding)
{
    await branding.TrySetFromApi();
}


Console.WriteLine("Press Ctrl + C to exit.");

var shutdownService = provider.GetRequiredService<IShutdownService>();
Console.CancelKeyPress += async (s, e) =>
{
    await shutdownService.Shutdown();
};

var dispatcher = provider.GetRequiredService<IAvaloniaDispatcher>();
try
{
    await Task.Delay(Timeout.InfiniteTimeSpan, dispatcher.AppCancellationToken);
}
catch (TaskCanceledException) { }