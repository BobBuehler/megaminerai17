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

        }

        public static void AddSpider(XState state, XSpider broodMother, XSpiderType type)
        {
            // Create XSpider with correct Type and Owner
            // Add to State
            // Add to Player
            // Add to Nest
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

        public static void CutWeb(XState state, XSpider mover, XWeb target)
        {

        }

        public static void SpitToNest(XState state, XSpider mover, XNest target)
        {

        }

        public static void WeaveWeb(XState state, XSpider mover, XWeb target)
        {

        }

        public static void NextTurn(XState state)
        {
            var player = state.Players.Values.First(p => p.Key != state.CurrentPlayer);

            //ProgressSpitters(state, state.Spiders.Where(s => s.);
            //ProgressCutters(state, player.Cutters);
            //ProgressWeavers(state, player.Weavers);

            ProgressMoves(state, player.Spitters.Concat(player.Cutters).Concat(player.Weavers));
            ProgressBroodMotherHealth(state, player.BroodMother);
            ProgressBroodMotherEggs(state, player.BroodMother);

            state.CurrentPlayer = player.Key;
            state.CurrentTurn++;
        }

        public static void ProgressSpitters(XState state, IEnumerable<XSpider> spitters)
        {
            // Cluster into coworker sets
            // Progress coworkers
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
