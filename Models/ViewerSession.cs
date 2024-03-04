namespace Gizmo.RemoteControl.Viewer.Models
{
    public sealed class ViewerSession
    {
        public uint Id { get; internal set; }
        public string Type { get; internal set; } = string.Empty;
        public string Name { get; internal set; } = string.Empty;
        public string UserName { get; internal set; } = string.Empty;
    }
}
