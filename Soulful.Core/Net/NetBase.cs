using LiteNetLib;
using Soulful.Core.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soulful.Core.Net
{
    public abstract class NetBase
    {
        #region Constants

        public const int PORT = 6259;
        public const DeliveryMethod D_METHOD = DeliveryMethod.ReliableOrdered;

        #endregion

        #region Fields

        protected readonly EventBasedNetListener _listener;
        protected readonly NetManager _networker;

        /// <summary>
        /// Use this token source to cancel the networker polling task
        /// </summary>
        protected CancellationTokenSource _cancelPollToken;

        private Task _pollTask;

        #endregion

        public bool IsRunning => _networker.IsRunning;

        /// <summary>
        /// Asynchronously invoked when a game package is received
        /// </summary>
        public event EventHandler<GameKeyPackage> GameEvent;

        protected NetBase()
        {
            _listener = new EventBasedNetListener();
            _listener.NetworkReceiveEvent += OnReceive;
            _networker = new NetManager(_listener);
        }

        protected virtual void Start(int port = 0)
        {
            if (IsRunning)
                throw App.CreateError<InvalidOperationException>("[NetBase]Cannot start a net service if it is already running");

            if (port == 0)
                _networker.Start();
            else
                _networker.Start(PORT);

            _cancelPollToken = new CancellationTokenSource();
            _pollTask = new Task(() => PollEvents(_cancelPollToken.Token), TaskCreationOptions.LongRunning);
            _pollTask.Start();
        }

        public virtual void Stop()
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("[NetBase]Cannot stop a net service if it is not already running");

            // Cancel the polling
            _cancelPollToken.Cancel();
            _cancelPollToken.Dispose();

            _networker.Stop();
        }

        /// <summary>
        /// Invoked when data is received from a peer
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="reader"></param>
        /// <param name="deliveryMethod"></param>
        protected virtual void OnReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            GameKey key = (GameKey)reader.GetByte();
            GameKeyPackage package = new GameKeyPackage(key, reader, peer);
            GameEvent?.Invoke(this, package);

            reader.Recycle();
        }

        /// <summary>
        /// Polls events on the networker. When a cancellation is received, stops the networker
        /// </summary>
        private void PollEvents(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _networker.PollEvents();
                _cancelPollToken.Token.WaitHandle.WaitOne(NetHelpers.POLL_DELAY);
            }
        }
    }
}
