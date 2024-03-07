namespace Gizmo.RemoteControl.Viewer.Models.Settings
{
    public sealed class ViewerServer
    {
        public const string SectionName = "RemoteControlServer";

        public string Scheme { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
    }
}
