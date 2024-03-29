﻿using System.ComponentModel.DataAnnotations;

using Gizmo.RemoteControl.Shared.Enums;

namespace Gizmo.RemoteControl.Viewer.Models
{
    public sealed class ViewerParameters
    {
        [Required(ErrorMessage = "is required.")]
        public string Host { get; internal set; } = string.Empty;
        
        [Required(ErrorMessage = "is required.")]
        public string AccessKey { get; internal set; } = string.Empty;

        [Required(ErrorMessage = "is required.")]
        public string SessionId { get; internal set; } = string.Empty;

        [Required(ErrorMessage = "is required.")]
        public string RequesterName { get; internal set; } = string.Empty;

        public bool ViewOnly { get; internal set; } = false;
        public RemoteControlMode Mode { get; internal set; } = RemoteControlMode.Attended;
    }
}
