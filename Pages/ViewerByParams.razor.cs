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

    protected override void OnInitialized()
    {
        Server ??= "http://localhost:81";
        SessionId ??= "695468c9-93f7-47ea-8622-85495b2e04f9";
        AccessKey ??= "password";
        RequesterName ??= "Gizmo";
        ViewOnly ??= true;
        Mode ??= "Attended";
    }
}
