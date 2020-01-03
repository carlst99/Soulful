using Soulful.Core.Model;
using Soulful.Core.Model.CardDb;
using System;
using System.Linq;

namespace Soulful.Core.Services
{
    public interface IGameService
    {
        /// <summary>
        /// Gets a value indicating the state of this <see cref="GameService"/>
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Invoked when a game event occurs
        /// </summary>
        /// <remarks>This event should match any client side game events</remarks>
        event EventHandler<GameKeyPackage> GameEvent;

        /// <summary>
        /// Invoked when the game is stopped
        /// </summary>
        event EventHandler GameStopped;

        void Start();
        void Start(IQueryable<Pack> packKeys);
        void Stop();
    }
}
