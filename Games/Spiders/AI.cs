// This is where you build your AI for the Spiders game.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Spiders
{
    /// <summary>
    /// This is where you build your AI for the Spiders game.
    /// </summary>
    class AI : BaseAI
    {
        #region Properties
        #pragma warning disable 0169 // the never assigned warnings between here are incorrect. We set it for you via reflection. So these will remove it from the Error List.
        #pragma warning disable 0649
        /// <summary>
        /// This is the Game object itself, it contains all the information about the current game
        /// </summary>
        public readonly Spiders.Game Game;
        /// <summary>
        /// This is your AI's player. This AI class is not a player, but it should command this Player.
        /// </summary>
        public readonly Spiders.Player Player;
        #pragma warning restore 0169
        #pragma warning restore 0649

        private Random Random = new Random();

        #endregion


        #region Methods
        /// <summary>
        /// This returns your AI's name to the game server. Just replace the string.
        /// </summary>
        /// <returns>string of you AI's name.</returns>
        public override string GetName()
        {
            return "¯\\¯\\¯\\¯\\_(ツ)_/¯/¯/¯/¯"; // REPLACE THIS WITH YOUR TEAM NAME!
        }

        /// <summary>
        /// This is automatically called when the game first starts, once the Game object and all GameObjects have been initialized, but before any players do anything.
        /// </summary>
        /// <remarks>
        /// This is a good place to initialize any variables you add to your AI, or start tracking game objects.
        /// </remarks>
        public override void Start()
        {
			API.Init(Game, Player);
            base.Start();
        }

        /// <summary>
        /// This is automatically called every time the game (or anything in it) updates.
        /// </summary>
        /// <remarks>
        /// If a function you call triggers an update this will be called before that function returns.
        /// </remarks>
        public override void GameUpdated()
        {
            base.GameUpdated();
        }

        /// <summary>
        /// This is automatically called when the game ends.
        /// </summary>
        /// <remarks>
        /// You can do any cleanup of you AI here, or do custom logging. After this function returns the application will close.
        /// </remarks>
        /// <param name="won">true if your player won, false otherwise</param>
        /// <param name="reason">a string explaining why you won or lost</param>
        public override void Ended(bool won, string reason)
        {
            base.Ended(won, reason);
        }


        /// <summary>
        /// This is called every time it is this AI.player's turn.
        /// </summary>
        /// <returns>Represents if you want to end your turn. True means end your turn, False means to keep your turn going and re-call this function.</returns>
        public bool RunTurn()
        {
            try
            {
                Console.WriteLine(Game.CurrentTurn);

                // Spawn
                API.Refresh();

                var state = API.State;
                var broodMother = state.Spiders[state.Players[state.CurrentPlayer].BroodMother];
                for (int i = 0; i < Math.Min(broodMother.Eggs, 5); i++)
                {
                    API.Execute(new XAction(broodMother, XActionType.Spawn) { SpawnType = XSpiderType.Spitter });
                }

                // Mobilize
                API.Refresh();

                var wantedWebs = Solver.getWantedWebs(state);
                var actions = Solver.mobilizeSpitters(state, wantedWebs);

                actions.ForEach(API.Execute);
            }
            catch (Exception e)
            {
                if (e.Message != "ACK")
                {
                    Console.WriteLine(e);
                }
            }
            return true;
        }

        #endregion
    }
}
