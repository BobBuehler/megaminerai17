using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Spiders
{
    class Solver
    {
        public static Dictionary<XNest, int> Pather(XState state, XNest start)
        {
            AStar<XNest> astar = new AStar<XNest>
                (
                start.Single(), 
                ( XNest => false ), 
                ( x1, x2) => API.movementTime( x1.Location.EDist( x2.Location ) ), 
                ( XNest => 0 ), 
                ( node => API.getNeighbors( state, node ).Select( tup => tup.Item2 ) )
                );
            return astar.GScore;
        }

        public static XAction generateAction(XSpider attacker, XSpider attackee)
        {
            XAction act = new XAction(attacker, XActionType.Attack);
            act.TargetSpider = attackee;
            return act;
        }

        public static XAction generateAction(XSpider mover, XWeb path, XActionType actType)
        {
            XAction act = new XAction(mover, actType);
            act.TargetWeb = path;
            return act;
        }

        public static XAction generateAction(XSpider spitter, XNest nest)
        {
            XAction act = new XAction(spitter, XActionType.Spit);
            act.TargetNest = nest;
            return act;
        }

        public static IEnumerable<XAction> generateActions(XState state, XSpider spider)
        {
            List<XAction> actionList = new List<XAction>();
            if (API.isBusy(spider))
            {
                return actionList;
            }

            actionList.AddRange( API.getAttackTargets(state, spider).Select( target => generateAction(spider, target) ) );

            var webTargets = API.getWebTargets(state, spider);
            actionList.AddRange( webTargets.Select( target => generateAction(spider, target, XActionType.Move) ) );

            if (spider.Type == XSpiderType.Cutter)
            {
                actionList.AddRange( webTargets.Select( target => generateAction(spider, target, XActionType.Cut) ) );
            }
            else if (spider.Type == XSpiderType.Weaver)
            {
                actionList.AddRange( webTargets.Select( target => generateAction(spider, target, XActionType.Spit) ) );
            }
            else if (spider.Type == XSpiderType.Spitter)
            {
                actionList.AddRange( API.getSpitTargets(state, spider).Select( target => generateAction(spider, target) ) );
            }

            return actionList;
        }

        public static IEnumerable<Tuple<int, int>> getWantedWebs(XState state)
        {
            var player = state.Players[state.OtherPlayer];
            var broodMother = state.Spiders[player.BroodMother];

            var nestsByPoints = state.Nests.Values.Where(n => n.Key != broodMother.Nest).ToDictionary(n => n.Location);
            return T(nestsByPoints.Keys).Select(t => Tuple.Create(nestsByPoints[t.Item1].Key, nestsByPoints[t.Item2].Key));
        }

        public static IEnumerable<XAction> mobilizeSpitters(XState state, IEnumerable<Tuple<int, int>> wantedWebs)
        {
            var existingWebs = state.Webs.Values.SelectMany(w => new [] {Tuple.Create(w.NestA, w.NestB), Tuple.Create(w.NestB, w.NestA)}).ToHashSet();
            var websThatDontExist = wantedWebs.Where(t => !existingWebs.Contains(Tuple.Create(t.Item1, t.Item2)));
            var idleSpitters = state.Players[state.CurrentPlayer].Spitters
                .Select(s => state.Spiders[s])
                .Where(s => s.WorkRemaining == 0);
            var nestsWithIdleSpitters = idleSpitters.Select(s => s.Nest).ToHashSet();
            var connectedWebsToMake = websThatDontExist.Where(t => nestsWithIdleSpitters.Contains(t.Item1) || nestsWithIdleSpitters.Contains(t.Item2)).ToLazyList();
            var nestsOfInterest = websThatDontExist.SelectMany(w => new[] { w.Item1, w.Item2 }).ToHashSet();
            foreach (var spitter in idleSpitters)
            {
                var webToMake = connectedWebsToMake.FirstOrDefault(t => spitter.Nest == t.Item1 || spitter.Nest == t.Item2);
                if (webToMake != null)
                {
                    var targetNest = API.getNextNest(spitter.Nest, webToMake.Item1, webToMake.Item2);
                    yield return new XAction(spitter, XActionType.Spit) { TargetNest = state.Nests[targetNest] };
                }
                else
                {
                    var search = new AStar<XNest>
                        (
                            state.Nests[spitter.Nest].Single(),
                            n => nestsOfInterest.Contains(n.Key),
                            (n1, n2) => API.movementTime(n1.Location.EDist(n2.Location)),
                            n => 0,
                            n => getConnectedNests(state, n)
                        );
                    Console.WriteLine(state.Nests[spitter.Nest].Location);
                    search.From.ForEach(kvp => Console.WriteLine("{0}:{1}", kvp.Key.Location, kvp.Value.Location));
                    search.Path.ForEach(n => Console.Write("{0}->", n.Location));
                    Console.WriteLine();
                    if (search.Path.Count > 1)
                    {
                        yield return new XAction(spitter, XActionType.Move) { TargetWeb = API.GetWeb(state, search.Path.Nth(0), search.Path.Nth(1)) };
                    }
                }
            }
        }

        public static IEnumerable<XNest> getConnectedNests(XState state, XNest nest)
        {
            return nest.Webs.Select(w => API.getNextNest(state, nest, state.Webs[w]));
        }

        public static IEnumerable<Tuple<Point, Point>> T(IEnumerable<Point> points)
        {
            int maxSeenCount = 6;
            var edges = points.SelectMany(p1 => points.Select(p2 => new { p1 = p1, p2 = p2, dist = p1.EDist(p2)}));
            var seenCount = points.ToDictionary(p => p, p => 0);
            var seen = new HashSet<Tuple<Point, Point>>();
            foreach(var edge in edges.Where(e => !e.p1.Equals(e.p2)).OrderBy(e => e.dist))
            {
                if (seenCount[edge.p1] < maxSeenCount && seenCount[edge.p2] < maxSeenCount && !seen.Contains(Tuple.Create(edge.p1, edge.p2)))
                {
                    seenCount[edge.p1]++;
                    seenCount[edge.p2]++;
                    seen.Add(Tuple.Create(edge.p1, edge.p2));
                    seen.Add(Tuple.Create(edge.p2, edge.p1));
                    yield return Tuple.Create(edge.p1, edge.p2);
                }
            }

        }

        public static void SpreadSpiderlings(IEnumerable<Spiderling> lings)
        {
            Smarts.Refresh();
            var idle = lings.Where(s => s.WorkRemaining == 0);
            if (!idle.Any())
            {
                return;
            }

            foreach (var ling in idle)
            {
                Smarts.Refresh();
                var lingsPerPoint = Smarts.Game.Nests.ToDictionary(n => n.ToPoint(), n => 0);
                lings.ForEach(s => lingsPerPoint[s.ToPoint()]++);
                var nearAndLess = Smarts.Game.Nests.Where(n => lingsPerPoint[n.ToPoint()] < lingsPerPoint[ling.Nest.ToPoint()]).OrderBy(n => ling.Nest.ToPoint().EDist(n.ToPoint()));
                foreach (var nest in nearAndLess)
                {
                    Web web;
                    if (!Smarts.Webs.TryGetValue(Tuple.Create(ling.Nest.ToPoint(), nest.ToPoint()), out web))
                    {
                        if (ling is Spitter)
                        {
                            (ling as Spitter).Spit(nest);
                            break;
                        }
                    }
                    else
                    {
                        if (web.Load < web.Strength)
                        {
                            ling.Move(web);
                            break;
                        }
                    }
                }
            }
        }

        public static void Attack(IEnumerable<Spiderling> lings, bool suicideOk = false)
        {
            foreach(var ling in lings.Where(l => l.WorkRemaining == 0))
            {
                foreach(var target in ling.Nest.Spiders.Where(s => s.Owner != ling.Owner))
                {
                    if (API.canAttackKind(ling.GetXSpiderType(), target.GetXSpiderType()))
                    {
                        if (suicideOk || ling.GetXSpiderType() != target.GetXSpiderType())
                        {
                            ling.Attack(target as Spiderling);
                            break;
                        }
                    }
                }
            }
        }
    }
}
