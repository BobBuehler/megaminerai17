using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs.Games.Spiders
{
    static class Simulator
    {
        public static void Act(XState state, XAction action)
        {
            switch (action.Type)
            {
                case XActionType.Consume:
                    RemoveSpider(state, action.TargetSpider);
                    break;
                case XActionType.Spawn:
                    AddSpider(state, action.Actor, action.SpawnType);
                    break;
                case XActionType.Attack:
                    AttackSpider(state, action.Actor, action.TargetSpider);
                    break;
                case XActionType.Move:
                    MoveSpider(state, action.Actor, action.TargetWeb);
                    break;
                case XActionType.Cut:
                    CutWeb(state, action.Actor, action.TargetWeb);
                    break;
                case XActionType.Spit:
                    SpitToNest(state, action.Actor, action.TargetNest);
                    break;
                case XActionType.Weave:
                    WeaveWeb(state, action.Actor, action.TargetWeb);
                    break;
            }
        }

        public static void RemoveSpider(XState state, XSpider spider)
        {
            state.Spiders.Remove(spider.Key);
            RemoveSpider(state.Players.Values.SelectMany(p => new [] {p.Cutters, p.Spitters, p.Weavers}), spider.Key);
            RemoveSpider(state.Spiders.Values.Select(s => s.Coworkers), spider.Key);
            RemoveSpider(state.Nests.Values.Select(n => n.Spiders), spider.Key);
            RemoveSpider(state.Webs.Values.Select(w => w.Spiders), spider.Key);
        }

        public static void RemoveSpider(IEnumerable<HashSet<int>> refs, int spiderKey)
        {
            refs.ForEach(r => r.Remove(spiderKey));
        }

        public static void AddSpider(XState state, XSpider broodMother, XSpiderType type)
        {
            AddSpider(state, new XSpider(type, broodMother.Nest, broodMother.Owner));
        }

        public static void AddSpider(XState state, XSpider spider)
        {
            state.Spiders.Add(spider.Key, spider);
            AddSpider(state.Players[spider.Owner], spider);
            state.Nests[spider.Nest].Spiders.Add(spider.Key);
        }

        public static void AddSpider(XPlayer player, XSpider spider)
        {
            switch(spider.Type)
            {
                case XSpiderType.Cutter:
                    player.Cutters.Add(spider.Key);
                    break;
                case XSpiderType.Spitter:
                    player.Spitters.Add(spider.Key);
                    break;
                case XSpiderType.Weaver:
                    player.Weavers.Add(spider.Key);
                    break;
            }
        }

        public static void AttackSpider(XState state, XSpider attacker, XSpider target)
        {
            if (attacker.Type == target.Type)
            {
                // Kill both
                RemoveSpider(state, attacker);
                RemoveSpider(state, target);
            }
            else if (API.canAttackKind(attacker, target))
            {
                // Kill target
                RemoveSpider(state, target);
            }
            else
            {
                // Kill attacker
                RemoveSpider(state, attacker);
            }
        }

        public static void MoveSpider(XState state, XSpider mover, XWeb target)
        {
            // Remove this XSpider from its XNest (don't need to do anything with Coworkers)
            state.Nests[mover.Nest].Spiders.Remove(mover.Key);

            // Put this XSpider on the target XWeb
            mover.MovingOnWeb = target.Key;

            // Point this XSpider at the target XNest
            mover.MovingToNest = API.getNextNest(state, state.Nests[mover.Nest], target).Key;
            
            // Update this XSpider's movement time
            mover.WorkRemaining = API.movementTime(target.Length);

            // Delete this XSpider's reference to its XNest
            mover.Nest = -1;
        }

        public static void CutWeb(XState state, XSpider cutter, XWeb target)
        {
            //coworkers
            cutter.Coworkers = API.findCoworkers(state, cutter);
            if(!cutter.Coworkers.Any())//If there are no coworkers, calculate work remaining
            {
                cutter.WorkRemaining = API.cutWorkRemaining(target.Strength, target.Length);
            }
            else
            {
                cutter.WorkRemaining = state.Spiders[cutter.Coworkers.First()].WorkRemaining;
            }

            cutter.Coworkers.Add(cutter.Key);//Add yourself to the list (it may already contain you, but it doesn't hurt to do it again.
            
            foreach (var worker in cutter.Coworkers)//Add self as coworker to each coworker
            {
                state.Spiders[worker].Coworkers.Add(cutter.Key);
            }           
        }

        public static void SpitToNest(XState state, XSpider mover, XNest target)
        {

        }

        public static void WeaveWeb(XState state, XSpider mover, XWeb target)
        {

        }

        public static void AddWeb(XState state, int nestA, int nestB)
        {
            var length = state.Nests[nestA].Location.EDist(state.Nests[nestB].Location);
            AddWeb(state, new XWeb(length, nestA, nestB, API.initialWebStrength()));
        }

        public static void AddWeb(XState state, XWeb web)
        {
            state.Webs[web.Key] = web;
            state.Nests[web.NestA].Webs.Add(web.Key);
            state.Nests[web.NestB].Webs.Add(web.Key);
        }

        public static void NextTurn(XState state)
        {
            var player = state.Players.Values.First(p => p.Key != state.CurrentPlayer);

            ProgressSpitters(state, state.Spiders.Values.Where(s => s.Type == XSpiderType.Spitter));
            ProgressCutters(state, state.Spiders.Values.Where(s => s.Type == XSpiderType.Cutter));
            ProgressWeavers(state, state.Spiders.Values.Where(s => s.Type == XSpiderType.Weaver));

            ProgressMoves(state, player.Spitters.Concat(player.Cutters).Concat(player.Weavers));
            // 2x this ProgressBroodMotherHealth(state, player.BroodMother);
            ProgressBroodMotherEggs(state, player.BroodMother);

            state.CurrentPlayer = player.Key;
            state.CurrentTurn++;
        }

        public static IEnumerable<HashSet<int>> DistinctSets(IEnumerable<HashSet<int>> sets)
        {
            var seen = new HashSet<int>();
            foreach (var set in sets.Where(s => s.Count > 0))
            {
                if (set.All(c => !seen.Contains(c)))
                {
                    set.ForEach(c => seen.Add(c));
                    yield return set;
                }
            }
        }

        public static void ProgressSpitters(XState state, IEnumerable<XSpider> spitters)
        {
            var clusters = DistinctSets(spitters.Select(s => s.Coworkers));
            clusters.ForEach(c => ProgressSpitterCluster(state, c.Select(k => state.Spiders[k])));
        }

        public static void ProgressSpitterCluster(XState state, IEnumerable<XSpider> spitters)
        {
            var workDone = API.workMultiplier(spitters.Count());
            var workRemaining = spitters.First().WorkRemaining - workDone;
            if (workRemaining > 0)
            {
                spitters.ForEach(s => s.WorkRemaining = workRemaining);
            }
            else
            {
                var spitter = spitters.First();
                AddWeb(state, spitter.Nest, spitter.SpittingToNest);

                spitters.ForEach(s => { s.Coworkers.Clear(); s.WorkRemaining = 0; });
            }
        }

        public static void ProgressCutters(XState state, IEnumerable<XSpider> cutters)
        {

        }

        public static void ProgressWeavers(XState state, IEnumerable<XSpider> weavers)
        {

        }

        public static void ProgressMoves(XState state, IEnumerable<int> spiders)
        {

        }

        public static void ProgressBroodMotherHealth(XState state, int broodMother)
        {

        }

        public static void ProgressBroodMotherEggs(XState state, int broodMother)
        {
            state.Spiders[broodMother].Eggs = API.newEggs(API.getNumSpiderlings(state.Players[state.CurrentPlayer]));
        }
    }
}
