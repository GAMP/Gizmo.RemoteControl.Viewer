using System.Net.Http.Json;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace Gizmo.RemoteControl.Viewer.Pages;

public sealed record AccessTokenValue(string Token, string RefreshToken);
public sealed record AccessToken(AccessTokenValue Result, string? Version, int HttpStatusCode, bool isError, string? Message);
public sealed record SessionCreateResult(Guid SessionId, string SessionPassword);
public sealed record RemoteControlSession(SessionCreateResult Result, string? Version, int HttpStatusCode, bool isError, string? Message);


[Authorize, Route("/remotecontrol/hosts/{Id:int}")]
public partial class ViewerById : ComponentBase
{
    [Parameter] public int Id { get; set; }

    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

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
            _serverUrl = "http://localhost:81";

            var token = HttpContextAccessor.HttpContext.Request.Cookies["_BASE_AUTH_COOKIE"];

            HttpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

            var session = await HttpClient.GetFromJsonAsync<RemoteControlSession>($"{_serverUrl}/api/v2/remotecontrol/hosts/{Id}/session");

            _sessionId = session?.Result.SessionId.ToString() ?? string.Empty;
            _accessKey = session?.Result.SessionPassword ?? string.Empty;
            _requesterName = "Gizmo";
            _viewOnly = true;
            _mode = "Attended";

            StateHasChanged();
        }
    }
}


