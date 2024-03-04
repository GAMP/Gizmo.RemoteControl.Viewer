using Gizmo.RemoteControl.Web.Viewer.Models.Settings;
using Gizmo.RemoteControl.Web.Viewer.Pages;
using Gizmo.RemoteControl.Web.Viewer.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gizmo.RemoteControl.Web.Viewer
{
    public static class Registrations
    {
        public static IServiceCollection AddViewer(this IServiceCollection services)
        {
            services
                .AddSingleton<ViewerHubConnection>()
                .AddSingleton<ViewerState>()
                .AddScoped<ViewerService>()
                .AddScoped<ViewerMessageSender>()
                .AddScoped<ViewerMessageReceiver>()
                .AddOptions<ViewerServer>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration
                        .GetSection(ViewerServer.SectionName)
                        .Bind(settings);
                })
                .Validate(x => !string.IsNullOrWhiteSpace(x.Scheme), "Scheme is required for the ViewerServer settings.")
                .Validate(x => !string.IsNullOrWhiteSpace(x.Host), "Host is required for the ViewerServer settings.")
                .Validate(x => x.Port > 0, "Port is required for the ViewerServer settings.")
                .ValidateOnStart();

            return services;
        }
    }
}
