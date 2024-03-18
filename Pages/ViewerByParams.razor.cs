using Microsoft.AspNetCore.Components;

namespace Gizmo.RemoteControl.Viewer.Pages;

[Route("/remotecontrol")]
public partial class ViewerByParams : ComponentBase
{
    [SupplyParameterFromQuery] public string? Server { get; set; }
    [SupplyParameterFromQuery] public string? SessionId { get; set; }
    [SupplyParameterFromQuery] public string? AccessKey { get; set; }
    [SupplyParameterFromQuery] public string? RequesterName { get; set; }
    [SupplyParameterFromQuery] public bool? ViewOnly { get; set; }
    [SupplyParameterFromQuery] public string? Mode { get; set; }

    private string _serverUrl = null!;
    private string _sessionId = null!;
    private string _accessKey = null!;
    private string _requesterName = null!;
    private bool _viewOnly;
    private string _mode = null!;

    protected override void OnInitialized()
    {
        _serverUrl = Server ?? string.Empty;
        _sessionId = SessionId ?? string.Empty;
        _accessKey = AccessKey ?? string.Empty;
        _requesterName = RequesterName ?? string.Empty;
        _viewOnly = ViewOnly ?? true;
        _mode = Mode ?? string.Empty;
    }
}
