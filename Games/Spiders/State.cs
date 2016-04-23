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
        public int Key;

        public XBase(BaseGameObject obj)
        {
            this.Key = API.GetKey(obj.Id);
        }

        public override int GetHashCode()
        {
            return Key;
        }
    }

    class XPlayer : XBase
    {
        public int ID;
        public int BroodMother;
        public IEnumerable<int> Cutters;
        public IEnumerable<int> Spitters;
        public IEnumerable<int> Weavers;

        public XPlayer(Player obj)
            : base(obj)
        {
        }
    }

    class XSpider : XBase
    {
        // Spider
        public XSpiderType Type;
        public int Nest;
        public int Owner;

        // BroodMother
        public double Eggs;
        public int Health;

        // Spiderling
        public IEnumerable<int> Coworkers;
        public int MovingOnWeb;
        public int MovingToNest;
        public double WorkRemaining;

        // Cutter
        public int CuttingWeb;

        // Spitter
        public int SpittingToNest;

        // Weaver
        public int StrengtheningWeb;
        public int WeakeningWeb;

        public XSpider(Spider obj)
            : base(obj)
        {
        }
    }

    class XNest : XBase
    {
        public Point Location;
        public IEnumerable<int> Webs;
        public IEnumerable<int> Spiders;

        public XNest(Nest obj)
            : base(obj)
        {
        }
    }

    class XWeb : XBase
    {
        public double Length;
        public int Load;
        public int NestA;
        public int NestB;
        public IEnumerable<int> Spiders;
        public int Strength;

        public XWeb(Web obj)
            : base(obj)
        {
            this.Length = obj.Length;
        }
    }

    class XState
    {
        public IDictionary<int, XPlayer> Players;
        public IDictionary<int, XSpider> Spiders;
        public IDictionary<int, XNest> Nests;
        public IDictionary<int, XWeb> Webs;

        public XState(Game game)
        {
            Players = game.Players.Select(p => new XPlayer(p)).ToDictionary(p => p.Key);
            Spiders = game.Players.SelectMany(p => p.Spiders).Select(s => new XSpider(s)).ToDictionary(s => s.Key);
            Nests = game.Nests.Select(n => new XNest(n)).ToDictionary(n => n.Key);
            Webs = game.Webs.Select(w => new XWeb(w)).ToDictionary(w => w.Key);
        }
    }

    enum XActionType
    {
        Consume,
        Spawn,

        Attack,
        Move,

        Cut,
        Spit,
        Weave
    }

    class XAction
    {
        public XSpider actor;

        public XSpiderType spawnType;
        public XSpider targetSpider;
        public XWeb targetWeb;
        public XNest targetNest;
    }
}
