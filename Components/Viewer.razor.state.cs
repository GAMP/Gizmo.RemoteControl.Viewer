using Gizmo.RemoteControl.Viewer.Models;

namespace Gizmo.RemoteControl.Viewer.Components
{
    public sealed class ViewerState
    {
        public EventHandler? HasChanged { get; set; }

        public ViewerCanvas Canvas { get; internal set; } = new();

        public ViewerRequestParameters RequestParameters { get; internal set; } = new();
        public ViewerMetrics Metrics { get; internal set; } = new();
        public ViewerSettings? Settings { get; internal set; }
        public List<ViewerSession> Sessions { get; internal set; } = [];
        public ViewerDisplays Displays { get; internal set; } = new();
        public ViewerError Error { get; internal set; } = new();
        public ViewerAdditional Additional { get; internal set; } = new();

        public void Reset()
        {
            Canvas = new()
            {
                Element = Canvas.Element,
            };
            Metrics = new();
            Settings = null;
            Sessions = [];
            Displays = new();
            Additional = new();
        }
    }
}
