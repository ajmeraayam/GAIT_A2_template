using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class DistanceCalculator
    {
        // Stores the tuple of source and destination location as key and distance between them as value
        private Dictionary<Tuple<Tuple<int, int>, Tuple<int, int>>, int> distances;
        // List of all the locations for the floor prefab
        private List<Tuple<int, int>> floorLoc;
        // List of all the locations for the breakable walls prefab
        private List<Tuple<int, int>> breakableWallsLoc;
        // Used in calculating distances
        private Dictionary<Tuple<int, int>, int> dist = new Dictionary<Tuple<int, int>, int>();

        public DistanceCalculator(List<Tuple<int, int>> floorLoc, List<Tuple<int, int>> breakableWallsLoc)
        {
            this.floorLoc = floorLoc;
            this.breakableWallsLoc = breakableWallsLoc;
            // Removes all the breakable walls from the floor list
            RemoveWallsFromFloor();
            distances = new Dictionary<Tuple<Tuple<int, int>, Tuple<int, int>>, int>();
            // Add all the floor locations in the dictionary and assign them a default max value
            foreach(Tuple<int, int> location in this.floorLoc)
            {
                this.dist[location] = 10000;
            }
        }

        // Calculates distance between all the floor locations and stores them in a dictionary
        public void CalculateDistances()
        {
            // For all the location in the floor list
            foreach(Tuple<int, int> source in this.floorLoc)
            {
                Dictionary<Tuple<int, int>, int> dist_copy = new Dictionary<Tuple<int, int>, int>(dist);
                Dictionary<Tuple<int, int>, bool> closed = new Dictionary<Tuple<int, int>, bool>();
                // Priority queue for Uniform Cost Search
                PriorityQueue<Tuple<int, int>> pQueue = new PriorityQueue<Tuple<int, int>>(true);
                // Enqueue the source location in the priority queue
                pQueue.Enqueue(source, 0);
                // Set distance to itself as 0
                dist_copy[source] = 0;

                while(pQueue.Count > 0)
                {
                    Tuple<int, int> node = pQueue.Dequeue();
                    // If the popped location is in closed then skip the iteration
                    if(closed.ContainsKey(node))
                        continue;
                    
                    // Set the popped location as true in closed dictionary
                    closed[node] = true;
                    // Read the current value of the location from dist dictionary
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

                    // Update the adjacent locations in the 'dist' dictionary, if the new distance is less than currently recorded distance 
                    foreach(Tuple<int, int> other in adjacent)
                    {
                        if(!dist_copy.ContainsKey(other))
                            continue;
                        
                        int oldDist = dist_copy[other];
                        int newDist = nodeDist + 1;
                        // If new distance is less than currently recorded distance then enqueue the location and update the dictionary
                        if(newDist < oldDist)
                        {
                            dist_copy[other] = newDist;
                            pQueue.Enqueue(other, newDist);
                        }

                    }
                }
                // For all the location in the floor list, update the 'distances' dictionary with the actual maze distance
                foreach(Tuple<int, int> target in this.floorLoc)
                {
                    Tuple<Tuple<int, int>, Tuple<int, int>> combination = Tuple.Create(source, target);
                    this.distances[combination] = dist_copy[target];
                }
            }
        }

        // Get the actual maze distance between given source and destination location
        public int GetMazeDistance(Tuple<int, int> loc1, Tuple<int, int> loc2)
        {
            Tuple<Tuple<int, int>, Tuple<int, int>> source_dest_tuple = Tuple.Create(loc1, loc2);

            return this.distances[source_dest_tuple];
        }

        // Remove breakable walls location from the floor list
        private void RemoveWallsFromFloor()
        {
            // If given breakable wall location exists in the floor list then remove that location from the list
            foreach(Tuple<int, int> location in this.breakableWallsLoc)
            {
                if(this.floorLoc.Any(tup => tup.Item1 == location.Item1 && tup.Item2 == location.Item2))
                {
                    this.floorLoc.RemoveAll(tup => tup.Item1 == location.Item1 && tup.Item2 == location.Item2);
                }
            }
        }

        // If floor location exists in the list then returns true, else false
        private bool IsExplorableFloor(int x, int y)
        {
            if(this.floorLoc.Any(tup => tup.Item1 == x && tup.Item2 == y))
                return true;
            else
                return false;
        }

        // Returns manhattan distance between given source and destination location
        public static int ManhattanDistance(Tuple<int, int> pos1, Tuple<int, int> pos2)
        {
            return (Math.Abs(pos1.Item1 - pos2.Item1) + Math.Abs(pos1.Item2 - pos2.Item2));
        }
    }
}