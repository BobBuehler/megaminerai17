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

        public static IEnumerable<XAction> mobilizeCutters(XState state, IEnumerable<Tuple<int, int>> wantedWebs)
        {
            var existingWebs = state.Webs.Values.SelectMany(w => new[] { Tuple.Create(w.NestA, w.NestB), Tuple.Create(w.NestB, w.NestA) }).ToHashSet();
            var websThatNeedCut = existingWebs.Where(web => !wantedWebs.Contains(web));
            var idleCutters = state.Players[state.CurrentPlayer].Cutters
                .Select(s => state.Spiders[s])
                .Where(s => s.WorkRemaining == 0);
            var nestsWithIdleCutters = idleCutters.Select(s => s.Nest).ToHashSet();
            var connectedWebsToCut = websThatNeedCut.Where(web => nestsWithIdleCutters.Contains(web.Item1) || nestsWithIdleCutters.Contains(web.Item2));

            //var connectedWebsToMake = websThatDontExist.Where(t => nestsWithIdleCutters.Contains(t.Item1) || nestsWithIdleCutters.Contains(t.Item2)).ToLazyList();
            //var nestsOfInterest = websThatDontExist.SelectMany(w => new[] { w.Item1, w.Item2 }).ToHashSet();

            var broodMotherNest = state.Nests[API.getAllyBroodMother(state).Nest];

            var closeEnemySpiders = API.getEnemySpidersNearNest(state, broodMotherNest, 20);
            var closeEnemyNests = closeEnemySpiders.Select( spi => state.Nests[spi.Nest] ).ToHashSet();
            var broodMotherCutQuota = 10 + 5 * closeEnemyNests.Count();

            var quoteSpiders = idleCutters.Where(spider => state.Nests[spider.Nest] == broodMotherNest).Take( broodMotherCutQuota );
            var otherSpiders = idleCutters.Where(spider => !quoteSpiders.Contains(spider));

            var bmWebs = connectedWebsToCut.Where(web => state.Nests[web.Item1] == broodMotherNest || state.Nests[web.Item2] == broodMotherNest);
            var otherWebs = connectedWebsToCut.Where(web => !bmWebs.Contains(web));

            bmWebs.OrderBy(key => state.Nests[key.Item1].Location.EDist(state.Nests[key.Item2].Location));
            otherWebs.OrderBy(key => state.Nests[key.Item1].Location.EDist(state.Nests[key.Item2].Location));

            foreach(var spider in quoteSpiders)
            {
                //Guard broodMother
            }


            foreach (var web in otherWebs)
            {
                /*
                var search = new AStar<XNest>
                    (
                    state.Nests[spitter.Nest].Single(),
                    n => nestsOfInterest.Contains(n.Key),
                    (n1, n2) => API.movementTime(n1.Location.EDist(n2.Location)),
                    n => 0,
                    n => getConnectedNests(state, n)
                    );
                 */
            }
            return null;
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

        public static void SpreadCutters()
        {
            Smarts.Refresh();
            var ourCutters = Smarts.Game.CurrentPlayer.Spiders.Where(s => s.GetXSpiderType() == XSpiderType.Cutter).Select(s => s as Cutter).ToArray();
            var idleCutters = ourCutters.Where(s => s.WorkRemaining == 0);
            var nestsWithCutters = ourCutters.Select(s => s.MovingToNest != null ? s.MovingToNest : s.Nest).Select(n => n.ToPoint()).ToHashSet();
            
            var broodMotherNest = Smarts.Game.CurrentPlayer.BroodMother.Nest;
            SpreadCuttersNest(broodMotherNest);

        }

        public static IEnumerable<Spider> cuttersCutWithSuicide(IEnumerable<Spider> cutters, IEnumerable<Web> targetWebs)
        {
            var unusedCutters = cutters;
            Dictionary<Web, IEnumerable<Spider>> cutPairs = new Dictionary<Web, IEnumerable<Spider>>();
            Dictionary<Web, IEnumerable<Spider>> suicidePairs = new Dictionary<Web, IEnumerable<Spider>>();

            Action<Web, int, Dictionary<Web, IEnumerable<Spider>>> assignCutters = (w, numCutters, actPairs) =>
            {
                actPairs[w] = unusedCutters.Take(numCutters);
                unusedCutters = unusedCutters.Skip(numCutters);
            };
            foreach(var web in targetWebs)
            {
                var webLength = API.webLength(web);
                Console.WriteLine("TargetWebs - " + web);
                if (webLength <= 20)
                {
                    var numSuiciders = web.Strength - web.Load + 1;
                    assignCutters(web, numSuiciders, suicidePairs);
                }
                else
                {
                    var numCutters = (webLength < 30 ? 2 : 1);
                    assignCutters(web, numCutters, cutPairs);
                }
            }

            Console.WriteLine("Attempt foreach cut");
            foreach(var entry in suicidePairs)
            {
                Console.WriteLine("Suicicdepairs - " + entry);
                entry.Value.Select(spi => (Cutter)spi).ForEach(cutter => cutter.Move(entry.Key));
            }

            foreach (var entry in cutPairs)
            {
                Console.WriteLine("Cutpairs - " + entry);
                entry.Value.Select(spi => (Cutter)spi).ForEach(cutter => cutter.Cut(entry.Key));
            }

            return unusedCutters;
        }

        public static IEnumerable<Spider> cuttersCut(IEnumerable<Spider> cutters, HashSet<Web> targetWebs)
        {
            var unusedCutters = cutters;
            Dictionary<Web, IEnumerable<Spider>> cutPairs = new Dictionary<Web, IEnumerable<Spider>>();

            Action<Web, int> assignCutters = (w, numCutters) =>
            {
                cutPairs[w] = unusedCutters.Take(numCutters);
                unusedCutters = unusedCutters.Skip(numCutters);
            };
            foreach (var web in targetWebs)
            {
                var webLength = API.webLength(web);
                if (webLength > 20)
                {
                    assignCutters(web, 1);
                }
            }

            foreach (var entry in cutPairs)
            {
                entry.Value.Select(spi => (Cutter)spi).ForEach(cutter => cutter.Cut(entry.Key));
            }

            return unusedCutters;
        }

        public static IEnumerable<Spider> SpreadCuttersNest(Nest nest)
        {
            var closeEnemySpiders = API.getEnemySpidersNearNest(nest, 20);
            var closeEnemyNests = closeEnemySpiders.Select(spi => spi.Nest).ToHashSet();

            var bm = Smarts.Game.CurrentPlayer.BroodMother;
            var nestQuota = (bm.Nest == nest ? 10 : 0) + 5 * closeEnemyNests.Count();

            var idleCutters = nest.Spiders.Where(spi => Smarts.Game.CurrentPlayer == spi.Owner).Where(spi => spi != bm).Select(spi => (Spiderling)spi).Where(s => s.WorkRemaining == 0);
            var quoteSpiders = idleCutters.Where(spider => spider.Nest == nest).Take(nestQuota);
            var unquoteSpiders = idleCutters.Where(spi => !quoteSpiders.Contains(spi));

            var enemyWebs = Smarts.TheirSpiderlings.Where(spi => spi.MovingToNest == nest).Where(spi => nest == spi.MovingToNest).Select(spi => spi.MovingOnWeb).OrderBy(API.webLength).Distinct();
            Console.WriteLine("EnemyWebs - " + (enemyWebs.Any() ? enemyWebs.First() : null ));
            var unusedCutters = cuttersCutWithSuicide(idleCutters, enemyWebs);

            //var targetWebs = Smarts.TheirSpiderlings.Where(spi => spi.MovingToNest == null).Where(spi => spi.Nest.Webs.Any(web => (web.NestA == nest || web.NestB == nest)))
            //    .Select(spi => Smarts.Webs[Tuple.Create(nest.ToPoint(), spi.MovingToNest.ToPoint())]).ToHashSet();
            //unusedCutters = cuttersCut(unusedCutters, targetWebs);

            return unquoteSpiders;
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
                        if (web.Load < Math.Min(2, web.Strength))
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

        public static Tuple<IEnumerable<Point>, int> CalcPath(Point start, Func<Point, bool> isGoal)
        {
            var search = new AStar<Tuple<Point, int>>
                (
                    Tuple.Create(start, 0).Single(),
                    t => isGoal(t.Item1),
                    (t1, t2) => t2.Item2 - t1.Item2,
                    p => 0,
                    t => Smarts.Nests.Keys.Select(n => Tuple.Create(n, t.Item2 + API.movementTime(t.Item1.EDist(n)))).Where(t2 => t2.Item2 < 10 && WillHold(t.Item1, t2.Item1, t2.Item2))
                );

            return Tuple.Create(search.Path.Select(t => t.Item1), search.Path.Sum(t => t.Item2));
        }

        public static bool WillHold(Point start, Point end, int turnsFromNow)
        {
            var nestA = Smarts.Nests[start];
            var nestB = Smarts.Nests[end];
            Web web;
            if (Smarts.Webs.TryGetValue(Tuple.Create(start, end), out web))
            {
                var futureLoad = web.Spiderlings.Count(s => s.WorkRemaining > turnsFromNow);
                if (futureLoad >= Math.Min(2, web.Strength))
                {
                    return false;
                }
                var cutter = nestA.Spiders.Concat(nestB.Spiders).Where(s => s is Cutter).Select(s => s as Cutter).FirstOrDefault(c => c.CuttingWeb == web);
                if (cutter != null)
                {
                    return cutter.WorkRemaining / Math.Sqrt(cutter.NumberOfCoworkers) > turnsFromNow;
                }
                return true;
            }
            else
            {
                var spitter = nestA.Spiders.Concat(nestB.Spiders).Where(s => s is Spitter).Select(s => s as Spitter).FirstOrDefault(s => s.SpittingWebToNest == nestA || s.SpittingWebToNest == nestB);
                if (spitter != null)
                {
                    return spitter.WorkRemaining / Math.Sqrt(spitter.NumberOfCoworkers) <= turnsFromNow;
                }
                return false;
            }
        }

        public static void Assault(IEnumerable<Spiderling> lings, int count)
        {
            Smarts.Refresh();
            var goal = Smarts.Game.CurrentPlayer.OtherPlayer.BroodMother.Nest;
            var assaults = lings
                .Where(l => l.WorkRemaining == 0)
                .Select(l =>
                {
                    var path = CalcPath(l.ToPoint(), p => goal.ToPoint().Equals(p));
                    return new { Ling = l, Path = path.Item1, TurnCount = path.Item2 };
                })
                .Where(c => c.TurnCount > 0)
                .OrderBy(c => c.TurnCount)
                .Take(count);

            foreach(var assault in assaults)
            {
                //Console.WriteLine(assault.Ling.ToPoint());
                //assault.Path.ForEach(p => Console.Write("{0}->", p));
                //Console.WriteLine();
                var target = assault.Path.Nth(1);
                var web = Smarts.Webs[Tuple.Create(assault.Ling.ToPoint(), target)];
                if (web.Load < web.Strength)
                {
                    assault.Ling.Move(web);
                }
            }
        }
    }
}
