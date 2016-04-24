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
            Smarts.Init(Game);
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
                Console.WriteLine("{0} {1}-{2}", Game.CurrentTurn, Game.CurrentPlayer.BroodMother.Health, Game.CurrentPlayer.OtherPlayer.BroodMother.Health);
                OtherRun();
                return true;

                // Spawn
                API.Refresh();


                var state = API.State;
                var broodMother = state.Spiders[state.Players[state.CurrentPlayer].BroodMother];

                //var unitCount = Smarts.OurSpiderlings.Where(spi => !spi.Equals(Smarts.Game.CurrentPlayer.BroodMother)).Count();
                Smarts.Refresh();
                var spitterCount = Smarts.OurSpiderlings.Where(spi => spi.GetXSpiderType() == XSpiderType.Spitter).Count();
                var cutterCount = Smarts.OurSpiderlings.Where(spi => spi.GetXSpiderType() == XSpiderType.Cutter).Count();

                for (int i = 0; i < Math.Min(broodMother.Eggs, 20); i++)
                {
                    if (spitterCount / 5 < cutterCount)
                    {
                        spitterCount++;
                        API.Execute(new XAction(broodMother, XActionType.Spawn) { SpawnType = XSpiderType.Spitter });
                    }
                    else
                    {
                        cutterCount++;
                        API.Execute(new XAction(broodMother, XActionType.Spawn) { SpawnType = XSpiderType.Cutter });
                    }
                }

                Smarts.Refresh();

                if (Game.CurrentPlayer.Spiders.Count > 100)
                {
                    Solver.Assault(Smarts.OurSpiderlings, 50);
                }
                Solver.Attack(Smarts.OurSpiderlings);
                Solver.Assault(Smarts.OurSpiderlings, 2);
                Solver.SpreadSpiderlings(Game.CurrentPlayer.Spiders.Where(s => s is Spitter).Select(s => s as Spitter));
                Solver.SpreadCutters();
                Solver.Attack(Smarts.OurSpiderlings, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(Game.CurrentPlayer.TimeRemaining);
                Console.WriteLine(e);
            }
            return true;
        }

        public void OtherRun()
        {
            API.Refresh();
            Smarts.Refresh();

            var mother = Game.CurrentPlayer.BroodMother;
            var eggCount = mother.Eggs;
            var cutterCount = Smarts.OurSpiderlings.Count(s => s is Cutter);
            var spitterCount = Smarts.OurSpiderlings.Count(s => s is Spitter);
            var weaverCount = Smarts.OurSpiderlings.Count(s => s is Weaver);
            for (int i = 0; i < eggCount; i++)
            {
                //if (cutterCount < 20)
                //{
                //    mother.Spawn("Cutter");
                //}
                if (spitterCount / (weaverCount + 1) < 5)
                {
                    mother.Spawn("Spitter");
                }
                else
                {
                    mother.Spawn("Weaver");
                }
            }

            Solver.Attack(Smarts.OurSpiderlings);

            var idleHomeSpitters = Game.CurrentPlayer.BroodMother.Nest.Spiders.GetOwned<Spitter>(Game.CurrentPlayer);
            SuicideWebs(idleHomeSpitters.Count() / 15);

            var ourNest = mother.Nest;
            var theirNest = Game.CurrentPlayer.OtherPlayer.BroodMother.Nest;
            var stops = Game.Nests.Where(n => n != ourNest && n != theirNest).OrderBy(n => ourNest.EDist(n) + n.EDist(theirNest));
            foreach(var stop in stops)
            {
                Web incoming = Smarts.Webs.GetOrDefault(Tuple.Create(ourNest.ToPoint(), stop.ToPoint()));
                Web outgoing = Smarts.Webs.GetOrDefault(Tuple.Create(stop.ToPoint(), theirNest.ToPoint()));

                if (incoming == null)
                {
                    Spit(ourNest, stop);
                }
                else
                {
                    MoveToQuota<Weaver>(ourNest, stop, incoming, 2);
                    MoveToQuota<Spitter>(ourNest, stop, incoming, 2);
                }

                if (outgoing == null)
                {
                    Spit(stop, theirNest);
                }
                else
                {
                    MoveToQuota<Spitter>(stop, theirNest, outgoing, 2);
                }
            }
        }

        public void Spit(Nest nestA, Nest nestB)
        {
            var spitters = nestA.Spiders.GetOwned<Spitter>(Game.CurrentPlayer);
            if (spitters.Any(s => s.SpittingWebToNest == nestB))
            {
                return;
            }

            var spitter = spitters.FirstOrDefault(s => s.WorkRemaining == 0);
            if (spitter != null)
            {
                spitter.Spit(nestB);
            }
        }

        public void MoveToQuota<T>(Nest nestA, Nest nestB, Web web, int quota) where T : Spiderling
        {
            if (web.Load > 2)
            {
                return;
            }

            var toB = nestB.Spiders.Concat(web.Spiderlings.Where(s => s.MovingToNest == nestB)).GetOwned<T>(Game.CurrentPlayer);
            if (toB.Count() < quota)
            {
                var move = nestA.Spiders.GetOwned<T>(Game.CurrentPlayer).FirstOrDefault(s => s.WorkRemaining == 0);
                if (move != null)
                {
                    move.Move(web);
                }

            }
        }

        public void SuicideWebs(int count)
        {
            var ourNest = Game.CurrentPlayer.BroodMother.Nest;

            foreach(var web in ourNest.Webs.OrderByDescending(w => w.Spiderlings.Count(s => s.Owner != Game.CurrentPlayer)))
            {
                if (web.Spiderlings.Count(s => s.Owner != Game.CurrentPlayer) > 1)
                {
                    var idleHomeSpitters = ourNest.Spiders.GetOwned<Spitter>(Game.CurrentPlayer).Where(s => s.WorkRemaining == 0).ToArray();
                    var spidersToKill = web.Strength - web.Load + 1;
                    if (idleHomeSpitters.Count() > spidersToKill)
                    {
                        idleHomeSpitters.Take(spidersToKill).ForEach(s => s.Move(web));
                        
                        count--;
                        if (count == 0)
                        {
                            return;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
