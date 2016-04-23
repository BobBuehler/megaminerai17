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
            switch(action.Type)
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

        }

        public static void AttackSpider(XState state, XSpider attacker, XSpider target)
        {

        }

        public static void MoveSpider(XState state, XSpider mover, XWeb target)
        {

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
            
            ProgressSpitters(state, player.Spitters);
            ProgressCutters(state, player.Cutters);
            ProgressWeavers(state, player.Weavers);

            ProgressMoves(state, player.Spitters.Concat(player.Cutters).Concat(player.Weavers));
            ProgressBroodMotherHealth(state, player.BroodMother);
            ProgressBroodMotherEggs(state, player.BroodMother);

            state.CurrentPlayer = player.Key;
            state.CurrentTurn++;
        }

        public static void ProgressSpitters(XState state, IEnumerable<int> spiders)
        {

        }

        public static void ProgressCutters(XState state, IEnumerable<int> spiders)
        {

        }

        public static void ProgressWeavers(XState state, IEnumerable<int> spiders)
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

        }
    }
}
