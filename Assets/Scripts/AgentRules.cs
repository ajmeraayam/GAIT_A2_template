using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Collections.Generic;

    public static class AgentRules
    {
        // Get all the possible/legal actions
        public static List<string> GetLegalActions(Tuple<int, int> playerPos, GameStateData stateData)
        {
            return Actions.GetPossibleActions(playerPos, stateData);
        }

        // Creates a new game state according to the given player position, current game state, that game state's data and given action
        public static GameState ApplyAction(Tuple<int, int> playerPos, GameState state, GameStateData stateData, string action)
        {
            // Get successor player position from the given player position and action
            Tuple<int, int> successorPlayerLoc = Actions.GetSuccessor(playerPos, action);
            // Updates the food list, soda list and health left for the player according to their position
            stateData.UpdateStateData(successorPlayerLoc);
            // Copies the current game state and updates the GameStateData according to the successor player position
            GameState successorState = new GameState(state, stateData, successorPlayerLoc);
            return successorState;
        }
    }
}
