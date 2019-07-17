using LiteNetLib;
using Soulful.Core.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soulful.Core.Net
{
    public abstract class NetBase : INetBase
    {
        public const int PORT = 6259;

        #region Fields

        protected readonly EventBasedNetListener _listener;
        protected readonly NetManager _networker;

        /// <summary>
        /// Use this token source to cancel the networker polling task
        /// </summary>
        protected CancellationTokenSource _cancelPollToken;

        /// <summary>
        /// A lock should be taken on this object when a task involving the <see cref="_networker"/> is required
        /// </summary>
        protected readonly object _networkerLock;

        #endregion

        public bool IsRunning => _networker.IsRunning;

        public event EventHandler<GameKeyPackage> GameEvent;

        protected NetBase()
        {
            _networkerLock = new object();

            _listener = new EventBasedNetListener();
            _listener.NetworkReceiveEvent += OnReceive;
            _networker = new NetManager(_listener);
        }

        public virtual void Start()
        {
            if (IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is already running");

            _cancelPollToken = new CancellationTokenSource();
            new Task(PollEvents, _cancelPollToken.Token, TaskCreationOptions.LongRunning).Start();
        }

        public virtual void Stop()
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is not running");

            _cancelPollToken.Cancel();
            RunNetworkerTask(() => _networker.Stop());
        }

        #region RunNetworkerTask

        protected void RunNetworkerTask(Action action)
        {
            lock (_networkerLock)
                action.Invoke();
        }

        protected T RunNetworkerTask<T>(Func<T> action)
        {
            T toReturn;

            lock (_networkerLock)
                toReturn = action.Invoke();
            return toReturn;
        }

        #endregion

        protected virtual void OnReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            GameKey key = (GameKey)reader.GetByte();
            GameKeyPackage package = new GameKeyPackage(key, reader, peer);
            GameEvent?.Invoke(this, package);

            reader.Recycle();
        }

        /// <summary>
        /// Polls events on the networker
        /// </summary>
        private void PollEvents()
        {
            while (!_cancelPollToken.IsCancellationRequested)
            {
                RunNetworkerTask(() => _networker?.PollEvents());
                _cancelPollToken.Token.WaitHandle.WaitOne(NetHelpers.POLL_DELAY);
            }
        }
    }
}
