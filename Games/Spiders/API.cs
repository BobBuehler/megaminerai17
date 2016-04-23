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

        private static double _edist(Point d)
        {
            return Math.Sqrt(d.x * d.x + d.y * d.y);
        }

        public static Func<Point, double> EDist; // memoized caller
		
		public static double movementTime(double distance)
		{
			return Math.Ceiling(distance / Game.MovementSpeed);
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

        public static string GetId(int key)
        {
            return keyToId[key];
        }
    }
}
