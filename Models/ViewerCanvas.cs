using Microsoft.AspNetCore.Components;

namespace Gizmo.RemoteControl.Web.Viewer.Models
{
    public sealed class ViewerCanvas
    {
        public ElementReference Element { get; set; }
        public Dictionary<string, object> Attributes { get; init; } = new(4)
        {
            {"style" , "width: 100%; border-radius: 10px;"},
        };
        public int Width { get; internal set; } = 800;
        public int Height { get; internal set; } = 600;
    }
}
