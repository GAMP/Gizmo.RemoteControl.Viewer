namespace Gizmo.RemoteControl.Viewer.Models
{
    internal sealed record ViewerAgent(string ServerUrl, string SessionId, string AccessKey, string RequesterName);
}
