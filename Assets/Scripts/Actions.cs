﻿using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Collections.Generic;

    public static class Actions
    {
        static Dictionary<string, Tuple<int, int>> directions = new Dictionary<string, Tuple<int, int>>() {{"NORTH", Tuple.Create(0, 1)}, {"SOUTH", Tuple.Create(0, -1)}, {"EAST", Tuple.Create(1, 0)}, {"WEST", Tuple.Create(-1, 0)}, {"STOP", Tuple.Create(0, 0)}};

        static List<Tuple<string, Tuple<int, int>>> directionList = DirectionToList();

        public static List<Tuple<string, Tuple<int, int>>> DirectionToList()
        {
            List<Tuple<string, Tuple<int, int>>> directionsToList = new List<Tuple<string, Tuple<int, int>>>(); 
            foreach(KeyValuePair<string, Tuple<int, int>> direction in directions)
            {
                directionsToList.Add(Tuple.Create(direction.Key, direction.Value));
            }

            return directionsToList;
        }

        public static Tuple<int, int> GetSuccessor(Tuple<int, int> playerPos, string action)
        {
            // X and Y of Player position
            int x = playerPos.Item1;
            int y = playerPos.Item2;
            // Vector for given action
            Tuple<int, int> vector = directions[action];
            // Return the next position according to the action
            return Tuple.Create(x + vector.Item1, y + vector.Item2);
        }

        public static string ReverseDirection(string action)
        {
            if(action.Equals("NORTH"))
                return "SOUTH";
            if(action.Equals("SOUTH"))
                return "NORTH";
            if(action.Equals("EAST"))
                return "WEST";
            if(action.Equals("WEST"))
                return "EAST";
            
            return action;
        }
        
        // Find all the possible/legal actions
        public static List<string> GetPossibleActions(Tuple<int, int> playerPos, GameStateData stateData)
        {
            // Current player position
            int playerX = playerPos.Item1;
            int playerY = playerPos.Item2;

            // Get the list of floor locations
            List<Tuple<int, int>> floorLoc = stateData.FloorLoc;
            // List of floor location for moving in each direction
            List<Tuple<string, Tuple<int, int>>> successorFloorLoc = new List<Tuple<string, Tuple<int, int>>>();
            List<string> legalDirections = new List<string>();
            
            // Adding direction vector to current player position
            for(int i = 0; i < directionList.Count; i++)
            {
                int x = directionList[i].Item2.Item1 + playerX;
                int y = directionList[i].Item2.Item2 + playerY;
                successorFloorLoc.Add(Tuple.Create(directionList[i].Item1, Tuple.Create(x, y)));
            }
            // If the action leading to next position is a floor (i.e. moveable gameobject), then add that
            // direction to the list of legal directions.
            for(int i = 0; i < successorFloorLoc.Count; i++)
            {
                if(floorLoc.Contains(successorFloorLoc[i].Item2))
                {
                    legalDirections.Add(successorFloorLoc[i].Item1);
                }
            }

            return legalDirections;
        }   
    }
}