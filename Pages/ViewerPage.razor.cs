using Gizmo.RemoteControl.Viewer.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Gizmo.RemoteControl.Viewer.Pages;

[Authorize, Route("/remotecontrol/hosts/{Id:int}")]
public partial class ViewerPage : ComponentBase
{
    [Parameter] public int Id { get; set; }
    [Inject] public IRemoteControlViewerService Service { get; set; } = default!;


    private string? _host;
    private string? _sessionId;
    private string? _accessKey;
    private string? _requesterName;
    private bool _viewOnly;
    private string? _mode;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var session = await Service.CreateSessionAsync(Id);

            _host = session.Host;
            _sessionId = session.Id.ToString();
            _accessKey = session.AccessKey;

            _requesterName = "Gizmo";
            _viewOnly = true;
            _mode = "Attended";

            StateHasChanged();
        }
    }
}


