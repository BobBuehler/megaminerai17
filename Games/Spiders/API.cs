using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Spiders
{
    public static class API
    {
        public static void Init()
        {
            EDist = Extensions.Memoize<Point, double>(p => _edist(p));
        }

        private static double _edist(Point d)
        {
            return Math.Sqrt(d.x * d.x + d.y * d.y);
        }

        public static Func<Point, double> EDist; // memoized caller
    }
}
