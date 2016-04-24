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
            var player = state.Players[state.CurrentPlayer];
            var broodMother = state.Spiders[player.BroodMother];
            var broodNest = state.Nests[broodMother.Nest];

            var search = new AStar<XNest>
                (
                    broodNest.Single(),
                    n => false,
                    (n1, n2) => API.movementTime(n1.Location.EDist(n2.Location)),
                    n => 0,
                    n1 => state.Nests.Values.Where(n2 => API.movementTime(n1.Location.EDist(n2.Location)) % 2 == 1)
                );

            return search.From.Select(kvp => Tuple.Create(kvp.Value.Key, kvp.Key.Key));
        }

        public static IEnumerable<XAction> mobilizeSpitters(XState state, IEnumerable<Tuple<int, int>> wantedWebs)
        {
            var existingWebs = state.Webs.Values.Select(w => Tuple.Create(w.NestA, w.NestB)).ToHashSet();
            var websThatDontExist = wantedWebs.Where(t => !existingWebs.Contains(Tuple.Create(t.Item1, t.Item2)));
            var idleSpitters = state.Players[state.CurrentPlayer].Spitters
                .Select(s => state.Spiders[s])
                .Where(s => s.WorkRemaining == 0);
            var nestsWithIdleSpitters = idleSpitters.Select(s => s.Nest).ToHashSet();
            var connectedWebsToMake = websThatDontExist.Where(t => nestsWithIdleSpitters.Contains(t.Item1) || nestsWithIdleSpitters.Contains(t.Item2)).ToLazyList();
            foreach (var spitter in idleSpitters)
            {
                var webToMake = connectedWebsToMake.FirstOrDefault(t => spitter.Nest == t.Item1 || spitter.Nest == t.Item2);
                if (webToMake != null)
                {
                    yield return null;
                }
            }
        }
    }
}
