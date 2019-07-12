using LiteNetLib;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soulful.Core.Net
{
    public abstract class NetBase
    {
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

        public bool IsRunning => _networker.IsRunning;

        protected NetBase()
        {
            _networkerLock = new object();

            _listener = new EventBasedNetListener();
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

        /// <summary>
        /// Polls events on the networker
        /// </summary>
        private void PollEvents()
        {
            while (!_cancelPollToken.IsCancellationRequested)
            {
                RunNetworkerTask(() => _networker?.PollEvents());
                _cancelPollToken.Token.WaitHandle.WaitOne(NetConstants.POLL_DELAY);
            }
        }
    }
}
