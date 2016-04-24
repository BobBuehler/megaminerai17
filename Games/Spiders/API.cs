using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Spiders
{
    static class API
    {
        static Game Game;

        static Player Player;

        public static void Init(Game game, Player player)
        {
            EDist = Extensions.Memoize<Point, double>(p => _edist(p));
			Game = game;
			Player = player;
            idToKey = new Dictionary<string, int>();
            keyToId = new Dictionary<int, string>();
        }

        public static void Refresh()
        {
            Players = Game.Players.ToDictionary(p => p.Id);
            Spiders = Game.Players.SelectMany(p => p.Spiders).ToDictionary(s => s.Id);
            Nests = Game.Nests.ToDictionary(n => n.Id);
            Webs = Game.Webs.ToDictionary(w => w.Id);
            State = new XState(Game);
        }

        public static IDictionary<string, Player> Players;
        public static IDictionary<string, Spider> Spiders;
        public static IDictionary<string, Nest> Nests;
        public static IDictionary<string, Web> Webs;
        public static XState State;


        private static double _edist(Point d)
        {
            return Math.Sqrt(d.x * d.x + d.y * d.y);
        }

        public static Func<Point, double> EDist; // memoized caller
		
		public static int movementTime(double distance)
		{
			return (int)Math.Ceiling(distance / Game.MovementSpeed);
		}

        public static int getNumSpiderlings(XPlayer xplayer)
        {
            return xplayer.Cutters.Count + xplayer.Spitters.Count + xplayer.Weavers.Count;
        }
		
		public static int newEggs(int numSpiderlings)
		{
			return (int)Math.Floor( Game.EggsScalar * (Player.MaxSpiderlings - numSpiderlings) );
		}
		
		public static double cutWorkRemaining(int webStength, double length)
		{
            return (5 * webStength) / (Game.CutSpeed * Math.Sqrt(length));
		}
		
        public static double spitWorkRemaining(double distance)
        {
            return distance / Game.SpitSpeed;
        }

        public static double weaveWorkRemaining(int webStength, double length)
        {
            return (length * Math.Sqrt(webStength)) / Game.WeaveSpeed;
        }

        public static double workMultiplier(int numWorkers)
        {
            return Math.Sqrt(numWorkers);
        }

        public static int initialWebStrength()
        {
            return Game.InitialWebStrength;
        }

        private static IDictionary<string, int> idToKey;
        private static IDictionary<int, string> keyToId;

        public static int GetKey(string id)
        {
            int key;
            if (!idToKey.TryGetValue(id, out key))
            {
                key = idToKey.Count;
                idToKey[id] = key;
                keyToId[key] = id;
            }
            return key;
        }

        public static int GetKey()
        {
            return GetKey("SIM-" + idToKey.Count);
        }

        public static string GetId(int key)
        {
            return keyToId[key];
        }

        public static int getNextNest(int here, int nestA, int nestB)
        {
            return here == nestA ? nestB : nestA;
        }

        public static XNest getNextNest(XState state, XNest nest, XWeb web)
        {
            return state.Nests[getNextNest(nest.Key, web.NestA, web.NestB)];
        }

        public static IEnumerable< Tuple<XWeb, XNest> > getNeighbors(XState state, XNest node)
        {
            Func<XWeb, XNest> getNN = w => API.getNextNest(state, node, w);
            return node.Webs.Select( w => state.Webs[w]).Select(w => Tuple.Create(w, getNN(w)) );
        }

        public static bool isBusy(XSpider spider)
        {
            return spider.WorkRemaining != 0;
        }

        public static IEnumerable<XSpider> getNestSpiders(XState state, XNest nest)
        {
            return nest.Spiders.Select(spider => state.Spiders[spider]);
        }

        public static IEnumerable<XWeb> getNestWebs(XState state, XNest nest)
        {
            return nest.Webs.Select(web => state.Webs[web]);
        }

        public static IEnumerable<XSpider> filterByAllies(int owner, IEnumerable<XSpider> spiders)
        {
            return spiders.Where(spider => spider.Owner == owner);
        }

        public static IEnumerable<XSpider> filterByEnemies(int owner, IEnumerable<XSpider> spiders)
        {
            return spiders.Where(spider => spider.Owner != owner);
        }

        public static XNest getSpiderNest(XState state, XSpider spider)
        {
            return state.Nests[spider.Nest];
        }

        //Doesnt check for owner.. or nest...
        public static bool canAttackKind(XSpider attacker, XSpider attackee)
        {
            if (attacker.Type == XSpiderType.Cutter && attackee.Type == XSpiderType.Weaver)
            {
                return false;
            }
            if (attacker.Type == XSpiderType.Spitter && attackee.Type == XSpiderType.Cutter)
            {
                return false;
            }
            if (attacker.Type == XSpiderType.Weaver && attackee.Type == XSpiderType.Spitter)
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<XSpider> getAttackTargets(XState state, XSpider spider)
        {
            return filterByEnemies(spider.Owner, getNestSpiders(state, getSpiderNest(state, spider))).Where(attackee => canAttackKind(spider, attackee));
        }

        public static IEnumerable<XWeb> getWebTargets(XState state, XSpider spider)
        {
            return getNestWebs(state, getSpiderNest(state, spider));
        }

        public static IEnumerable<XNest> getSpitTargets(XState state, XSpider spider)
        {
            var curNest = state.Nests[spider.Nest];
            var connectedNests = getNestWebs(state, curNest).SelectMany(w => new int[] { w.NestA, w.NestB }).ToHashSet();
            return state.Nests.Values.Where(n => !connectedNests.Contains(n.Key));
        }

        public static bool isCoCutter(XSpider spider1, XSpider spider2)
        {
            return spider1.CuttingWeb != -1 && spider1.CuttingWeb == spider2.CuttingWeb;
        }

        public static bool isCoSpitter(XSpider spider1, XSpider spider2)
        {
            return spider1.SpittingToNest != -1 && 
                ((spider1.SpittingToNest == spider2.SpittingToNest && spider1.Nest == spider2.Nest)
                || (spider1.Nest == spider2.SpittingToNest && spider1.SpittingToNest == spider2.Nest));
        }

        public static bool isCoStrengthener(XSpider spider1, XSpider spider2)
        {
            return spider1.StrengtheningWeb != -1 && spider1.StrengtheningWeb == spider2.StrengtheningWeb;
        }

        public static bool isCoWeakener(XSpider spider1, XSpider spider2)
        {
            return spider1.WeakeningWeb != -1 && spider1.WeakeningWeb == spider2.WeakeningWeb;
        }

        public static XNest OtherNest(XState state, XSpider spider)
        {
            var otherNest = spider.SpittingToNest;
            var targetWeb = spider.CuttingWeb;
            if (spider.StrengtheningWeb != -1)
            {
                targetWeb = spider.StrengtheningWeb;
            }
            if (spider.WeakeningWeb != -1)
            {
                targetWeb = spider.WeakeningWeb;
            }
            if (targetWeb != -1)
            {
                var web = state.Webs[targetWeb];
                otherNest = getNextNest(spider.Nest, web.NestA, web.NestB);
            }
            return otherNest == -1 ? null : state.Nests[otherNest];
        }

        public static HashSet<XSpider> calcCoworkers(XState state, XSpider spider, Func<XSpider, XSpider, bool> areCoworkers)
        {
            var otherNest = OtherNest(state, spider);

            return state.Nests[spider.Nest].Spiders
                .Concat(otherNest.Spiders)
                .Select(s => state.Spiders[s])
                .Where(s => areCoworkers(spider, s))
                .ToHashSet();
        }

        public static T GetSpider<T>(XSpider spider) where T : Spider
        {
            return GetSpider<T>(spider.Key);
        }

        public static T GetSpider<T>(int key) where T : Spider
        {
            return (T)Spiders[GetId(key)];
        }

        public static XWeb GetWeb(XState state, XNest nest1, XNest nest2)
        {
            return nest1.Webs.Select(w => state.Webs[w]).FirstOrDefault(w => getNextNest(state, nest1, w).Equals(nest2));
        }

        public static Web GetWeb(XWeb web)
        {
            return GetWeb(web.Key);
        }

        public static Web GetWeb(int key)
        {
            return Webs[GetId(key)];
        }

        public static Nest GetNest(XNest nest)
        {
            return GetNest(nest.Key);
        }

        public static Nest GetNest(int key)
        {
            return Nests[GetId(key)];
        }

        public static void Execute(XAction action)
        {
            if (Game.CurrentPlayer.TimeRemaining < 1000000000)
            {
                throw new Exception("ACK");
            }
            var player = Game.CurrentPlayer;
            var remaining = player.TimeRemaining;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            switch(action.Type)
            {
                case XActionType.Spawn:
                    GetSpider<BroodMother>(action.Actor).Spawn(action.SpawnType.ToString());
                    break;
                case XActionType.Move:
                    GetSpider<Spiderling>(action.Actor).Move(GetWeb(action.TargetWeb));
                    break;
                case XActionType.Spit:
                    GetSpider<Spitter>(action.Actor).Spit(GetNest(action.TargetNest));
                    break;
                // TODO MORE!
            }
            watch.Stop();
            Console.WriteLine("Action - {0} - {1}", action.Type, (player.TimeRemaining - remaining) / 10000000000);
        }
    }
}
