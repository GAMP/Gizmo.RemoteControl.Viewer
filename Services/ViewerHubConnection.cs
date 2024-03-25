using Gizmo.RemoteControl.Shared;
using Gizmo.RemoteControl.Shared.Helpers;
using Gizmo.RemoteControl.Shared.Models;
using Gizmo.RemoteControl.Shared.Models.Dtos;
using Gizmo.RemoteControl.Viewer.Components;
using Gizmo.RemoteControl.Viewer.Models;

using MessagePack;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Gizmo.RemoteControl.Viewer.Services
{
    public sealed class ViewerHubConnection(
        ILogger<ViewerHubConnection> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        private readonly ILogger _logger = logger;

        private HubConnection? _connection;
        private EventHandler<(string, CancellationToken)>? _onError;

        public EventHandler<string>? OnError { get; set; }
        public EventHandler<string>? OnWarning { get; set; }

        internal async Task Connect(ViewerAgent agent, CancellationToken cToken)
        {
            if (_connection is not null)
            {
                switch (_connection.State)
                {
                    case HubConnectionState.Connected:
                    case HubConnectionState.Connecting:
                        await _connection.StopAsync(cToken);
                        await _connection.DisposeAsync();
                        _connection = null;
                        break;
                }
            }

            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl($"{agent.ServerUrl}/hubs/viewer")
                    .AddMessagePackProtocol(options =>
                    {
                        options.SerializerOptions = MessagePackSerializerOptions.Standard
                            .WithResolver(MessagePack.Resolvers.StandardResolver.Instance)
                            .WithSecurity(MessagePackSecurity.TrustedData);
                    })
                    .WithAutomaticReconnect()
                    .Build();

                await using var serviceScope = serviceScopeFactory.CreateAsyncScope();

                var messageReceiver = serviceScope.ServiceProvider.GetRequiredService<ViewerMessageReceiver>();
                SubscribeReceivedMessageHandlers(_connection, messageReceiver, cToken);

                await _connection.StartAsync(cToken);

                var viewerService = serviceScope.ServiceProvider.GetRequiredService<ViewerService>();
                await StartDesktopStream(_connection, viewerService, agent.SessionId, agent.AccessKey, agent.RequesterName, cToken);

                await _connection.StopAsync(cToken);
                await _connection.DisposeAsync();

                _connection = null;

                OnError?.Invoke(this, "The session was stopped");
            }
            catch (Exception exception)
            {
                _onError?.Invoke(this, (exception.Message, cToken));
            }
        }
        public async Task Disconnect()
        {
            if (_connection is not null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public async Task Send<T>(T dto, DtoType type)
        {
            if (_connection is not null && _connection.State is HubConnectionState.Connected)
                foreach (var dtoChunk in DtoChunker.ChunkDto(dto, type))
                    await _connection.InvokeAsync("SendDtoToClient", MessagePackSerializer.Serialize(dtoChunk));
            else
                _logger.LogWarning("Unable to send dto. Connection is not established.");
        }
        public async Task Send(string endpoint, params object?[] args)
        {
            if (_connection is not null && _connection.State is HubConnectionState.Connected)
                await _connection.SendAsync(endpoint, args);
            else
                _logger.LogWarning("Unable to send message. Connection is not established.");
        }

        private void SubscribeReceivedMessageHandlers(HubConnection connection, ViewerMessageReceiver receiver, CancellationToken cToken)
        {
            _onError -= OnErrorHandler;
            _onError += OnErrorHandler;

            connection.On("PingViewer", () => "Pong");

            connection.On("ConnectionFailed", () => _onError?.Invoke(this, ("Connection failed or was denied.", cToken)));
            connection.On("ReconnectFailed", () => _onError?.Invoke(this, ("Unable to reconnect.", cToken)));
            connection.On("ConnectionRequestDenied", () => _onError?.Invoke(this, ("Connection request denied.", cToken)));
            connection.On("ViewerRemoved", () => OnWarning?.Invoke(this, "The session was stopped by your partner."));
            connection.On("ScreenCasterDisconnected", () => _onError?.Invoke(this, ("The host has disconnected.", cToken)));
            connection.On("Reconnecting", () => OnWarning?.Invoke(this, "Reconnecting..."));

            connection.On("SendDtoToViewer", async (byte[] dto) =>
            {
                try
                {
                    await receiver.OnSendDtoToViewer(dto, cToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error while handling received dto.");
                }
            });

            connection.On("ShowMessage", (string message) => receiver.OnShowMessage(message));
            connection.On("RelaunchedScreenCasterReady", (string sessionId, string accessKey) => receiver.OnRelaunchedScreenCasterReady(sessionId, accessKey));
            connection.On("CursorChange", (CursorInfo cursorInfo) =>
            {
                try
                {
                    receiver.OnCursorChange(cursorInfo);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error while updating cursor.");
                }
            });
            connection.On("WindowsSessions", (WindowsSession[] sessions) =>
            {
                try
                {
                    receiver.OnWindowsSessions(sessions);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error while updating windows sessions.");
                }
            });
        }
        private async Task StartDesktopStream(HubConnection connection, ViewerService service, string sessionId, string accessKey, string requesterName, CancellationToken cToken)
        {
            OnWarning?.Invoke(this, "Connecting to the host...");

            var result = await connection.InvokeAsync<Result>("SendScreenCastRequestToDevice", sessionId, accessKey, requesterName, cToken);

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Reason);
            
            OnWarning?.Invoke(this, string.Empty);

            const int FrameHeaderSize = 28;
            const int ChunkSize = 50_000;

            FrameReceivedDto frameReceivedDto = new();
            var frameChunksCount = 0;

            List<byte> frameBuffer = [];
            float x = default;
            float y = default;
            float width = default;
            float height = default;

            var stream = connection.StreamAsync<byte[]>("GetDesktopStream", cancellationToken: cToken);

            await foreach (var chunk in stream)
            {
                var memoryChunk = chunk.AsMemory();

                if (frameChunksCount > 0)
                {
                    frameBuffer.AddRange(memoryChunk.Span);
                    frameChunksCount--;

                    if (frameChunksCount == 0) // render Image Frame
                    {
                        await service.DrawImage([.. frameBuffer], x, y, width, height);
                    }
                }
                else // get Image Frame metadata
                {
                    if (chunk.Length < FrameHeaderSize)
                        throw new InvalidOperationException("Invalid frame header.");

                    var frameSize = BitConverter.ToInt32(memoryChunk[..4].Span);
                    frameChunksCount = (int)Math.Ceiling((double)(FrameHeaderSize + frameSize) / ChunkSize) - 1;

                    frameBuffer.Clear();
                    frameBuffer.Capacity = frameSize;
                    frameBuffer.AddRange(memoryChunk[FrameHeaderSize..].Span);

                    var header = memoryChunk[..FrameHeaderSize];

                    x = BitConverter.ToSingle(header[4..8].Span);
                    y = BitConverter.ToSingle(header[8..12].Span);
                    width = BitConverter.ToSingle(header[12..16].Span);
                    height = BitConverter.ToSingle(header[16..20].Span);

                    frameReceivedDto.Timestamp = BitConverter.ToInt64(header[20..28].Span);
                }

                _ = Send(frameReceivedDto, DtoType.FrameReceived);
            }
        }

        private async void OnErrorHandler(object? sender, (string Message, CancellationToken CToken) args)
        {
            if (_connection is not null && _connection.State == HubConnectionState.Connected)
            {
                try
                {
                    await _connection.StopAsync(args.CToken);
                    await _connection.DisposeAsync();

                    OnError?.Invoke(sender, args.Message);
                }
                catch (Exception exception)
                {
                    OnError?.Invoke(sender, exception.Message);
                }
                finally
                {
                    _connection = null;
                }
            }
            else
            {
                OnError?.Invoke(sender, args.Message);
            }
        }
    }
}
