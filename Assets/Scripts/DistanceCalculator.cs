using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class DistanceCalculator
    {
        private Dictionary<Tuple<Tuple<int, int>, Tuple<int, int>>, int> distances;
        private List<Tuple<int, int>> floorLoc;
        private List<Tuple<int, int>> breakableWallsLoc;
        private Dictionary<Tuple<int, int>, int> dist = new Dictionary<Tuple<int, int>, int>();

        public DistanceCalculator(List<Tuple<int, int>> floorLoc, List<Tuple<int, int>> breakableWallsLoc)
        {
            this.floorLoc = floorLoc;
            this.breakableWallsLoc = breakableWallsLoc;
            RemoveWallsFromFloor();
            distances = new Dictionary<Tuple<Tuple<int, int>, Tuple<int, int>>, int>();
            foreach(Tuple<int, int> location in this.floorLoc)
            {
                this.dist[location] = 10000;
            }
        }

        public void CalculateDistances()
        {
            foreach(Tuple<int, int> source in this.floorLoc)
            {
                Dictionary<Tuple<int, int>, int> dist_copy = new Dictionary<Tuple<int, int>, int>(dist);
                Dictionary<Tuple<int, int>, bool> closed = new Dictionary<Tuple<int, int>, bool>();
                
                PriorityQueue<Tuple<int, int>> pQueue = new PriorityQueue<Tuple<int, int>>(true);
                pQueue.Enqueue(source, 0);
                dist_copy[source] = 0;

                while(pQueue.Count > 0)
                {
                    Tuple<int, int> node = pQueue.Dequeue();
                    // If the popped location is in closed then skip the iteration
                    if(closed.ContainsKey(node))
                        continue;
                    
                    closed[node] = true;
                    int nodeDist = dist_copy[node];
                    List<Tuple<int, int>> adjacent = new List<Tuple<int, int>>();
                    int x = node.Item1;
                    int y = node.Item2;

                    // if adjacent locations are explorable, then add them to the list
                    if(IsExplorableFloor(x, y + 1))
                        adjacent.Add(Tuple.Create(x, y+1));
                    if(IsExplorableFloor(x, y - 1))
                        adjacent.Add(Tuple.Create(x, y-1));
                    if(IsExplorableFloor(x + 1, y))
                        adjacent.Add(Tuple.Create(x+1, y));
                    if(IsExplorableFloor(x - 1, y))
                        adjacent.Add(Tuple.Create(x-1 , y));

                    foreach(Tuple<int, int> other in adjacent)
                    {
                        if(!dist_copy.ContainsKey(other))
                            continue;
                        
                        int oldDist = dist_copy[other];
                        int newDist = nodeDist + 1;
                        if(newDist < oldDist)
                        {
                            dist_copy[other] = newDist;
                            pQueue.Enqueue(other, newDist);
                        }

                    }
                }

                foreach(Tuple<int, int> target in this.floorLoc)
                {
                    Tuple<Tuple<int, int>, Tuple<int, int>> combination = Tuple.Create(source, target);
                    this.distances[combination] = dist_copy[target];
                }
            }
        }

        public int GetMazeDistance(Tuple<int, int> loc1, Tuple<int, int> loc2)
        {
            Tuple<Tuple<int, int>, Tuple<int, int>> source_dest_tuple = Tuple.Create(loc1, loc2);

            return this.distances[source_dest_tuple];
        }

        private void RemoveWallsFromFloor()
        {
            foreach(Tuple<int, int> location in this.breakableWallsLoc)
            {
                if(this.floorLoc.Any(tup => tup.Item1 == location.Item1 && tup.Item2 == location.Item2))
                {
                    this.floorLoc.RemoveAll(tup => tup.Item1 == location.Item1 && tup.Item2 == location.Item2);
                }
            }
        }

        private bool IsExplorableFloor(int x, int y)
        {
            if(this.floorLoc.Any(tup => tup.Item1 == x && tup.Item2 == y))
                return true;
            else
                return false;
        }

        public static int ManhattanDistance(Tuple<int, int> pos1, Tuple<int, int> pos2)
        {
            return (Math.Abs(pos1.Item1 - pos2.Item1) + Math.Abs(pos1.Item2 - pos2.Item2));
        }
    }
}