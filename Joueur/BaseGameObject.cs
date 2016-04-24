using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joueur.cs
{
    abstract class BaseGameObject
    {
        /// <summary>
        /// A unique identifier for each game object. During any game IDs will never be re-used.
        /// </summary>
        public string Id { get; protected set; }

        protected T RunOnServer<T>(string functionName, IDictionary<string, object> args = null)
        {
            if (Joueur.cs.Games.Spiders.Smarts.Game.CurrentPlayer.TimeRemaining < 300000000)
            {
                throw new Exception("ACK");
            }
            Console.WriteLine("Action - {0}", functionName);
            return Client.Instance.RunOnServer<T>(this, functionName, args);
        }
    }
}
