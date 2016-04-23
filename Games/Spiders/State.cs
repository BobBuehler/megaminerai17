using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Spiders
{
    enum XSpiderType
    {
        BroodMother,
        Cutter,
        Spitter,
        Weaver
    }

    class XBase
    {
        public string hash;
        public override int GetHashCode()
        {
            return hash.GetHashCode();
        }

        public XBase(BaseGameObject obj)
        {
            this.hash = obj.Id;
        }
    }

    class XPlayer : XBase
    {
        public int ID;
        public XSpider BroodMother;
        public IEnumerable<XSpider> Cutters;
        public IEnumerable<XSpider> Spitters;
        public IEnumerable<XSpider> Weavers;

        public XPlayer(Player obj, int id)
            : base(obj)
        {
            ID = id;
        }
    }

    class XSpider : XBase
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

        public XSpider(Spider obj)
            : base(obj)
        {
        }
    }

    class XNest : XBase
    {
        public Point Location;
        public IEnumerable<XWeb> Webs;
        public IEnumerable<XSpider> Spiders;

        public XNest(Nest obj)
            : base(obj)
        {
        }
    }

    class XWeb : XBase
    {
        public double Length;
        public int Load;
        public XNest NestA;
        public XNest NestB;
        public IEnumerable<XSpider> Spiders;
        public int Strength;

        public XWeb(Web obj)
            : base(obj)
        {
            this.Length = obj.Length;
        }
    }

    class XState
    {
        // Players, Nests, Webs
    }
}
