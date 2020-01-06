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
        public const DeliveryMethod D_METHOD = DeliveryMethod.ReliableOrdered;

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
        private static readonly object _networkerLock = new object();

        #endregion

        public bool IsRunning => _networker.IsRunning;

        public event EventHandler<GameKeyPackage> GameEvent;

        protected NetBase()
        {
            _listener = new EventBasedNetListener();
            _listener.NetworkReceiveEvent += OnReceive;
            _networker = new NetManager(_listener);
        }

        public virtual void Start(int port = 0)
        {
            if (IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is already running");

            if (port == 0)
                RunNetworkerTask(() => _networker.Start());
            else
                RunNetworkerTask(() => _networker.Start(PORT));

            _cancelPollToken = new CancellationTokenSource();
            new Task(PollEvents, TaskCreationOptions.LongRunning).Start();
        }

        public virtual void Stop()
        {
            if (!IsRunning)
                throw App.CreateError<InvalidOperationException>("Server is not running");

            _cancelPollToken.Cancel();
            RunNetworkerTask(() => _networker.Stop());
        }

        #region RunNetworkerTask

        /// <summary>
        /// Invokes a networker action in a thread-safe manner
        /// </summary>
        /// <param name="action"></param>
        protected static void RunNetworkerTask(Action action)
        {
            lock (_networkerLock)
                action.Invoke();
        }

        /// <summary>
        /// Invokes a networker action in a thread-safe manner
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        protected static T RunNetworkerTask<T>(Func<T> action)
        {
            T toReturn;

            lock (_networkerLock)
                toReturn = action.Invoke();
            return toReturn;
        }

        #endregion

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
        /// Polls events on the networker
        /// </summary>
        private void PollEvents()
        {
            while (!_cancelPollToken.IsCancellationRequested && _networker.IsRunning)
            {
                RunNetworkerTask(() => _networker.PollEvents());
                _cancelPollToken.Token.WaitHandle.WaitOne(NetHelpers.POLL_DELAY);
            }
        }
    }
}
