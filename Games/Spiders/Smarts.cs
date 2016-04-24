using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Spiders
{
    static class Smarts
    {
        public static Game Game;

        public static void Init(Game game)
        {
            Game = game;
        }

        public static IDictionary<Point, Nest> Nests;
        public static IDictionary<Point, HashSet<Point>> WebGraph;
        public static IDictionary<Tuple<Point, Point>, Web> Webs;
        public static IEnumerable<Spiderling> OurSpiderlings;
        public static IEnumerable<Spiderling> TheirSpiderlings;

        public static void Refresh()
        {
            Nests = Game.Nests.ToDictionary(n => n.ToPoint());
            WebGraph = Game.Nests.ToDictionary(n => n.ToPoint(), n => new HashSet<Point>());
            Webs = new Dictionary<Tuple<Point, Point>, Web>();
            foreach (var web in Game.Webs)
            {
                var points = web.ToPoints();
                WebGraph[points.Item1].Add(points.Item2);
                WebGraph[points.Item2].Add(points.Item1);
                Webs[points] = web;
                Webs[points.Reverse()] = web;
            }
            OurSpiderlings = Game.CurrentPlayer.Spiders.Where(s => s is Spiderling).Select(s => s as Spiderling);
            TheirSpiderlings = Game.CurrentPlayer.OtherPlayer.Spiders.Where(s => s is Spiderling).Select(s => s as Spiderling);
        }

        public static Point ToPoint(this Nest nest)
        {
            return new Point(nest.X, nest.Y);
        }

        public static Tuple<Point, Point> ToPoints(this Web web)
        {
            return Tuple.Create(web.NestA.ToPoint(), web.NestB.ToPoint());
        }

        public static Tuple<T, T> Reverse<T>(this Tuple<T, T> tuple)
        {
            return Tuple.Create(tuple.Item2, tuple.Item1);
        }
    }
}
