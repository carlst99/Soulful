using Soulful.Core.Model;
using Soulful.Core.Model.Cards;
using System;
using System.Collections.Generic;

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

        void Start(List<Pack> packs = null);
        void Stop();
    }
}
