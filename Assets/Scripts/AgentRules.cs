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

        public static GameState ApplyAction(Tuple<int, int> playerPos, GameState state, GameStateData stateData, string action)
        {
            //List<string> legal = GetLegalActions(playerPos, stateData);
            Tuple<int, int> successorPlayerLoc = Actions.GetSuccessor(playerPos, action);
            // Updates the food list, soda list and health left for the player according to their position
            stateData.UpdateStateData(successorPlayerLoc);
            GameState successorState = new GameState(state, stateData);
            return successorState;
        }
    }
}
