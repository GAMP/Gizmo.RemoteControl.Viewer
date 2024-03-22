using System.Text.Json;
using Gizmo.RemoteControl.Shared.Enums;
using Gizmo.RemoteControl.Viewer.Models;
using Gizmo.RemoteControl.Viewer.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace Gizmo.RemoteControl.Viewer.Components
{
    public sealed class ViewerService : IDisposable
    {
        const string SettingKey = "Viewer_Settings";

        private static IJSRuntime? JsRuntime;

        private readonly ViewerState _state;
        private readonly ViewerMessageSender _sender;
        private readonly ViewerHubConnection _connection;
        private readonly NavigationManager _navigationManager;

        public ViewerService(
            ViewerState state,
            ViewerMessageSender sender,
            ViewerHubConnection connection,
            NavigationManager navigationManager)
        {
            _state = state;
            _sender = sender;
            _connection = connection;
            _navigationManager = navigationManager;

            _connection.OnError += OnErrorHandler;
            _connection.OnWarning += OnWarningHandler;
        }

        public static void SetJSRuntime(IJSRuntime jsRuntime) => JsRuntime = jsRuntime;

        public void SetParameters(string? host, string? sessionId, string? accessKey, string? requesterName, bool? viewOnly, string? modeStr)
        {
            _state.Parameters.Host = host ?? string.Empty;
            _state.Parameters.SessionId = sessionId ?? string.Empty;
            _state.Parameters.AccessKey = accessKey ?? string.Empty;
            _state.Parameters.RequesterName = requesterName ?? string.Empty;
            _state.Parameters.ViewOnly = viewOnly ?? true;
            
            if(Enum.TryParse<RemoteControlMode>(modeStr, out var mode))
                _state.Parameters.Mode =  mode;
        }

        public void SetRequesterName(string? displayName)
        {
            if (string.IsNullOrEmpty(_state.Parameters.RequesterName) && !string.IsNullOrWhiteSpace(displayName))
                _state.Parameters.RequesterName = displayName;
        }
        public void SetConnectionMode(RemoteControlMode mode)
        {
            _state.Parameters.Mode = mode;
        }
        public void SetError(string? error)
        {
            _state.Error.Message = error;
        }
        public void SetWarning(string? warning)
        {
            _state.Additional.Warning = warning;
        }

        public async Task<ViewerSettings> GetSettings()
        {
            if (_state.Settings is not null)
                return _state.Settings;

            if (JsRuntime is null)
                throw new InvalidOperationException("JSRuntime is not set. Use this method from OnAfterRenderAsync.");

            var settingValue = await JsRuntime.InvokeAsync<string>("localStorage.getItem", SettingKey);

            return string.IsNullOrEmpty(settingValue)
                ? new ViewerSettings(null)
                : JsonSerializer.Deserialize<ViewerSettings>(settingValue)
                    ?? throw new InvalidOperationException("Failed to deserialize viewer settings.");
        }
        public async Task SetSettings(ViewerSettings settings)
        {
            _state.Settings = settings;

            if (JsRuntime is null)
                throw new InvalidOperationException("JSRuntime is not set. Use this method from OnAfterRenderAsync.");

            var settingValue = JsonSerializer.Serialize(settings);
            await JsRuntime.InvokeVoidAsync("localStorage.setItem", SettingKey, settingValue);
        }

        public async Task CopyInviteLinkToClipboard()
        {
            var inviteLink = _state.Parameters.Mode == RemoteControlMode.Attended
                ? $"{_navigationManager.Uri}?sessionId={_state.Parameters.SessionId}"
                : $"{_navigationManager.Uri}?mode=Unattended&sessionId={_state.Parameters.SessionId}&accessKey={_state.Parameters.AccessKey}";

            await SetClipboardText(inviteLink);

            SetWarning("Invite link copied to clipboard");
        }
        public async Task ToggleFullScreen()
        {
            if (JsRuntime is null)
                throw new InvalidOperationException("JSRuntime is not set. Use this method from OnAfterRenderAsync.");

            await JsRuntime.InvokeVoidAsync("GizmoRemoteControlInternalFunctions.ToggleFullScreen", _state.Canvas.Element);
        }
        public async Task SetClipboardText(string text)
        {
            if (JsRuntime is null)
                throw new InvalidOperationException("JSRuntime is not set. Use this method from OnAfterRenderAsync.");

            await JsRuntime.InvokeVoidAsync("GizmoRemoteControlInternalFunctions.SetClipboardText", text);
        }

        public Task ConnectRemoteScreen(CancellationToken cToken)
        {
                var agent = new ViewerAgent(_state.Parameters.Host, _state.Parameters.SessionId, _state.Parameters.AccessKey, _state.Parameters.RequesterName);
                return _connection.Connect(agent, cToken);
        }
        public Task DisconnectRemoteScreen() => _connection.Disconnect();

        public async Task DrawImage(byte[] image, float x, float y, float width, float height)
        {
            if (JsRuntime is null)
                throw new InvalidOperationException("JSRuntime is not set. Use this method from OnAfterRenderAsync.");

            await JsRuntime.InvokeVoidAsync("GizmoRemoteControlInternalFunctions.DrawImage", _state.Canvas.Element, image, x, y, width, height);
        }

        public Task OnDisplayChange(ChangeEventArgs args)
        {
            var displayName = args.Value?.ToString();

            return displayName is not null
                ? _sender.SendSelectScreen(displayName)
                : Task.CompletedTask;
        }
        public Task OnWindowsSessionChange(ChangeEventArgs args)
        {
            var sessionId = args.Value?.ToString();

            return sessionId is not null
                ? _sender.ChangeWindowsSession(int.Parse(sessionId))
                : Task.CompletedTask;
        }

        public Task OnMouseMove(MouseEventArgs args)
        {
            if (!_state.Parameters.ViewOnly && JsRuntime is not null)
            {
                var x = args.OffsetX / Convert.ToDouble(_state.Canvas.Width);
                var y = args.OffsetY / Convert.ToDouble(_state.Canvas.Height);
                return _sender.SendMouseMove(x, y);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        public Task OnMouseDown(MouseEventArgs args)
        {
            if (!_state.Parameters.ViewOnly && JsRuntime is not null)
            {
                var x = args.OffsetX / Convert.ToDouble(_state.Canvas.Width);
                var y = args.OffsetY / Convert.ToDouble(_state.Canvas.Height);
                return _sender.SendMouseDown((int)args.Button, x, y);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        public Task OnMouseUp(MouseEventArgs args)
        {
            if (!_state.Parameters.ViewOnly && JsRuntime is not null)
            {
                var x = args.OffsetX / Convert.ToDouble(_state.Canvas.Width);
                var y = args.OffsetY / Convert.ToDouble(_state.Canvas.Height);
                return _sender.SendMouseUp((int)args.Button, x, y);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        public Task OnMouseClick(MouseEventArgs args)
        {
            if (!_state.Parameters.ViewOnly && JsRuntime is not null)
            {
                var x = args.OffsetX / Convert.ToDouble(_state.Canvas.Width);
                var y = args.OffsetY / Convert.ToDouble(_state.Canvas.Height);
                return _sender.SendTap(x, y);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        public Task OnMouseWheel(WheelEventArgs args) => !_state.Parameters.ViewOnly && JsRuntime is not null
            ? _sender.SendMouseWheel(args.DeltaX, args.OffsetY)
            : Task.CompletedTask;
        public Task OnKeyDown(string key) => !_state.Parameters.ViewOnly && JsRuntime is not null
            ? _sender.SendKeyDown(key)
            : Task.CompletedTask;
        public Task OnKeyUp(string key) => !_state.Parameters.ViewOnly && JsRuntime is not null
            ? _sender.SendKeyUp(key)
            : Task.CompletedTask;
        public Task OnBlur() => !_state.Parameters.ViewOnly && JsRuntime is not null
            ? _sender.SendSetKeyStatesUp()
            : Task.CompletedTask;
        public Task OnSendClipboardText(string text, bool typeText) =>
            _sender.SendClipboardTransfer(text, typeText);

        private void OnErrorHandler(object? _, string error)
        {
            SetError(error);
            _state.Reset();

            _state.HasChanged?.Invoke(this, EventArgs.Empty);
        }
        private void OnWarningHandler(object? _, string warning)
        {
            SetWarning(warning);
            _state.HasChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _connection.OnError -= OnErrorHandler;
            _connection.OnWarning -= OnWarningHandler;
        }
    }
}
