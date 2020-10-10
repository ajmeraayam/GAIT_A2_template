using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class GameStateData
    {
        private List<Tuple<int, int>> floorLoc;
        public List<Tuple<int, int>> FloorLoc { get { return floorLoc; } }
        private int healthLeft;
        public int HealthLeft { get { return healthLeft; } }
        private List<Tuple<int, int>> foodLoc;
        public List<Tuple<int, int>> FoodLoc { get { return foodLoc; } }
        private List<Tuple<int, int>> sodaLoc;
        public List<Tuple<int, int>> SodaLoc { get { return sodaLoc; } }
        private List<Tuple<int, int>> breakableWallsLoc;
        public List<Tuple<int, int>> BreakableWallsLoc { get { return breakableWallsLoc; } }
        private Tuple<int, int> exitLoc;
        public Tuple<int, int> ExitLoc { get { return exitLoc; } }
        private List<Tuple<int, int>> enemiesLoc;
        public List<Tuple<int, int>> EnemiesLoc { get { return enemiesLoc; } }

        public GameStateData(List<Tuple<int, int>> floorLoc, List<Tuple<int, int>> foodLoc, List<Tuple<int, int>> sodaLoc, List<Tuple<int, int>> breakableWallsLoc, List<Tuple<int, int>> enemiesLoc, Tuple<int, int> exitLoc, int healthLeft)
        {
            this.floorLoc = floorLoc;
            this.foodLoc = foodLoc;
            this.sodaLoc = sodaLoc;
            this.breakableWallsLoc = breakableWallsLoc;
            this.exitLoc = exitLoc;
            this.enemiesLoc = enemiesLoc;
            this.healthLeft = healthLeft;
        }

        // Copy Constructor
        public GameStateData(GameStateData stateData)
        {
            this.floorLoc = new List<Tuple<int, int>>(stateData.FloorLoc);
            this.foodLoc = new List<Tuple<int, int>>(stateData.FoodLoc);
            this.sodaLoc = new List<Tuple<int, int>>(stateData.SodaLoc);
            this.breakableWallsLoc = new List<Tuple<int, int>>(stateData.BreakableWallsLoc);
            this.exitLoc = Tuple.Create(stateData.ExitLoc.Item1, stateData.ExitLoc.Item2);
            this.enemiesLoc = new List<Tuple<int, int>>(stateData.EnemiesLoc);
            this.healthLeft = stateData.HealthLeft;
        }

        public void UpdateStateData(Tuple<int, int> playerPos)
        {
            if(this.sodaLoc.Any(tup => tup.Item1 == playerPos.Item1 && tup.Item2 == playerPos.Item2))
            {
                this.sodaLoc.RemoveAll(tup => tup.Item1 == playerPos.Item1 && tup.Item2 == playerPos.Item2);
            }

            if(this.foodLoc.Any(tup => tup.Item1 == playerPos.Item1 && tup.Item2 == playerPos.Item2))
            {
                this.foodLoc.RemoveAll(tup => tup.Item1 == playerPos.Item1 && tup.Item2 == playerPos.Item2);
            }
            this.healthLeft--;
        }

        public bool CompareData(GameStateData other)
        {
            if(this.foodLoc.Count != other.FoodLoc.Count || !this.foodLoc.Except(other.FoodLoc).ToList().Any())
                return false;
            if(this.sodaLoc.Count != other.SodaLoc.Count || !this.sodaLoc.Except(other.SodaLoc).ToList().Any())
                return false;
            if(this.breakableWallsLoc.Count != other.BreakableWallsLoc.Count || !this.breakableWallsLoc.Except(other.BreakableWallsLoc).ToList().Any())
                return false;
            if(this.enemiesLoc.Count != other.EnemiesLoc.Count || !this.enemiesLoc.Except(other.EnemiesLoc).ToList().Any())
                return false;
            
            return true;
        }
    }
}
