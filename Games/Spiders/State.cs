using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Spiders
{
    public enum XSpiderType
    {
        BroodMother,
        Cutter,
        Spitter,
        Weaver
    }

    public class XBase
    {
        public string Id;
        public override int GetHashCode()
        {
 	         return Id.GetHashCode();
        }
    }

    public class XPlayer : XBase
    {
        public int ID;
        public XSpider BroodMother;
        public IEnumerable<XSpider> Cutters;
        public IEnumerable<XSpider> Spitters;
        public IEnumerable<XSpider> Weavers;
    }

    public class XSpider : XBase
    {
        // Spider
        public XSpiderType Type;
        public XNest Nest;
        public XPlayer Owner;

        // BroodMother
        public double Eggs;
        public int Health;

        // Spiderling
        public IEnumerable<XSpider> Coworkers;
        public XWeb MovingOnWeb;
        public XNest MovingToNest;
        public double WorkRemaining;

        // Cutter
        public XWeb CuttingWeb;

        // Spitter
        public XNest SpittingToNest;

        // Weaver
        public XWeb StrengtheningWeb;
        public XWeb WeakeningWeb;
    }

    public class XNest : XBase
    {
        public Point Location;
        public IEnumerable<XWeb> Webs;
        public IEnumerable<XSpider> Spiders;
    }

    public class XWeb : XBase
    {
        public double Length;
        public int Load;
        public XNest NestA;
        public XNest NestB;
        public IEnumerable<XSpider> Spiders;
        public int Strength;
    }

    public class XState
    {
        // Players, Nests, Webs
    }
}
