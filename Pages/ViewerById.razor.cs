using System.Net.Http.Json;

using Microsoft.AspNetCore.Components;

namespace Gizmo.RemoteControl.Viewer.Pages;

public sealed record AccessTokenValue(string Token, string RefreshToken);
public sealed record AccessToken(AccessTokenValue Result, string? Version, int HttpStatusCode, bool isError, string? Message);
public sealed record SessionCreateResult(Guid SessionId, string SessionPassword);
public sealed record RemoteControlSession(SessionCreateResult Result, string? Version, int HttpStatusCode, bool isError, string? Message);


[Route("/remotecontrol/{Id:int}")]
public partial class ViewerById : ComponentBase
{
    [Parameter] public int Id { get; set; }

    [Inject] public HttpClient HttpClient { get; set; } = default!;

    private string? _serverUrl;
    private string? _sessionId;
    private string? _accessKey;
    private string? _requesterName;
    private bool? _viewOnly;
    private string? _mode;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _serverUrl = "http://localhost:81";

            var token = await HttpClient.GetFromJsonAsync<AccessToken>($"{_serverUrl}/api/v2/auth/accesstoken?userName=admin&password=admin");

            HttpClient.DefaultRequestHeaders.Authorization = new("Bearer", token?.Result.Token);

            var session = await HttpClient.GetFromJsonAsync<RemoteControlSession>($"{_serverUrl}/api/v2/remotecontrol/{Id}/session");

            _sessionId = session?.Result.SessionId.ToString();
            _accessKey = session?.Result.SessionPassword;
            _requesterName = "Gizmo";
            _viewOnly = true;
            _mode = "Attended";
        }
    }
}


