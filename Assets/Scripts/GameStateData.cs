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

        /*public GameStateData DeepCopy()
        {
            List<Tuple<int, int>> floor = new List<Tuple<int, int>>(this.floorLoc);
            List<Tuple<int, int>> food = new List<Tuple<int, int>>(this.foodLoc);
            List<Tuple<int, int>> soda = new List<Tuple<int, int>>(this.sodaLoc);
            List<Tuple<int, int>> breakableWalls = new List<Tuple<int, int>>(this.breakableWallsLoc);
            List<Tuple<int, int>> enemies = new List<Tuple<int, int>>(this.enemiesLoc);
            Tuple<int, int> exit = Tuple.Create(this.exitLoc.Item1, this.exitLoc.Item2);
            GameStateData state = new GameStateData(floor, food, sodaLoc, breakableWalls, enemies, exit);
            return state;
        }*/

        public void UpdateStateData(Tuple<int, int> playerPos)
        {
            if(this.foodLoc.Contains(playerPos))
            {
                this.foodLoc.Remove(playerPos);
            }
            if(this.sodaLoc.Contains(playerPos))
            {
                this.sodaLoc.Remove(playerPos);
            }
            this.healthLeft--;
        }

        /*public override bool Equals(object obj)
        {
            return this.Equals(obj as GameStateData);
        }

        public bool Equals(GameStateData other)
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
        }*/

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

        /*public override int GetHashCode()
        {
            int hash = this.healthLeft;
            if(this.enemiesLoc.Count != 0)
            {
                hash = (hash * 23) + this.enemiesLoc.Count;
                foreach(Tuple<int, int> tup in this.enemiesLoc)
                {
                    hash *= 23;
                    if(tup != null)
                        hash += tup.GetHashCode();
                }
            }
            return hash;
        }

        public override int GetHashCode()
        {
            return 1;
        }*/
    }
}
