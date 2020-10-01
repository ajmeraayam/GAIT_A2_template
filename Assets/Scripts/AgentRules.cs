using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Collections.Generic;

    public static class AgentRules
    {
        public static List<string> GetLegalActions(Tuple<int, int> playerPos, GameStateData stateData)
        {
            return Actions.GetPossibleActions(playerPos, stateData);
        }

        public static void ApplyAction(GameStateData stateData, string action)
        {

        }
    }
}
