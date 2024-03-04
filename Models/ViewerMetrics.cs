namespace Gizmo.RemoteControl.Viewer.Models
{
    public sealed class ViewerMetrics
    {
        public double Fps { get; internal set; }
        public double Mbps { get; internal set; }
        public double Latency { get; internal set; }
        public bool IsGpu { get; internal set; }
    }
}
