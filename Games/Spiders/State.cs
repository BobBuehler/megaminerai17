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
            this.Key = obj.GetKey();
        }

        public override int GetHashCode()
        {
            return Key;
        }
    }

    class XPlayer : XBase
    {
        public int BroodMother;
        public IEnumerable<int> Cutters;
        public IEnumerable<int> Spitters;
        public IEnumerable<int> Weavers;

        public XPlayer(Player obj)
            : base(obj)
        {
            BroodMother = obj.BroodMother.GetKey();
            Cutters = obj.Spiders.Where(s => s.GetXSpiderType() == XSpiderType.Cutter).Select(xs => xs.GetKey());
            Spitters = obj.Spiders.Where(s => s.GetXSpiderType() == XSpiderType.Spitter).Select(xs => xs.GetKey());
            Weavers = obj.Spiders.Where(s => s.GetXSpiderType() == XSpiderType.Weaver).Select(xs => xs.GetKey());
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
            Type = XSpiderType.BroodMother;
            Nest = obj.Nest.GetKey();
            Owner = obj.Owner.GetKey();

            Eggs = -1;
            Health = -1;

            Coworkers = Enumerable.Empty<int>();
            MovingOnWeb = -1;
            MovingToNest = -1;
            WorkRemaining = -1;

            CuttingWeb = -1;
            SpittingToNest = -1;
            StrengtheningWeb = -1;
            WeakeningWeb = -1;

            if (obj is Spiderling)
            {
                Coworkers = (obj as Spiderling).Coworkers.Select(s => s.GetKey());
                MovingOnWeb = (obj as Spiderling).MovingOnWeb.GetKey();
                MovingToNest = (obj as Spiderling).MovingToNest.GetKey();
                WorkRemaining = (obj as Spiderling).WorkRemaining;
            }

            if (obj is Cutter)
            {
                Type = XSpiderType.Cutter;
                CuttingWeb = (obj as Cutter).CuttingWeb.GetKey();
            }
            else if (obj is Spitter)
            {
                Type = XSpiderType.Spitter;
                SpittingToNest = (obj as Spitter).SpittingWebToNest.GetKey();
            }
            else if (obj is Weaver)
            {
                Type = XSpiderType.Weaver;
                StrengtheningWeb = (obj as Weaver).StrengtheningWeb.GetKey();
                WeakeningWeb = (obj as Weaver).WeakeningWeb.GetKey();
            }
            else if (obj is BroodMother)
            {
                Type = XSpiderType.BroodMother;
                Eggs = (obj as BroodMother).Eggs;
                Health = (obj as BroodMother).Health;
            }
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
            Location = new Point(obj.X, obj.Y);
            Webs = obj.Webs.Select(w => w.GetKey());
            Spiders = obj.Spiders.Select(s => s.GetKey());
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
            Length = obj.Length;
            Load = obj.Load;
            NestA = obj.NestA.GetKey();
            NestB = obj.NestB.GetKey();
            Spiders = obj.Spiderlings.Select(s => s.GetKey());
            Strength = obj.Strength;
        }
    }

    class XState
    {
        public int CurrentPlayer;
        public int CurrentTurn;
        public IDictionary<int, XPlayer> Players;
        public IDictionary<int, XSpider> Spiders;
        public IDictionary<int, XNest> Nests;
        public IDictionary<int, XWeb> Webs;

        public XState(Game game)
        {
            CurrentPlayer = game.CurrentPlayer.GetKey();
            CurrentTurn = game.CurrentTurn;
            Players = game.Players.Select(p => new XPlayer(p)).ToDictionary(p => p.Key);
            Spiders = game.Players.SelectMany(p => p.Spiders).Select(s => new XSpider(s)).ToDictionary(s => s.Key);
            Nests = game.Nests.Select(n => new XNest(n)).ToDictionary(n => n.Key);
            Webs = game.Webs.Select(w => new XWeb(w)).ToDictionary(w => w.Key);
        }

        public XState(XState state)
        {
            // TODO, invoke copy constructors
            CurrentPlayer = state.CurrentPlayer;
            CurrentTurn = state.CurrentTurn;
            Players = state.Players.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Spiders = state.Spiders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Nests = state.Nests.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Webs = state.Webs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
        public XSpider Actor;
        public XActionType Type;

        public XSpiderType SpawnType;
        public XSpider TargetSpider;
        public XWeb TargetWeb;
        public XNest TargetNest;
    }
}
