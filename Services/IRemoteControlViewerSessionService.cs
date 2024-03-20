namespace Gizmo.RemoteControl.Viewer.Services
{
    /// <summary>
    /// Represents a remote control viewer session.
    /// </summary>
    public sealed record RemoteControlViewerSession
    {
        /// <summary>
        /// Gets created remote session id.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets create session id password.
        /// </summary>
        public string Password { get; init; } = null!;

        /// <summary>
        /// Gets the server url of the remote control host.
        /// </summary>
        public string ServerUrl { get; init; } = null!;
    }   

    /// <summary>
    /// Represents a remote control viewer session service.
    /// </summary>
    public interface IRemoteControlViewerSessionService
    {
        /// <summary>
        /// Creates a remote control session.
        /// <paramref name="hostId"/>Host id.</param>
        /// <paramref name="cToken"/>Cancellation token.</param>
        /// <returns>Remote control session.</returns>
        /// </summary>
        Task<RemoteControlViewerSession> CreateSessionAsync(int hostId, CancellationToken cToken = default);
    }
}
