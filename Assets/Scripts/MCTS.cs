using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class MCTS
    {
        private Dictionary<Tuple<int, int>, Tuple<float, int>> cumulativeReward;
        private Dictionary<Tuple<int, int>, Tuple<float, int>> meanReward;
        private Dictionary<Tuple<int, int>, int> visitCount;
        private Dictionary<GameState, List<GameState>> parent_child_mapping;
        private int depthThreshold;
        private float exploration_threshold;

        public MCTS()
        {
            cumulativeReward = new Dictionary<Tuple<int, int>, Tuple<float, int>>();
            meanReward = new Dictionary<Tuple<int, int>, Tuple<float, int>>();
            visitCount = new Dictionary<Tuple<int, int>, int>();
            parent_child_mapping = new Dictionary<GameState, List<GameState>>();
            depthThreshold = 15;
            exploration_threshold = (float) Math.Sqrt(2);
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
            if(depth > this.depthThreshold)
            {
                return false;
            }
            else
            {
                if(this.parent_child_mapping.ContainsKey(state))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private List<GameState> ExpandNode(GameState state, int depth)
        {
            List<GameState> childNodes = new List<GameState>();
            List<string> actions = state.GetLegalActions();
            actions.Remove("STOP");

            foreach(string action in actions)
            {
                GameState successor = state.GenerateSuccessor(action);
                childNodes.Add(successor);
            }

            return childNodes;
        }

        private void UpdateRewards(GameState state, float reward, int winReward)
        {
            float reward_cumulative = 0f;
            int win_cumulative = 0;
            float reward_mean = 0f;
            int win_mean = 0;
            
            Tuple<int, int> currentPos = state.GetPlayerPosition();
            if(this.cumulativeReward.ContainsKey(currentPos))
            {
                Tuple<float, int> cumulativeRew = this.cumulativeReward[currentPos];
                reward_cumulative = cumulativeRew.Item1;
                win_cumulative = cumulativeRew.Item2;
            }

            if(this.meanReward.ContainsKey(currentPos))
            {
                Tuple<float, int> meanRew = this.meanReward[currentPos];
                reward_mean = meanRew.Item1;
                win_mean = meanRew.Item2;
            }
            
            int visit = 0;
            if(this.visitCount.ContainsKey(currentPos))
            {
                visit = this.visitCount[currentPos];
            }
            visit++;

            reward_cumulative += reward;
            reward_mean = reward_cumulative / visit;
            win_cumulative = winReward;
            win_mean = win_cumulative;

            this.cumulativeReward[currentPos] = Tuple.Create(reward_cumulative, win_cumulative);
            this.meanReward[currentPos] = Tuple.Create(reward_mean, win_mean);
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
            float reward_cumulative = 0f;
            int win_cumulative = 0;
            float reward_mean = 0f;
            int win_mean = 0;
            
            int ni = 0;
            int np = this.visitCount[state.GetPlayerPosition()];

            Tuple<int, int> currentPos = child.GetPlayerPosition();
            if(this.cumulativeReward.ContainsKey(currentPos))
            {
                Tuple<float, int> cumulativeRew = this.cumulativeReward[currentPos];
                reward_cumulative = cumulativeRew.Item1;
                win_cumulative = cumulativeRew.Item2;
            }

            if(this.meanReward.ContainsKey(currentPos))
            {
                Tuple<float, int> meanRew = this.meanReward[currentPos];
                reward_mean = meanRew.Item1;
                win_mean = meanRew.Item2;
            }

            if(this.visitCount.ContainsKey(currentPos))
            {
                ni = this.visitCount[currentPos];
            }

            float vi = reward_mean;
            float uct = 0;

            if(ni == 0)
            {
                uct = float.PositiveInfinity;
            }
            else
            {
                uct = vi + this.exploration_threshold * (float) Math.Sqrt((float) Math.Log(np) / ni);
            }

            return uct;
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

        // Consider number of moves taken by player to expect an enemy move
        private float Evaluate(GameState prevGameState, GameState gameState, int depth)
        {
            List<Tuple<int, int>> currentFoodList = gameState.GetFood();
            List<Tuple<int, int>> prevFoodList = prevGameState.GetFood();
            List<Tuple<int, int>> currentSodaList = gameState.GetSoda();
            List<Tuple<int, int>> prevSodaList = prevGameState.GetSoda();
            List<Tuple<int, int>> currentEnemiesList = gameState.GetEnemies();
            List<Tuple<int, int>> prevEnemiesList = prevGameState.GetEnemies();

            int currentClosestEnemyDist = GetClosestEnemyDistance(gameState);
            int prevClosestEnemyDist = GetClosestEnemyDistance(prevGameState);
            int prevClosestFoodDist = GetClosestFoodDistance(prevGameState);
            int currentClosestFoodDist = GetClosestFoodDistance(gameState);
            int prevClosestSodaDist = GetClosestSodaDistance(prevGameState);
            int currentClosestSodaDist = GetClosestSodaDistance(gameState);
            int prevDistToExit = GetDistanceToExit(prevGameState);
            int currentDistToExit = GetDistanceToExit(gameState);

            float reward = 0f;
            // Consider empty enemy and food/soda list
            // Negative reward if moving close to enemy, positive if moving away from enemy
            if(currentClosestEnemyDist < 10000)
                reward += ((currentClosestEnemyDist - prevClosestEnemyDist) / (currentClosestEnemyDist * (depth + 1)));

            // If food is closer than soda (Case works for no soda remaining and only food remaining on board)
            if(prevClosestFoodDist < prevClosestSodaDist)
            {
                // Give reward for moving closer to food over soda
                // This means moving closer to food
                if(currentClosestFoodDist < prevClosestFoodDist)
                {
                    reward += (prevClosestFoodDist - currentClosestFoodDist) / (prevClosestFoodDist * (depth + 1));
                }
                // Moving away from food
                else
                {
                    // But moving close to soda
                    if(prevClosestSodaDist > currentClosestSodaDist)
                    {
                        // Using similar reward to food reward but dividing it further by 2
                        // because moving away from the closest food to move closer to soda is not very beneficial
                        reward += (prevClosestSodaDist - currentClosestSodaDist) / (prevClosestSodaDist * (depth + 1) * 2);
                    }
                    else
                    {
                        reward += (prevClosestFoodDist - currentClosestFoodDist) / (prevClosestFoodDist * (depth + 1));
                    }
                }
            }
            // If soda is closer than food (Case works for no food remaining and only soda remaining on board)
            else if(prevClosestFoodDist > prevClosestSodaDist)
            {
                // Give reward for moving closer to soda over food
                // This means moving closer to soda
                if(currentClosestSodaDist < prevClosestSodaDist)
                {
                    reward += 2 * ((prevClosestSodaDist - currentClosestSodaDist) / (prevClosestSodaDist * (depth + 1)));
                }
                // Moving away from soda
                else
                {
                    // But moving close to food
                    if(prevClosestFoodDist > currentClosestFoodDist)
                    {
                        // Using similar reward to soda reward but dividing it further by 2
                        // because moving away from the closest soda to move closer to food is not very beneficial
                        reward += (prevClosestFoodDist - currentClosestFoodDist) / (prevClosestFoodDist * (depth + 1) * 2);
                    }
                    else
                    {
                        reward += (prevClosestSodaDist - currentClosestSodaDist) / (prevClosestSodaDist * (depth + 1));
                    }
                }
            }
            // If both of them are on the board and at same distance
            else if(prevClosestFoodDist == prevClosestSodaDist && prevClosestFoodDist < 10000)
            {
                // Give reward for moving closer to soda over food
                // If moving into this state leads to eating soda
                if(prevSodaList.Count > currentSodaList.Count)
                {
                    reward += 20f;
                }
                // If moving into this tate leads to eating food
                else if(prevFoodList.Count > currentFoodList.Count)
                {
                    reward += 10f;
                }
                // If moving close to food
                else if(currentClosestFoodDist < currentClosestSodaDist)
                {
                    reward += (prevClosestFoodDist - currentClosestFoodDist) / (prevClosestFoodDist * (depth + 1));
                }
                // If moving close to soda
                else if(currentClosestFoodDist > currentClosestSodaDist)
                {
                    reward += 2 * ((prevClosestSodaDist - currentClosestSodaDist) / (prevClosestSodaDist * (depth + 1)));
                }
                // If distance remains same
                else
                {
                    reward += 0;
                }
            }
            // If no food or soda remaining on board
            else if(prevClosestFoodDist == prevClosestSodaDist && prevClosestFoodDist == 10000)
            {
                // If moving closer or keeping same distance to exit
                if(prevDistToExit >= currentDistToExit)
                {
                    reward += 1;
                }
                else
                {
                    reward += 0;
                }
            }

            return reward;
        }

        private int GetClosestEnemyDistance(GameState state)
        {
            List<Tuple<int, int>> enemiesList = state.GetEnemies();
            Tuple<int, int> playerPos = state.GetPlayerPosition();

            int distance = 10000;
            foreach(Tuple<int, int> enemy in enemiesList)
            {
                int dist = DistanceCalculator.ManhattanDistance(enemy, playerPos);
                if(dist < distance)
                {
                    distance = dist;
                }
            }

            return distance;
        }

        private int GetClosestFoodDistance(GameState state)
        {
            List<Tuple<int, int>> foodList = state.GetFood();
            Tuple<int, int> playerPos = state.GetPlayerPosition();

            int distance = 10000;
            foreach(Tuple<int, int> food in foodList)
            {
                int dist = DistanceCalculator.ManhattanDistance(food, playerPos);
                if(dist < distance)
                {
                    distance = dist;
                }
            }

            return distance;
        }

        private int GetClosestSodaDistance(GameState state)
        {
            List<Tuple<int, int>> sodaList = state.GetSoda();
            Tuple<int, int> playerPos = state.GetPlayerPosition();

            int distance = 10000;
            foreach(Tuple<int, int> soda in sodaList)
            {
                int dist = DistanceCalculator.ManhattanDistance(soda, playerPos);
                if(dist < distance)
                {
                    distance = dist;
                }
            }

            return distance;
        }

        private int GetDistanceToExit(GameState state)
        {
            Tuple<int, int> exit = state.GetExitLoc();
            Tuple<int, int> playerPos = state.GetPlayerPosition();

            return DistanceCalculator.ManhattanDistance(exit, playerPos);

        }
    }
}
