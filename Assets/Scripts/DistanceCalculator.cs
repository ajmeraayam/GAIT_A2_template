using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Collections.Generic;

    public class DistanceCalculator
    {
        /*private Dictionary<Tuple<Tuple<int, int>, Tuple<int, int>>, int> distances;
        private List<Tuple<int, int>> floorLoc;
        private Dictionary<Tuple<int, int>, int> dist = new Dictionary<Tuple<int, int>, int>();

        public DistanceCalculator(List<Tuple<int, int>> floorLoc)
        {
            this.floorLoc = floorLoc;
            distances = new Dictionary<Tuple<Tuple<int, int>, Tuple<int, int>>, int>();
            for(int i = 0; i < this.floorLoc.Count; i++)
            {
                dist.Add(this.floorLoc[i], 100000);
            }
        }

        public void CalculateDistances()
        {
            for(int i = 0; i < this.floorLoc.Count; i++)
            {
                Tuple<int, int> loc1 = this.floorLoc[i];

                for(int j = 0; j < this.floorLoc.Count; j++)
                {
                    Tuple<int, int> loc2 = this.floorLoc[j];
                    bool pairExists = distances.ContainsKey(Tuple.Create(loc1, loc2));
                    bool reversePairExists = distances.ContainsKey(Tuple.Create(loc2, loc1));
                    if(!pairExists && !reversePairExists)
                    {
                        int distance = CalculateDistanceBetweenPoints(loc1, loc2);
                    }
                }
            }
        }

        private int CalculateDistanceBetweenPoints(Tuple<int, int> loc1, Tuple<int, int> loc2)
        {
            Dictionary<Tuple<int, int>, int> dist_copy = new Dictionary<Tuple<int, int>, int>(dist);
            Dictionary<Tuple<int, int>, int> closed = new Dictionary<Tuple<int, int>, int>();

            

            return 0;
        }*/

        public static int ManhattanDistance(Tuple<int, int> pos1, Tuple<int, int> pos2)
        {
            return (Math.Abs(pos1.Item1 - pos2.Item1) + Math.Abs(pos1.Item2 - pos2.Item2));
        }


    }
}