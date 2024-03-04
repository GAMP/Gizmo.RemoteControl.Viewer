﻿using Gizmo.RemoteControl.Web.Viewer.Models;

namespace Gizmo.RemoteControl.Web.Viewer.Pages
{
    public sealed class ViewerState
    {
        public EventHandler? HasChanged { get; set; }

        public ViewerCanvas Canvas { get; internal set; } = new();

        public ViewerConnection Connection { get; internal set; } = new();
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
