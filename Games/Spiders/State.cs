﻿using System;
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
            Key = obj.GetKey();
        }

        public XBase(XBase copy)
        {
            Key = copy.Key;
        }

        public XBase()
        {
            Key = API.GetKey();
        }

        public override int GetHashCode()
        {
            return Key;
        }
    }

    class XPlayer : XBase
    {
        public int BroodMother;
        public HashSet<int> Cutters;
        public HashSet<int> Spitters;
        public HashSet<int> Weavers;

        public XPlayer(Player obj)
            : base(obj)
        {
            BroodMother = obj.BroodMother.GetKey();
            Cutters = obj.Spiders.Where(s => s.GetXSpiderType() == XSpiderType.Cutter).Select(xs => xs.GetKey()).ToHashSet();
            Spitters = obj.Spiders.Where(s => s.GetXSpiderType() == XSpiderType.Spitter).Select(xs => xs.GetKey()).ToHashSet();
            Weavers = obj.Spiders.Where(s => s.GetXSpiderType() == XSpiderType.Weaver).Select(xs => xs.GetKey()).ToHashSet();
        }

        public XPlayer(XPlayer copy)
            : base(copy)
        {
            BroodMother = copy.BroodMother;
            Cutters = new HashSet<int>(copy.Cutters);
            Spitters = new HashSet<int>(copy.Spitters);
            Weavers = new HashSet<int>(copy.Weavers);
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
        public int NumberOfCoworkers;
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

            NumberOfCoworkers = 0;
            MovingOnWeb = -1;
            MovingToNest = -1;
            WorkRemaining = -1;

            CuttingWeb = -1;
            SpittingToNest = -1;
            StrengtheningWeb = -1;
            WeakeningWeb = -1;

            if (obj is BroodMother)
            {
                var bm = obj as BroodMother;
                Type = XSpiderType.BroodMother;
                Eggs = bm.Eggs;
                Health = bm.Health;
            }
            else
            {
                var ling = obj as Spiderling;
                NumberOfCoworkers = ling.NumberOfCoworkers;
                MovingOnWeb = ling.MovingOnWeb.GetKey();
                MovingToNest = ling.MovingToNest.GetKey();
                WorkRemaining = ling.WorkRemaining;
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
                    var weave = obj as Weaver;
                    Type = XSpiderType.Weaver;
                    StrengtheningWeb = weave.StrengtheningWeb.GetKey();
                    WeakeningWeb = weave.WeakeningWeb.GetKey();
                }
            }
        }

        public XSpider(XSpider copy)
            : base(copy)
        {
            Type = copy.Type;
            Nest = copy.Nest;
            Owner = copy.Owner;
            Eggs = copy.Eggs;
            Health = copy.Health;
            NumberOfCoworkers = copy.NumberOfCoworkers;
            MovingOnWeb = copy.MovingOnWeb;
            MovingToNest = copy.MovingToNest;
            CuttingWeb = copy.CuttingWeb;
            SpittingToNest = copy.SpittingToNest;
            StrengtheningWeb = copy.StrengtheningWeb;
            WeakeningWeb = copy.WeakeningWeb;
        }

        public XSpider(XSpiderType type, int nest, int owner)
            : base()
        {
            Type = type;
            Nest = nest;
            Owner = owner;

            Eggs = -1;
            Health = -1;

            NumberOfCoworkers = 0;
            MovingOnWeb = -1;
            MovingToNest = -1;
            WorkRemaining = -1;

            CuttingWeb = -1;
            SpittingToNest = -1;
            StrengtheningWeb = -1;
            WeakeningWeb = -1;
        }
    }

    class XNest : XBase
    {
        public Point Location;
        public HashSet<int> Webs;
        public HashSet<int> Spiders;

        public XNest(Nest obj)
            : base(obj)
        {
            Location = new Point(obj.X, obj.Y);
            Webs = obj.Webs.Select(w => w.GetKey()).ToHashSet();
            Spiders = obj.Spiders.Select(s => s.GetKey()).ToHashSet();
        }

        public XNest(XNest copy)
            : base(copy)
        {
            Location = copy.Location;
            Webs = new HashSet<int>(copy.Webs);
            Spiders = new HashSet<int>(copy.Spiders);
        }
    }

    class XWeb : XBase
    {
        public double Length;
        public int Load;
        public int NestA;
        public int NestB;
        public HashSet<int> Spiders;
        public int Strength;

        public XWeb(XWeb copy)
            : base(copy)
        {
            Length = copy.Length;
            Load = copy.Load;
            NestA = copy.NestA;
            NestB = copy.NestB;
            Spiders = new HashSet<int>(copy.Spiders);
            Strength = copy.Strength;
        }

        public XWeb(Web obj)
            : base(obj)
        {
            Length = obj.Length;
            Load = obj.Load;
            NestA = obj.NestA.GetKey();
            NestB = obj.NestB.GetKey();
            Spiders = obj.Spiderlings.Select(s => s.GetKey()).ToHashSet();
            Strength = obj.Strength;
        }

        public XWeb(double length, int nestA, int nestB, int strength)
            : base()
        {
            Length = length;
            Load = 0;
            NestA = nestA;
            NestB = nestB;
            Spiders = new HashSet<int>();
            Strength = strength;
        }
    }

    class XState
    {
        public int CurrentPlayer;
        public int OtherPlayer;
        public int CurrentTurn;
        public IDictionary<int, XPlayer> Players;
        public IDictionary<int, XSpider> Spiders;
        public IDictionary<int, XNest> Nests;
        public IDictionary<int, XWeb> Webs;

        public XState(Game game)
        {
            CurrentPlayer = game.CurrentPlayer.GetKey();
            OtherPlayer = game.CurrentPlayer.OtherPlayer.GetKey();
            CurrentTurn = game.CurrentTurn;
            Players = game.Players.Select(p => new XPlayer(p)).ToDictionary(p => p.Key);
            Spiders = game.Players.SelectMany(p => p.Spiders).Select(s => new XSpider(s)).ToDictionary(s => s.Key);
            Nests = game.Nests.Select(n => new XNest(n)).ToDictionary(n => n.Key);
            Webs = game.Webs.Select(w => new XWeb(w)).ToDictionary(w => w.Key);
        }

        public XState(XState copy)
        {
            CurrentPlayer = copy.CurrentPlayer;
            CurrentPlayer = copy.OtherPlayer;
            CurrentTurn = copy.CurrentTurn;
            Players = copy.Players.ToDictionary(kvp => kvp.Key, kvp => new XPlayer(kvp.Value));
            Spiders = copy.Spiders.ToDictionary(kvp => kvp.Key, kvp => new XSpider(kvp.Value));
            Nests = copy.Nests.ToDictionary(kvp => kvp.Key, kvp => new XNest(kvp.Value));
            Webs = copy.Webs.ToDictionary(kvp => kvp.Key, kvp => new XWeb(kvp.Value));
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
        Strengthen,
        Weaken
    }

    class XAction
    {
        public XSpider Actor;
        public XActionType Type;

        public XSpiderType SpawnType;
        public XSpider TargetSpider;
        public XWeb TargetWeb;
        public XNest TargetNest;

        public XAction(XSpider actor, XActionType type)
        {
            Actor = actor;
            Type = type;
            SpawnType = XSpiderType.BroodMother;
            TargetSpider = null;
            TargetWeb = null;
            TargetNest = null;
        }
    }
}
