namespace Gizmo.RemoteControl.Web.Viewer.Models
{
    public sealed class ViewerDisplays
    {
        public string? Current { get; internal set; }
        public string[] Names { get; internal set; } = [];
    }
}
