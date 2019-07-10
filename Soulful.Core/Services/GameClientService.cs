using LiteNetLib;
using LiteNetLib.Utils;
using Serilog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Soulful.Core.Services
{
    public sealed class GameClientService : IGameClientService
    {
        private readonly EventBasedNetListener _listener;
        private readonly NetManager _client;
        private Task _pollTask;
        private CancellationTokenSource _cancelPollToken;

        public bool IsRunning => _client.IsRunning;
        public string Pin { get; private set; }
        public string UserName { get; private set; }

        /// <summary>
        /// Invoked when the client connects to a server
        /// </summary>
        public event EventHandler ConnectedToServer;

        public GameClientService()
        {
            _listener = new EventBasedNetListener();
            _listener.NetworkReceiveUnconnectedEvent += OnReceiveUnconnected;

            _client = new NetManager(_listener);
        }

        public void Start(string pin, string userName)
        {
            if (_client.IsRunning)
                throw App.CreateError<InvalidOperationException>("Client is already running");

            _cancelPollToken = new CancellationTokenSource();
            Pin = pin;
            UserName = userName;

            _client.Start();
            _pollTask = Task.Run(async () =>
            {
                while (!_cancelPollToken.IsCancellationRequested)
                {
                    _client.PollEvents();
                    await Task.Delay(15).ConfigureAwait(false);
                }
            }, _cancelPollToken.Token);

            NetDataWriter writer = new NetDataWriter();
            writer.Put(pin);
            _client.SendDiscoveryRequest(writer, NetConstants.PORT);

            Log.Information("Client started");
            Log.Information("Client attempting to discover server with pin {pin}", Pin);
        }

        public void Stop()
        {
            if (!_client.IsRunning)
                throw App.CreateError<InvalidOperationException>("Client is not running");

            _cancelPollToken.Cancel();
            _pollTask.Wait();
            _client.Stop(true);

            Log.Information("Client stopped");
        }

        private void OnReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryResponse)
            {
                NetDataWriter writer = new NetDataWriter();
                writer.Put(Pin);
                writer.Put(UserName);
                _client.Connect(remoteEndPoint, writer);
                ConnectedToServer?.Invoke(this, EventArgs.Empty);
                Log.Information("Client attempting to connect to server at {endPoint}", remoteEndPoint);
            }
            reader.Recycle();
        }
    }
}
