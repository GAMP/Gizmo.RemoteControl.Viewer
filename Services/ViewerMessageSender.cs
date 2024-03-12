using Gizmo.RemoteControl.Shared.Enums;
using Gizmo.RemoteControl.Shared.Models.Dtos;
using Gizmo.RemoteControl.Viewer.Components;

namespace Gizmo.RemoteControl.Viewer.Services
{
    public sealed class ViewerMessageSender(ViewerState state, ViewerHubConnection connection)
    {
        private readonly ViewerState _state = state;
        private readonly ViewerHubConnection _connection = connection;

        public Task GetWindowsSessions() => _state.Parameters.Mode == RemoteControlMode.Unattended
            ? _connection.Send(new WindowsSessionsDto([]), DtoType.WindowsSessions)
            : Task.CompletedTask;
        public Task ChangeWindowsSession(int sessionId) => _state.Parameters.Mode == RemoteControlMode.Unattended
            ? _connection.Send("ChangeWindowsSession", sessionId)
            : Task.CompletedTask;
        public Task SendSelectScreen(string displayName) => _connection.Send(new SelectScreenDto
        {
            DisplayName = displayName
        }, DtoType.SelectScreen);
        public Task SendMouseMove(double percentX, double percentY) => _connection.Send(new MouseMoveDto
        {
            PercentX = percentX,
            PercentY = percentY
        }, DtoType.MouseMove);
        public Task SendMouseDown(int button, double percentX, double percentY) => _connection.Send(new MouseDownDto
        {
            Button = button,
            PercentX = percentX,
            PercentY = percentY
        }, DtoType.MouseDown);
        public Task SendMouseUp(int button, double percentX, double percentY) => _connection.Send(new MouseUpDto
        {
            Button = button,
            PercentX = percentX,
            PercentY = percentY
        }, DtoType.MouseUp);
        public Task SendTap(double percentX, double percentY) => _connection.Send(new TapDto
        {
            PercentX = percentX,
            PercentY = percentY
        }, DtoType.Tap);
        public Task SendMouseWheel(double deltaX, double deltaY) => _connection.Send(new MouseWheelDto
        {
            DeltaX = deltaX,
            DeltaY = deltaY
        }, DtoType.MouseWheel);
        public Task SendKeyDown(string key) => _connection.Send(new KeyDownDto
        {
            Key = key
        }, DtoType.KeyDown);
        public Task SendKeyUp(string key) => _connection.Send(new KeyUpDto
        {
            Key = key
        }, DtoType.KeyUp);
        public Task SendKeyPress(string key) => _connection.Send(new KeyPressDto
        {
            Key = key
        }, DtoType.KeyPress);
        public Task SendSetKeyStatesUp() => _connection.Send(new EmptyDto(), DtoType.SetKeyStatesUp);
        public async Task SendCtrlAltDel()
        {
            await _connection.Send(new CtrlAltDelDto(), DtoType.CtrlAltDel);
            await _connection.Send("InvokeCtrlAltDel");
        }
        public Task SendToggleBlockInput(bool toggleOn) => _connection.Send(new ToggleBlockInputDto
        {
            ToggleOn = toggleOn
        }, DtoType.ToggleBlockInput);
        public Task SendClipboardTransfer(string text, bool typeText) => _connection.Send(new ClipboardTransferDto
        {
            Text = text,
            TypeText = typeText
        }, DtoType.ClipboardTransfer);
    }
}
