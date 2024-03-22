using Gizmo.RemoteControl.Viewer.Components;
using Gizmo.RemoteControl.Viewer.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Gizmo.RemoteControl.Viewer
{
    public static class Registrations
    {
        public static IServiceCollection AddRemoteControlViewer<T>(this IServiceCollection services)
            where T : class, IRemoteControlViewerService
                => services
                    .AddSingleton<ViewerHubConnection>()
                    .AddSingleton<ViewerState>()
                    .AddScoped<ViewerService>()
                    .AddScoped<ViewerMessageSender>()
                    .AddScoped<ViewerMessageReceiver>()
                    .AddScoped<HttpClient>()
                    .AddScoped<IRemoteControlViewerService, T>();
    }
}
