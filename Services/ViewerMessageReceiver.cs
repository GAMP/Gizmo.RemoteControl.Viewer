using System.Text.RegularExpressions;

using Gizmo.RemoteControl.Shared.Helpers;
using Gizmo.RemoteControl.Shared.Models;
using Gizmo.RemoteControl.Shared.Models.Dtos;
using Gizmo.RemoteControl.Web.Viewer.Pages;

using MessagePack;

namespace Gizmo.RemoteControl.Web.Viewer.Services;
public class ViewerMessageReceiver(ViewerState state, ViewerService service)
{
    private readonly ViewerState _state = state;
    private readonly ViewerService _service = service;

    public Task OnSendDtoToViewer(byte[] dto, CancellationToken cToken)
    {
        var wrapper = MessagePackSerializer.Deserialize<DtoWrapper>(dto, cancellationToken: cToken);

        return wrapper is null 
            ? throw new InvalidOperationException("Failed to deserialize dto.") 
            : HandleDtoWrapper(wrapper, cToken);
    }
    public void OnShowMessage(string message)
    {
        _state.Additional.Message = message;
        _state.HasChanged?.Invoke(this, EventArgs.Empty);
    }
    public void OnRelaunchedScreenCasterReady(string sessionId, string accessKey)
    {
        _state.Connection.SessionId = sessionId;
        _state.Connection.AccessKey = accessKey;
        _state.HasChanged?.Invoke(this, EventArgs.Empty);
    }
    public void OnCursorChange(CursorInfo cursorInfo)
    {
        CursorChange(cursorInfo);
        _state.HasChanged?.Invoke(this, EventArgs.Empty);
    }
    public void OnWindowsSessions(WindowsSession[] sessions)
    {
        UpdateWindowsSessions(sessions);
        _state.HasChanged?.Invoke(this, EventArgs.Empty);
    }

    #region DTO HANDLERS
    
    private async Task HandleDtoWrapper(DtoWrapper wrapper, CancellationToken cToken)
    {
        switch (wrapper.DtoType)
        {
            case DtoType.ClipboardText:
                await HandleClipboardText(wrapper, cToken);
                break;
            case DtoType.CursorChange:
                HandleCursorChange(wrapper);
                _state.HasChanged?.Invoke(this, EventArgs.Empty);
                break;
            case DtoType.ScreenData:
                HandleScreenData(wrapper);
                _state.HasChanged?.Invoke(this, EventArgs.Empty);
                break;
            case DtoType.ScreenSize:
                HandleScreenSize(wrapper);
                _state.HasChanged?.Invoke(this, EventArgs.Empty);
                break;
            case DtoType.WindowsSessions:
                HandleWindowsSessions(wrapper);
                _state.HasChanged?.Invoke(this, EventArgs.Empty);
                break;
            case DtoType.SessionMetrics:
                HandleSessionMetrics(wrapper);
                _state.HasChanged?.Invoke(this, EventArgs.Empty);
                break;
            default:
                throw new InvalidOperationException("Unknown dto type.");
        }
    }
    
    private Task HandleClipboardText(DtoWrapper wrapper, CancellationToken cToken) => 
        DtoChunker.TryComplete<ClipboardTextDto>(wrapper, out var dto)
            ? _service.SetClipboardText(dto.ClipboardText)
            : throw new InvalidOperationException("Failed to complete clipboard text dto.");
    private void HandleCursorChange(DtoWrapper wrapper)
    {
        if (DtoChunker.TryComplete<CursorChangeDto>(wrapper, out var dto))
            CursorChange(new(dto.ImageBytes, new(dto.HotSpotX, dto.HotSpotY), dto.CssOverride));
        else
            throw new InvalidOperationException("Failed to complete cursor change dto.");
    }
    private void HandleScreenData(DtoWrapper wrapper)
    {
        if (DtoChunker.TryComplete<ScreenDataDto>(wrapper, out var dto))
        {
            _state.Canvas.Width = dto.ScreenWidth;
            _state.Canvas.Height = dto.ScreenHeight;
            _state.Canvas.Attributes["width"] = dto.ScreenWidth;
            _state.Canvas.Attributes["height"] = dto.ScreenHeight;

            _state.Additional.MachineName = dto.MachineName;
            _state.Displays.Current = dto.SelectedDisplay;
            _state.Displays.Names = dto.DisplayNames.ToArray();
        }
        else
            throw new InvalidOperationException("Failed to complete screen data dto.");
    }
    private void HandleScreenSize(DtoWrapper wrapper)
    {
        if (DtoChunker.TryComplete<ScreenSizeDto>(wrapper, out var dto))
        {
            _state.Canvas.Width = dto.Width;
            _state.Canvas.Height = dto.Height;
            _state.Canvas.Attributes["width"] = dto.Width;
            _state.Canvas.Attributes["height"] = dto.Height;
        }
        else
            throw new InvalidOperationException("Failed to complete screen size dto.");
    }
    private void HandleSessionMetrics(DtoWrapper wrapper)
    {
        if (DtoChunker.TryComplete<SessionMetricsDto>(wrapper, out var dto))
        {
            _state.Metrics.Fps = dto.Fps;
            _state.Metrics.Mbps = dto.Mbps;
            _state.Metrics.Latency = dto.RoundTripLatency;
            _state.Metrics.IsGpu = dto.IsGpuAccelerated;
        }
        else
            throw new InvalidOperationException("Failed to complete session metrics dto.");
    }
    private void HandleWindowsSessions(DtoWrapper wrapper)
    {
        if (DtoChunker.TryComplete<WindowsSessionsDto>(wrapper, out var dto))
            UpdateWindowsSessions([.. dto.WindowsSessions]);
        else
            throw new InvalidOperationException("Failed to complete windows sessions dto.");
    }
    
    #endregion

    private void CursorChange(CursorInfo cursorInfo)
    {
        string canvasCursorStyle;

        if (!string.IsNullOrWhiteSpace(cursorInfo.CssOverride))
        {
            canvasCursorStyle = $" cursor: {cursorInfo.CssOverride};";
        }
        else if (cursorInfo.ImageBytes.Length == 0)
        {
            canvasCursorStyle = " cursor: default;";
        }
        else
        {
            var base64 = Convert.ToBase64String(cursorInfo.ImageBytes);
            canvasCursorStyle = $" cursor: url('data:image/png;base64,{base64}') {cursorInfo.HotSpot.X} {cursorInfo.HotSpot.Y}, default;";
        }

        var canvasStyle = _state.Canvas.Attributes["style"] as string;

        if (canvasStyle!.IndexOf("cursor:", StringComparison.OrdinalIgnoreCase) > -1)
        {
            _state.Canvas.Attributes["style"] = Regex.Replace(canvasStyle, @"cursor:.*?;", canvasCursorStyle);
        }
        else
        {
            _state.Canvas.Attributes["style"] = canvasStyle + canvasCursorStyle;
        }
    }
    private void UpdateWindowsSessions(WindowsSession[] sessions)
    {
        _state.Sessions.Clear();

        foreach (var session in sessions)
        {
            _state.Sessions.Add(new()
            {
                Id = session.Id,
                Type = session.Type.ToString(),
                Name = session.Name,
                UserName = session.Username
            });
        }
    }
}
