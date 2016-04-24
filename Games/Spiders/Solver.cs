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
            XAction act = new XAction();
            act.Actor = attacker;
            act.Type = XActionType.Attack;
            act.TargetSpider = attackee;
            return act;
        }

        public static XAction generateAction(XSpider mover, XWeb path, XActionType actType)
        {
            XAction act = new XAction();
            act.Actor = mover;
            act.Type = actType;
            act.TargetWeb = path;
            return act;
        }

        public static XAction generateAction(XSpider spitter, XNest nest)
        {
            XAction act = new XAction();
            act.Actor = spitter;
            act.Type = XActionType.Spit;
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

    }
}
