using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using Gizmo.RemoteControl.Shared.Enums;

namespace Gizmo.RemoteControl.Viewer.Components;

public partial class Viewer : ComponentBase
{
    [Parameter] public string? Host { get; set; }
    [Parameter] public string? SessionId { get; set; }
    [Parameter] public string? AccessKey { get; set; }
    [Parameter] public string? RequesterName { get; set; }
    [Parameter] public bool? ViewOnly { get; set; }
    [Parameter] public string? Mode { get; set; }

    [Inject] IJSRuntime JsRuntime { get; set; } = null!;

    [Inject] ViewerState State { get; set; } = null!;
    [Inject] ViewerService Service { get; set; } = null!;


    private EditContext _editContext = default!;

    protected override void OnInitialized()
    {
        Service.SetError(null);
        Service.SetWarning(null);
    }

    protected override void OnParametersSet()
    {
        Service.SetParameters(Host, SessionId, AccessKey, RequesterName, ViewOnly, Mode);
        _editContext = new(State.Parameters);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            State.HasChanged -= (_, _) => InvokeAsync(StateHasChanged);
            State.HasChanged += (_, _) => InvokeAsync(StateHasChanged);

            var dotNetObjectRef = DotNetObjectReference.Create(this);

            await JsRuntime.InvokeVoidAsync("GizmoRemoteControlInternalFunctions.WatchClipboard", dotNetObjectRef);

            if (!State.Parameters.ViewOnly)
                await JsRuntime.InvokeVoidAsync("GizmoRemoteControlInternalFunctions.SubscribeEvents", dotNetObjectRef);

            ViewerService.SetJSRuntime(JsRuntime);

            var settings = await Service.GetSettings();

            Service.SetRequesterName(settings.DisplayName);

            if (State.Parameters.Mode == RemoteControlMode.Unattended && _editContext.Validate())
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
            await Service.SetSettings(new(State.Parameters.RequesterName));
            await Service.ConnectRemoteScreen(CancellationToken.None);
        }
    }

    [JSInvokable] public Task OnKeyDown(string key) => Service.OnKeyDown(key);
    [JSInvokable] public Task OnKeyUp(string key) => Service.OnKeyUp(key);
    [JSInvokable] public Task OnBlur() => Service.OnBlur();
    [JSInvokable] public Task SendClipboardText(string text, bool typeText) => Service.OnSendClipboardText(text, typeText);
}
