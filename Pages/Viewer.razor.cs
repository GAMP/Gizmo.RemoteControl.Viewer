using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using Gizmo.RemoteControl.Shared.Enums;

namespace Gizmo.RemoteControl.Viewer.Pages;

[Route("/remotecontrol/viewer")]
public partial class Viewer : ComponentBase
{
    [Parameter, SupplyParameterFromQuery] public string? SessionId { get; set; }
    [Parameter, SupplyParameterFromQuery] public string? AccessKey { get; set; }
    [Parameter, SupplyParameterFromQuery] public string? RequesterName { get; set; }
    [Parameter, SupplyParameterFromQuery] public bool? ViewOnly { get; set; }
    [Parameter, SupplyParameterFromQuery] public string? Mode { get; set; }

    [Inject] ViewerState State { get; set; } = null!;
    [Inject] ViewerService Service { get; set; } = null!;

    [Inject] IJSRuntime JsRuntime { get; set; } = null!;


    private EditContext _editContext = default!;

    protected override void OnInitialized()
    {
        Service.SetConnectionData(SessionId, AccessKey, RequesterName, ViewOnly, Mode);

        _editContext = new(State.Connection);
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            State.HasChanged -= (_, _) => InvokeAsync(StateHasChanged);
            State.HasChanged += (_, _) => InvokeAsync(StateHasChanged);

            var dotNetObjectRef = DotNetObjectReference.Create(this);

            await JsRuntime.InvokeVoidAsync("InternalFunctions.WatchClipboard", dotNetObjectRef);

            if (!State.Connection.ViewOnly)
                await JsRuntime.InvokeVoidAsync("InternalFunctions.SubscribeEvents", dotNetObjectRef);

            ViewerService.SetJSRuntime(JsRuntime);

            var settings = await Service.GetSettings();

            if (string.IsNullOrEmpty(State.Connection.RequesterName) && !string.IsNullOrWhiteSpace(settings.DisplayName))
                Service.SetConnectionData(null, null, settings.DisplayName, null, null);

            if (State.Connection.Mode == RemoteControlMode.Unattended && _editContext.Validate())
            {
                await Service.ConnectRemoteScreen(CancellationToken.None);
            }
        }
    }

    private async Task ShowRemoteScreen()
    {
        Service.SetError(null);
        Service.SetWarning(null);
        Service.SetConnectionMode(RemoteControlMode.Attended);

        if (_editContext.Validate())
        {
            await Service.SetSettings(new(State.Connection.RequesterName));
            await Service.ConnectRemoteScreen(CancellationToken.None);
        }
    }

    [JSInvokable] public Task OnKeyDown(string key) => Service.OnKeyDown(key);
    [JSInvokable] public Task OnKeyUp(string key) => Service.OnKeyUp(key);
    [JSInvokable] public Task OnBlur() => Service.OnBlur();
    [JSInvokable] public Task SendClipboardText(string text, bool typeText) => Service.OnSendClipboardText(text, typeText);
}
