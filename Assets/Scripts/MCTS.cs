using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class MCTS
    {
        Dictionary<Tuple<int, int>, Tuple<float, int>> cumulativeReward;
        Dictionary<Tuple<int, int>, Tuple<float, int>> meanReward;
        Dictionary<Tuple<int, int>, int> visitCount;
        Dictionary<GameState, List<GameState>> parent_child_mapping;
        private int depthThreshold;

        public MCTS()
        {
            cumulativeReward = new Dictionary<Tuple<int, int>, Tuple<float, int>>();
            meanReward = new Dictionary<Tuple<int, int>, Tuple<float, int>>();
            visitCount = new Dictionary<Tuple<int, int>, int>();
            parent_child_mapping = new Dictionary<GameState, List<GameState>>();
            depthThreshold = 15;
        }

        private Tuple<GameState, float, int> TreeSim(GameState state, int depth)
        {
            if(depth > this.depthThreshold)
            {
                return this.Playout(state, depth);
            }

            else if(this.IsExpandable(state, depth))
            {
                this.parent_child_mapping[state] = this.ExpandNode(state, depth);
                Tuple<GameState, float, int> selectedChild = this.Playout(state, depth);
                this.UpdateRewards(selectedChild.Item1, selectedChild.Item2, selectedChild.Item3);
                this.UpdateRewards(state, selectedChild.Item2, selectedChild.Item3);
                return Tuple.Create(state, selectedChild.Item2, selectedChild.Item3);
            }
            else
            {
                // Check the win condition as well
                GameState childState = this.Select_child(state, depth);
                if(childState.CheckEnemyOnCurrentLoc())
                {
                    Tuple<GameState, float, int> child = this.Playout(state, depth);
                    this.UpdateRewards(state, child.Item2, child.Item3);
                    return Tuple.Create(state, child.Item2, child.Item3);
                }
                Tuple<int, int> currentPos = state.GetPlayerPosition();
                Tuple<int, int> childPos = childState.GetPlayerPosition();
                int delta_l = DistanceCalculator.ManhattanDistance(currentPos, childPos);
                Tuple<GameState, float, int> next_iter = this.TreeSim(childState, depth + delta_l);
                this.UpdateRewards(state, next_iter.Item2, next_iter.Item3);
                return Tuple.Create(state, next_iter.Item2, next_iter.Item3);
            }
        }

        private bool IsExpandable(GameState state, int depth)
        {
            return false;
        }

        private List<GameState> ExpandNode(GameState state, int depth)
        {
            return new List<GameState>();
        }

        private void UpdateRewards(GameState state, float reward, int winReward)
        {

        }

        private GameState Select_child(GameState state, int depth)
        {
            List<GameState> children = this.parent_child_mapping[state];
            List<Tuple<GameState, float>> uctList = new List<Tuple<GameState, float>>();

            // GameState ignore_state = null;
            foreach(GameState child in children)
            {
                float uct = this.CalculateUCT(state, child, depth);
                uctList.Add(Tuple.Create(child, uct));
            }
            
            if(uctList.Count == 0)
            {
                return children[0];
            }

            float maxUCT = uctList.Max(tup => tup.Item2);
            List<Tuple<GameState, float>> maxList = new List<Tuple<GameState, float>>();
            foreach(Tuple<GameState, float> tup in uctList)
            {
                if(tup.Item2 == maxUCT)
                    maxList.Add(tup);
            }

            if(maxList.Count == 1)
            {
                return maxList[0].Item1;
            }
            else
            {
                var random = new Random();
                int randIndex = random.Next(maxList.Count);
                return maxList[randIndex].Item1;
            }
        }

        private float CalculateUCT(GameState state, GameState child, int depth)
        {
            return 0f;
        }

        private Tuple<GameState, float, int> Playout(GameState state, int depth)
        {
            GameState nextState = state;
            GameState child = null;
            int playoutDepth = 0;
            float reward = 0f;

            while(!nextState.IsOver() && !nextState.CheckEnemyOnCurrentLoc())
            {
                Tuple<GameState, float, int> selectedState = this.PlayoutSim(nextState, depth + playoutDepth);
                if(playoutDepth == 0)
                    child = selectedState.Item1;
                reward += selectedState.Item2;
                playoutDepth++;
            }
            int winReward = 0;

            if(nextState.CheckEnemyOnCurrentLoc())
                winReward = -1;
            else if(nextState.IsOver())
                winReward = 1;
            
            reward /= playoutDepth;

            return Tuple.Create(child, reward, winReward);
        }

        private Tuple<GameState, float, int> PlayoutSim(GameState state, int depth)
        {
            List<GameState> childNodes = null;
            if(!this.parent_child_mapping.ContainsKey(state))
            {
                childNodes = this.ExpandNode(state, depth);
            }
            else
            {
                childNodes = this.parent_child_mapping[state];
            }

            List<Tuple<GameState, float, int>> rewardList = new List<Tuple<GameState, float, int>>();

            foreach(GameState child in childNodes)
            {
                if(child.CheckEnemyOnCurrentLoc())
                {
                    rewardList.Add(Tuple.Create(child, -10000f, -1));
                    continue;
                }
                if(child.IsOver())
                {
                    rewardList.Add(Tuple.Create(child, 10000f, 1));
                }
                float reward = this.PlayoutEvaluation(state, child, depth);
                rewardList.Add(Tuple.Create(child, reward, 0));
            }

            if(rewardList.Count == 0)
            {
                var random = new Random();
                int randIndex = random.Next(childNodes.Count);
                return Tuple.Create(childNodes[randIndex], 0f, 0);
            }
            if(rewardList.Count == 1)
                return rewardList[0];
            
            // Sort descending by item3, if two tuples have same item3 value then sort descending by item2
            rewardList.Sort((x, y) => {
                int result = y.Item3.CompareTo(x.Item3);
                return result == 0 ? y.Item2.CompareTo(x.Item2) : result; 
            });
            return rewardList[0];
        }

        private float PlayoutEvaluation(GameState prevGameState, GameState gameState, int depth)
        {
            return Evaluate(prevGameState, gameState, depth);
        }

        private float Evaluate(GameState prevGameState, GameState gameState, int depth)
        {
            return 0f;
        }
    }
}
