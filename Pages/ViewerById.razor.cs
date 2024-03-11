using Microsoft.AspNetCore.Components;

namespace Gizmo.RemoteControl.Viewer.Pages;

[Route("/remotecontrol/{Id}")]
public partial class ViewerById : ComponentBase
{
    [Parameter] public string Id { get; set; } = string.Empty;

    private string? _server;
    private string? _sessionId;
    private string? _accessKey;
    private string? _requesterName;
    private bool?   _viewOnly;
    private string? _mode;
}
