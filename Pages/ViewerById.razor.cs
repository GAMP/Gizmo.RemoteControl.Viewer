using Gizmo.RemoteControl.Viewer.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Gizmo.RemoteControl.Viewer.Pages;

[Authorize, Route("/remotecontrol/hosts/{Id:int}")]
public partial class ViewerById : ComponentBase
{
    [Parameter] public int Id { get; set; }
    [Inject] public IRemoteControlViewerSessionService RemoteControlViewerSessionService { get; set; } = default!;


    private string _serverUrl = null!;
    private string _sessionId = null!;
    private string _accessKey = null!;
    private string _requesterName = null!;
    private bool _viewOnly;
    private string _mode = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var session = await RemoteControlViewerSessionService.CreateSessionAsync(Id);

            _serverUrl = session.ServerUrl;
            _sessionId = session.Id.ToString();
            _accessKey = session.Password;

            _requesterName = "Gizmo";
            _viewOnly = true;
            _mode = "Attended";

            StateHasChanged();
        }
    }
}


