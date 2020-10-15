using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class MCTS
    {
        // Dictionary of cumulative rewards for each position on board
        private Dictionary<Tuple<int, int>, Tuple<float, int>> cumulativeReward;
        // Dictionary of mean rewards for each position on board
        private Dictionary<Tuple<int, int>, Tuple<float, int>> meanReward;
        // Dictionary of visit count for each position on board
        private Dictionary<Tuple<int, int>, int> visitCount;
        // Dictionary of all the successor states for a given game state
        private Dictionary<GameState, List<GameState>> parent_child_mapping;
        // Maximum allowed depth of the tree
        private int depthThreshold;
        // Exploration threshold used in UCT (Uniform Confidence Bound 1 applied to trees)
        private float exploration_threshold;
        // Number of times the Monte Carlo process is called (Selection, Expansion, Playout, Backpropagation)
        public int training_threshold;
        // Maximum number of steps that can be taken in playout phase
        private int playout_threshold;
        public bool processing;
        // Reference to DistanceCalculator object
        private DistanceCalculator distanceCalculator;

        public MCTS(DistanceCalculator distCalc)
        {
            cumulativeReward = new Dictionary<Tuple<int, int>, Tuple<float, int>>(new CustomTupleComparer());
            meanReward = new Dictionary<Tuple<int, int>, Tuple<float, int>>(new CustomTupleComparer());
            visitCount = new Dictionary<Tuple<int, int>, int>(new CustomTupleComparer());
            parent_child_mapping = new Dictionary<GameState, List<GameState>>(new CustomComparer());
            depthThreshold = 10;
            exploration_threshold = 10f;
            training_threshold = 40;
            playout_threshold = 1;              
            processing = false;
            this.distanceCalculator = distCalc;
        }
        
        // This method will call one iteration of MCTS which involves Selection, Expansion, Playout and 
        // backpropagation
        public void RunNextTrainingIteration(GameState gameState)
        {
            this.TreeSim(gameState, 0);
        }
        
        // Return the next direction that should be taken by the player, based on rewards that are calculated 
        public string GetNextDirection(GameState state)
        {
            // Get all the successor states for given game state.
            List<GameState> children = this.parent_child_mapping[state];
            List<Tuple<GameState, float, int>> childRewardList = new List<Tuple<GameState, float, int>>();

            // For all the successor game states, check the dictionary for rewards.
            // The player position in each successor state will be used to find the corresponding reward in the dictionary
            // Mean rewards are gathered for each state in reward list
            foreach(GameState child in children)
            {
                Tuple<int, int> child_loc = child.GetPlayerPosition();
                Tuple<float, int> child_reward;
                if(this.meanReward.ContainsKey(child_loc))
                    child_reward = this.meanReward[child_loc];
                else
                    child_reward = Tuple.Create(-30.0f, -1);
                childRewardList.Add(Tuple.Create(child, child_reward.Item1, child_reward.Item2));
            }

            // Sort the reward list by win reward and if same win reward, then sort by reward
            // Sorted in descending order
            childRewardList.Sort((x, y) => {
                int result = y.Item3.CompareTo(x.Item3);
                return result == 0 ? y.Item2.CompareTo(x.Item2) : result; 
            });
            // Select the successor state with best reward
            GameState selected_child = childRewardList[0].Item1;
            // Find the direction (north, south, east or west) from current and successor state
            return Actions.GetDirectionFromStates(state, selected_child);
        }

        // This method calculates an iteration of selection, expansion, playout and backpropagation
        // Returns the passed game state, food/enemy reward and win reward
        private Tuple<GameState, float, int> TreeSim(GameState state, int depth)
        {
            // If current depth exceeds depth threshold, then start playout from the current game state
            if(depth > this.depthThreshold)
            {
                return this.Playout(state, depth);
            }
            // If a game state is never expanded, then expand the game state and start playout from that state
            else if(this.IsExpandable(state, depth))
            {
                this.parent_child_mapping[state] = this.ExpandNode(state, depth);
                Tuple<GameState, float, int> selectedChild = this.Playout(state, depth);
                this.UpdateRewards(selectedChild.Item1, selectedChild.Item2, selectedChild.Item3);
                this.UpdateRewards(state, selectedChild.Item2, selectedChild.Item3);
                return Tuple.Create(state, selectedChild.Item2, selectedChild.Item3);
            }
            // If game state was already expanded before then select best child using UCT 
            // and recursively call this method with the selected child.
            else
            {
                // Select best child
                GameState childState = this.Select_child(state, depth);
                // If the selected child leads to death of the agent or the game ends, then start playout
                if(childState.CheckEnemyOnCurrentLoc() || childState.IsOver())
                {
                    Tuple<GameState, float, int> child = this.Playout(state, depth);
                    this.UpdateRewards(state, child.Item2, child.Item3);
                    return Tuple.Create(state, child.Item2, child.Item3);
                }
                Tuple<int, int> currentPos = state.GetPlayerPosition();
                Tuple<int, int> childPos = childState.GetPlayerPosition();
                // Use distance between current state and child state to measure the depth of tree
                int delta_l = this.distanceCalculator.GetMazeDistance(currentPos, childPos);
                Tuple<GameState, float, int> next_iter = this.TreeSim(childState, depth + delta_l);
                this.UpdateRewards(state, next_iter.Item2, next_iter.Item3);
                return Tuple.Create(state, next_iter.Item2, next_iter.Item3);
            }
        }

        // Returns true if game state was never expanded
        private bool IsExpandable(GameState state, int depth)
        {
            //Doesn't expand if depth exceeds depth threshold
            if(depth > this.depthThreshold)
            {
                return false;
            }
            else
            {
                // If dictionary contains the current game state as key, then it is not expandable
                // Since it has been expanded before
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

        // Expand a game state, store the successor states in a list and return that list
        private List<GameState> ExpandNode(GameState state, int depth)
        {
            List<GameState> childNodes = new List<GameState>();
            // Get all the actions that can be taken by the player according to the current position
            List<string> actions = state.GetLegalActions();
            // Remove STOP action from list. We don't want over agent to stop
            actions.Remove("STOP");

            // For each given action, create a successor game state and store it in a list
            foreach(string action in actions)
            {
                GameState successor = state.GenerateSuccessor(action);
                childNodes.Add(successor);
            }

            return childNodes;
        }

        // Update the reward dictionaries and increment visit count in the dictionary for the player position in the given game state
        private void UpdateRewards(GameState state, float reward, int winReward)
        {
            float reward_cumulative = 0f;
            int win_cumulative = 0;
            float reward_mean = 0f;
            int win_mean = 0;
            
            // Get the player position in the given game state
            Tuple<int, int> currentPos = state.GetPlayerPosition();
            // If this position is stored as a key in the cumulative reward dictionary, then retrieve the rewards related to that position
            if(this.cumulativeReward.ContainsKey(currentPos))
            {
                Tuple<float, int> cumulativeRew = this.cumulativeReward[currentPos];
                reward_cumulative = cumulativeRew.Item1;
                win_cumulative = cumulativeRew.Item2;
            }
            // If this position is stored as a key in the mean reward dictionary, then retrieve the rewards related to that position
            if(this.meanReward.ContainsKey(currentPos))
            {
                Tuple<float, int> meanRew = this.meanReward[currentPos];
                reward_mean = meanRew.Item1;
                win_mean = meanRew.Item2;
            }

            int visit = 0;
            // If this position is stored as a key in the visit count dictionary, then retrieve the visit count related to that position
            if(this.visitCount.ContainsKey(currentPos))
            {
                visit = this.visitCount[currentPos];
            }
            // Increment the visit count
            visit++;

            // Add the new reward in the cumulative reward. Calculate mean reward by dividing this reward by visit count
            reward_cumulative += reward;
            reward_mean = reward_cumulative / (float) visit;
            win_cumulative = winReward;
            win_mean = win_cumulative;

            // Update the cumulative and mean reward dictionaries and visit count dictionary
            this.cumulativeReward[currentPos] = Tuple.Create(reward_cumulative, win_cumulative);
            this.meanReward[currentPos] = Tuple.Create(reward_mean, win_mean);
            this.visitCount[currentPos] = visit;
        }

        // Select the best child using UCT formula
        private GameState Select_child(GameState state, int depth)
        {
            // Find all the child states for given game state
            List<GameState> children = this.parent_child_mapping[state];
            List<Tuple<GameState, float>> uctList = new List<Tuple<GameState, float>>();

            // Calculate UCT value for each child state
            foreach(GameState child in children)
            {
                float uct = this.CalculateUCT(state, child, depth);
                // Add the tuple of child state and corresponding UCT value in the list
                uctList.Add(Tuple.Create(child, uct));
            }

            // If uct list is empty, then select the first children
            if(uctList.Count == 0)
            {
                return children[0];
            }

            // Find the maximum UCT value from the list
            float maxUCT = uctList.Max(tup => tup.Item2);
            List<Tuple<GameState, float>> maxList = new List<Tuple<GameState, float>>();
            // And collect all the tuples that have maximum UCT value in a list
            foreach(Tuple<GameState, float> tup in uctList)
            {
                if(tup.Item2 == maxUCT)
                    maxList.Add(tup);
            }
            // If the maxList has only one entry then return that child state
            if(maxList.Count == 1)
            {
                return maxList[0].Item1;
            }
            // Else return a random child state from that list
            else
            {
                var random = new Random();
                int randIndex = random.Next(maxList.Count);
                return maxList[randIndex].Item1;
            }
        }

        // This method calculates UCT value for the given child state
        private float CalculateUCT(GameState state, GameState child, int depth)
        {
            float reward_cumulative = 0f;
            int win_cumulative = 0;
            float reward_mean = 0f;
            int win_mean = 0;
            
            int ni = 0;
            // Get the visit count of player position for parent game state
            int np = this.visitCount[state.GetPlayerPosition()];

            Tuple<int, int> currentPos = child.GetPlayerPosition();
            // If child position is stored as a key in the cumulative reward dictionary, then retrieve the rewards related to that position 
            if(this.cumulativeReward.ContainsKey(currentPos))
            {
                Tuple<float, int> cumulativeRew = this.cumulativeReward[currentPos];
                reward_cumulative = cumulativeRew.Item1;
                win_cumulative = cumulativeRew.Item2;
            }
            // If child position is stored as a key in the mean reward dictionary, then retrieve the rewards related to that position
            if(this.meanReward.ContainsKey(currentPos))
            {
                Tuple<float, int> meanRew = this.meanReward[currentPos];
                reward_mean = meanRew.Item1;
                win_mean = meanRew.Item2;
            }
            // If child position is stored as a key in the visit count dictionary, then retrieve the visit count related to that position
            if(this.visitCount.ContainsKey(currentPos))
            {
                ni = this.visitCount[currentPos];
            }
            
            // UCT = vi + C * sqrt(log(np) / ni)
            // vi is the mean reward of the child position
            // C is the exploration constant
            // np is the visit count for the parent position
            // ni is the visit count for the child position 
            float vi = reward_mean;
            float uct = 0;

            // If child is never visited, then UCT value will be infinity
            if(ni == 0)
            {
                uct = float.PositiveInfinity;
            }
            // Else calculate the UCT value using above mentioned formula
            else
            {
                uct = vi + this.exploration_threshold * (float) Math.Sqrt((float) Math.Log(np) / (float) ni);
            }

            return uct;
        }

        private Tuple<GameState, float, int> Playout(GameState state, int depth)
        {
            GameState nextState = state;
            GameState child = null;
            int playoutDepth = 0;
            float reward = 0f;

            // Continue the playout till the playout depth exceeds the playout threshold
            while(playoutDepth < this.playout_threshold)
            {
                // Select the best child state by evaluating all the states
                Tuple<GameState, float, int> selectedState = this.PlayoutSim(nextState, depth + playoutDepth);
                if(playoutDepth == 0)
                    child = selectedState.Item1;

                nextState = selectedState.Item1;
                // Add all the rewards gained in the playout phase
                reward += selectedState.Item2;
                playoutDepth++;
                // If next best child leads to death of our agent or game ends, then break the playout
                if(nextState.IsOver() || nextState.CheckEnemyOnCurrentLoc())
                    break;
            }
            int winReward = 0;
            // Check if the last evaluated state in the playout process leads to winning or losing condition
            if(nextState.CheckEnemyOnCurrentLoc())
                winReward = -1;
            else if(nextState.IsOver())
                winReward = 1;
            // Average out the rewards gained from the playout
            if(playoutDepth > 0)
                reward /= playoutDepth;

            return Tuple.Create(child, reward, winReward);
        }

        // This method selects the best child of the given state according to the evaluation function
        private Tuple<GameState, float, int> PlayoutSim(GameState state, int depth)
        {
            List<GameState> childNodes = null;
            // Find all the child states for current game state
            if(!this.parent_child_mapping.ContainsKey(state))
            {
                childNodes = this.ExpandNode(state, depth);
            }
            else
            {
                childNodes = this.parent_child_mapping[state];
            }

            List<Tuple<GameState, float, int>> rewardList = new List<Tuple<GameState, float, int>>();

            // For each child state, find the reward for that state.
            foreach(GameState child in childNodes)
            {
                // If child state leads to death, then give a high negative reward.
                if(child.CheckEnemyOnCurrentLoc())
                {
                    float rew = (-30.01f) / (depth + 1);
                    rewardList.Add(Tuple.Create(child, rew, -1));
                }
                // If child state leads to win, then give a high positive reward
                else if(child.IsOver())
                {
                    float rew = (100.01f) / (depth + 1);
                    rewardList.Add(Tuple.Create(child, rew, 1));
                }
                // Otherwise we evaluate the state based on evaluation function
                else
                {
                    float reward = this.Evaluate(state, child, depth);
                    rewardList.Add(Tuple.Create(child, reward, 0));
                }
            }

            // If rewardList is empty then select a random child
            if(rewardList.Count == 0)
            {
                var random = new Random();
                int randIndex = random.Next(childNodes.Count);
                return Tuple.Create(childNodes[randIndex], 0f, 0);
            }
            // If rewardList has exactly one child then select that child
            if(rewardList.Count == 1)
                return rewardList[0];
            
            // Sort descending by item3, if two tuples have same item3 value then sort descending by item2
            rewardList.Sort((x, y) => {
                int result = y.Item3.CompareTo(x.Item3);
                return result == 0 ? y.Item2.CompareTo(x.Item2) : result; 
            });
            return rewardList[0];
        }

        // Evaluation function for playout phase
        private float Evaluate(GameState prevGameState, GameState gameState, int depth)
        {
            // Food list in current and previous game state
            List<Tuple<int, int>> currentFoodList = gameState.GetFood();
            List<Tuple<int, int>> prevFoodList = prevGameState.GetFood();
            // Soda list in current and previous game state
            List<Tuple<int, int>> currentSodaList = gameState.GetSoda();
            List<Tuple<int, int>> prevSodaList = prevGameState.GetSoda();
            // Enemies list in current and previous game state
            List<Tuple<int, int>> currentEnemiesList = gameState.GetEnemies();
            List<Tuple<int, int>> prevEnemiesList = prevGameState.GetEnemies();

            // Least distance to an enemy in current game state
            float currentClosestEnemyDist = GetClosestEnemyDistance(gameState);
            // Least distance to an enemy in previous game state
            float prevClosestEnemyDist = GetClosestEnemyDistance(prevGameState);
            // Least distance to food in previous game state
            float prevClosestFoodDist = GetClosestFoodDistance(prevGameState);
            // Least distance to food in current game state
            float currentClosestFoodDist = GetClosestFoodDistance(gameState);
            // Least distance to soda in previous game state
            float prevClosestSodaDist = GetClosestSodaDistance(prevGameState);
            // Least distance to soda in current game state
            float currentClosestSodaDist = GetClosestSodaDistance(gameState);
            // Distance to exit in previous game state
            float prevDistToExit = GetDistanceToExit(prevGameState);
            // Distance to exit in current game state
            float currentDistToExit = GetDistanceToExit(gameState);

            float reward = 0f;
            
            // Negative reward if moving close to enemy, positive if moving away from enemy
            /*if(currentClosestEnemyDist < 10000)
            {
                //This condition creates a bug
                if(currentClosestEnemyDist <= 3)
                {
                    float rew = (currentClosestEnemyDist - prevClosestEnemyDist) / (currentClosestEnemyDist);
                    rew /= (depth + 1);
                    /*if(prevClosestEnemyDist > currentClosestEnemyDist)
                    {    
                        rew *= 2;   
                    }
                    reward += rew;
                }
                else
                    reward += 1f;
            }   */

            // If the map has atleast 1 food or 1 soda left to consume
            if(prevFoodList.Count > 0 || prevSodaList.Count > 0)
            {
                // If this state leads to eating soda, then give reward according to the depth in the tree
                // Lower rewards for soda that is far (consumed in future) 
                // and higher reward for soda that is close
                if(prevSodaList.Count > currentSodaList.Count)
                {
                    float rew = 20f / (depth + 1);
                    reward += rew;
                }
                // If this state leads to eating food, then give reward according to the depth in the tree
                // Lower rewards for food that is far (consumed in future) 
                // and higher reward for food that is close
                else if(prevFoodList.Count > currentFoodList.Count)
                {
                    float rew = 10f / (depth + 1);
                    reward += rew;
                }
                // If next state doesn't lead our player to eat food or soda.
                else
                {
                    // If food was closer than soda in previous state
                    if(prevClosestFoodDist < prevClosestSodaDist)
                    {
                        // If we move a step closer to food
                        if(prevClosestFoodDist > currentClosestFoodDist)
                        {
                            float rew = (prevClosestFoodDist - currentClosestFoodDist) / prevClosestFoodDist;
                            rew /= (depth + 1);
                            reward += rew;
                        }
                        // If we move a step away or keep same distance
                        else
                        {
                            // but moving close to soda
                            if(prevClosestSodaDist > currentClosestSodaDist)
                            {
                                float rew = (prevClosestSodaDist - currentClosestSodaDist) / prevClosestSodaDist;
                                rew /= (depth + 1);
                                reward += rew;
                            }
                            // not moving close to soda and moving away or keeping same distance from food
                            else
                            {
                                float rew = (prevClosestFoodDist - currentClosestFoodDist) / prevClosestFoodDist;
                                rew /= (depth + 1);
                                reward += rew;
                            }
                        }
                    }
                    // If soda was closer than food in previous state
                    else if(prevClosestFoodDist > prevClosestSodaDist)
                    {
                        // If we move a step closer to soda
                        if(prevClosestSodaDist > currentClosestSodaDist)
                        {
                            float rew = (prevClosestSodaDist - currentClosestSodaDist) / prevClosestSodaDist;
                            rew /= (depth + 1);
                            reward += rew;
                        }
                        // If we move a step away or keep same distance
                        else
                        {
                            // but moving close to food
                            if(prevClosestFoodDist > currentClosestFoodDist)
                            {
                                float rew = (prevClosestFoodDist - currentClosestFoodDist) / prevClosestFoodDist;
                                rew /= (depth + 1);
                                reward += rew;
                            }
                            // not moving close to food and moving away or keeping same distance from soda
                            else
                            {
                                float rew = (prevClosestSodaDist - currentClosestSodaDist) / prevClosestSodaDist;
                                rew /= (depth + 1);
                                reward += rew;
                            }
                        }
                    }
                    // If soda and food were at same distance in previous state
                    else
                    {
                        // If this state also has same distance between food and soda
                        if(currentClosestFoodDist == currentClosestSodaDist)
                        {
                            // then reward will be based on soda distance
                            // So if the distance in previous state was 2 and it is also 2 in current state
                            // then reward will be 0. If the distance reduces to 1 then reward will be
                            // positive. If the distance increases to 3 then reward will be negative 
                            float rew = (prevClosestSodaDist - currentClosestSodaDist) / prevClosestSodaDist;
                            rew /= (depth + 1);
                            reward += rew;
                        }
                        // If this state brings us closer to food than soda
                        else if(currentClosestFoodDist < currentClosestSodaDist)
                        {
                            float rew = (prevClosestFoodDist - currentClosestFoodDist) / prevClosestFoodDist;
                            rew /= (depth + 1);
                            reward += rew;
                        }
                        // If this state brings us closer to soda than food
                        else
                        {
                            float rew = (prevClosestSodaDist - currentClosestSodaDist) / prevClosestSodaDist;
                            rew /= (depth + 1);
                            rew *= 1.05f;
                            reward += rew;
                        }
                    }
                }
            }
            // No food or soda left. Move towards exit 
            else
            {
                if(prevDistToExit > 0)
                {
                    // Getting closer to exit
                    if(prevDistToExit > currentDistToExit)
                    {
                        float rew = (prevDistToExit - currentDistToExit) / prevDistToExit;
                        rew /= (depth + 1);
                        //rew *= 2;
                        reward += rew;
                    }
                    // Getting farther or keeping same distance from exit
                    else
                    {
                        float rew = (prevDistToExit - currentDistToExit) / prevDistToExit;
                        rew /= (depth + 1);
                        reward += rew;
                    }
                }
            }

            
            return reward;
        }

        // Returns the distance to the closest enemy in current game state 
        private int GetClosestEnemyDistance(GameState state)
        {
            List<Tuple<int, int>> enemiesList = state.GetEnemies();
            Tuple<int, int> playerPos = state.GetPlayerPosition();

            // If there is no enemy in the game, then this will return 10000
            // Otherwise it will return the minimum distance to an enemy
            int distance = 10000;
            foreach(Tuple<int, int> enemy in enemiesList)
            {
                int dist = this.distanceCalculator.GetMazeDistance(enemy, playerPos);
                if(dist < distance)
                {
                    distance = dist;
                }
            }

            return distance;
        }

        // Returns the distance to the closest food in current game state
        private int GetClosestFoodDistance(GameState state)
        {
            List<Tuple<int, int>> foodList = state.GetFood();
            Tuple<int, int> playerPos = state.GetPlayerPosition();

            // If there is no food in the game, then this will return 10000
            // Otherwise it will return the minimum distance to a food
            int distance = 10000;
            foreach(Tuple<int, int> food in foodList)
            {
                int dist = this.distanceCalculator.GetMazeDistance(food, playerPos);
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

            // If there is no soda in the game, then this will return 10000
            // Otherwise it will return the minimum distance to a soda
            int distance = 10000;
            foreach(Tuple<int, int> soda in sodaList)
            {
                int dist = this.distanceCalculator.GetMazeDistance(soda, playerPos);
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
            // Returns the distance to the exit in the current game state
            return this.distanceCalculator.GetMazeDistance(exit, playerPos);
        }

        // Returns the mean rewards for all the successor states of the given game state
        public List<Tuple<GameState, Tuple<float, int>>> SuccessorMeanRewards(GameState state)
        {
            List<GameState> childNodes = this.parent_child_mapping[state];
            List<Tuple<GameState, Tuple<float, int>>> rewardList = new List<Tuple<GameState, Tuple<float, int>>>();

            foreach(GameState child in childNodes)
            {
                Tuple<float, int> reward;
                if(this.meanReward.ContainsKey(child.GetPlayerPosition()))
                    reward = this.meanReward[child.GetPlayerPosition()];
                else
                    reward = Tuple.Create(-30.0f, -1);
                rewardList.Add(Tuple.Create(child, reward));
            }

            return rewardList;
        }

        // Returns the cumulative rewards for all the successor states of the given game state
        public List<Tuple<GameState, Tuple<float, int>>> SuccessorCumulativeRewards(GameState state)
        {
            List<GameState> childNodes = this.parent_child_mapping[state];
            List<Tuple<GameState, Tuple<float, int>>> rewardList = new List<Tuple<GameState, Tuple<float, int>>>();

            foreach(GameState child in childNodes)
            {
                Tuple<float, int> reward;
                if(this.cumulativeReward.ContainsKey(child.GetPlayerPosition()))
                    reward = this.cumulativeReward[child.GetPlayerPosition()];
                else
                    reward = Tuple.Create(-30.0f, -1);
                rewardList.Add(Tuple.Create(child, reward));
            }

            return rewardList;
        }

        // Returns the visit count for all the successor states of the given game state
        public List<Tuple<GameState, int>> SuccessorVisitCount(GameState state)
        {
            List<GameState> childNodes = this.parent_child_mapping[state];
            List<Tuple<GameState, int>> visitList = new List<Tuple<GameState, int>>();

            foreach(GameState child in childNodes)
            {
                int visit;
                if(this.visitCount.ContainsKey(child.GetPlayerPosition()))
                    visit = this.visitCount[child.GetPlayerPosition()];
                else
                    visit = 0;

                visitList.Add(Tuple.Create(child, visit));
            }

            return visitList;
        }
    }
}
