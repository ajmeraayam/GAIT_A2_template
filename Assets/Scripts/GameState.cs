using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class GameState
    {
        // Position of the player in the current state
        private Vector3 playerPosition;
        public Vector3 PlayerPosition { get { return playerPosition; } }
        // Position of the player in the current state as a tuple
        private Tuple<int, int> playerPos;
        // Reference to the Player script to get the health of the player
        private Player playerScript;
        public Player PlayerScript { get { return playerScript; } }
        // Loader instance
        private Loader loaderScript;
        public Loader LoaderScript { get{ return loaderScript; } }
        // GameManager game object
        private GameObject gameManager;
        public GameObject GameManager { get{ return gameManager; } }
        // Board manager instance
        private BoardManager boardManager;
        public BoardManager BoardManagerScript { get{ return boardManager; } }
        // Dynamic Objects that are generated for each level
        private GameObject dynamicObjects;
        public GameObject DynamicObjects { get{ return dynamicObjects; } }
        // Floor and Outer walls that are generated for each level
        private GameObject boardObjects;
        public GameObject BoardObjects { get{ return boardObjects; } }
        // Object that stores data about this game state
        private GameStateData stateData;
        public GameStateData StateData { get{ return stateData; } }

        public GameState(Player playerScript, Loader loader, GameObject gameManager, BoardManager boardManager, GameObject dynamicObjects, GameObject boardObjects)
        {
            this.playerScript = playerScript;
            this.playerPosition = this.playerScript.gameObject.transform.position;
            this.playerPos = Tuple.Create((int) this.playerPosition.x, (int) this.playerPosition.y);
            this.loaderScript = loader;
            this.gameManager = gameManager;
            this.boardManager = boardManager;
            this.dynamicObjects = dynamicObjects;
            this.boardObjects = boardObjects;
            // Load all the floor, food, soda, enemy, exit and breakable walls location into a instance of GameStateData
            LoadMapObjects();
        }

        public GameState(GameState state, GameStateData data, Tuple<int, int> playerPosition)
        {
            this.playerScript = state.PlayerScript;
            this.playerPos = playerPosition;
            this.playerPosition = new Vector3((float) playerPos.Item1, (float) playerPos.Item2, 0f);
            this.loaderScript = state.LoaderScript;
            this.gameManager = state.GameManager;
            this.boardManager = state.BoardManagerScript;
            this.dynamicObjects = state.DynamicObjects;
            this.boardObjects = state.BoardObjects;
            this.stateData = data;
        }

        // Returns all the legal actions according to the player's position and state data
        public List<string> GetLegalActions()
        {
            return AgentRules.GetLegalActions(playerPos, this.stateData);
        }

        // Generates a successor game state according to the given action
        public GameState GenerateSuccessor(string action)
        {
            // Copy the state data 
            GameStateData stateData = new GameStateData(this.stateData);
            // Applies the action to the player (doesn't change the physical position of player in the game).
            // Takes the action, generates the successor position for the player (i.e. next position)
            // Then makes updates to the game state data according to the position (i.e. reduce health, 
            // remove food or soda if there were any, at that given position)
            GameState successorState = AgentRules.ApplyAction(playerPos, this, stateData, action);
            return successorState;
        }

        // Load all the floor, food, soda, enemy, exit, breakable walls location and health into an instance of GameStateData
        private void LoadMapObjects()
        {
            // Find all the floor and outer wall transforms
            Transform[] childBoardTransforms = boardObjects.GetComponentsInChildren<Transform>();
            // Find all the food, soda, enemy, breakable walls and exit transforms
            Transform[] childDynamicTransforms = dynamicObjects.GetComponentsInChildren<Transform>();
            
            // Get the floor and outer walls gameobjects
            GameObject[] childBoardObjects = new GameObject[childBoardTransforms.Length];
            for(int i = 0; i < childBoardTransforms.Length; i++)
            {
                childBoardObjects[i] = childBoardTransforms[i].gameObject;
            }

            // Get the food, soda, enemy, breakable walls and exit gameobjects
            GameObject[] childDynamicObjects = new GameObject[childDynamicTransforms.Length];
            for(int i = 0; i < childDynamicTransforms.Length; i++)
            {
                childDynamicObjects[i] = childDynamicTransforms[i].gameObject;
            }

            // Load all the gameobjects positions
            List<Tuple<int, int>> floorList = LoadFloorLocations(childBoardObjects);
            List<Tuple<int, int>> foodList = LoadFoodLocations(childDynamicObjects);
            List<Tuple<int, int>> sodaList = LoadSodaLocations(childDynamicObjects);
            List<Tuple<int, int>> breakableWallsList = LoadBreakableWallLocations(childDynamicObjects);
            List<Tuple<int, int>> enemiesList = LoadEnemyLocations(childDynamicObjects);
            Tuple<int, int> exit = LoadExitLocation(childDynamicObjects);
            int health = this.playerScript.food;
            // Create a new GameStateData from the calculated information
            stateData = new GameStateData(floorList, foodList, sodaList, breakableWallsList, enemiesList, exit, health);
        }

        private List<Tuple<int, int>> LoadFloorLocations(GameObject[] childBoardObjects)
        {
            // Storing floor locations
            List<Tuple<int, int>> floorLoc = new List<Tuple<int, int>>();
            // Find all the floor gameobjects and store the xy location in the list as a tuple
            foreach(GameObject obj in childBoardObjects)
            {
                if(obj.CompareTag("Floor"))
                {
                    Vector3 loc = obj.transform.position;
                    Tuple<int, int> tup = new Tuple<int, int>((int) loc.x, (int) loc.y);
                    floorLoc.Add(tup);
                }
            }
            return floorLoc;
        }

        private List<Tuple<int, int>> LoadFoodLocations(GameObject[] childDynamicObjects)
        {
            // Storing food locations
            List<Tuple<int, int>> foodLoc = new List<Tuple<int, int>>();
            // Find all the food gameobjects and store the xy location in the list as a tuple
            foreach(GameObject obj in childDynamicObjects)
            {
                if(obj.CompareTag("Food"))
                {
                    Vector3 loc = obj.transform.position;
                    Tuple<int, int> tup = new Tuple<int, int>((int) loc.x, (int) loc.y);
                    foodLoc.Add(tup);
                }
            }
            return foodLoc;
        }

        private List<Tuple<int, int>> LoadSodaLocations(GameObject[] childDynamicObjects)
        {
            // Storing soda locations
            List<Tuple<int, int>> sodaLoc = new List<Tuple<int, int>>();
            // Find all the food gameobjects and store the xy location in the list as a tuple
            foreach(GameObject obj in childDynamicObjects)
            {
                if(obj.CompareTag("Soda"))
                {
                    Vector3 loc = obj.transform.position;
                    Tuple<int, int> tup = new Tuple<int, int>((int) loc.x, (int) loc.y);
                    sodaLoc.Add(tup);
                }
            }
            return sodaLoc;
        }

        private List<Tuple<int, int>> LoadBreakableWallLocations(GameObject[] childDynamicObjects)
        {
            // Storing breakable walls locations
            List<Tuple<int, int>> breakableWallsLoc = new List<Tuple<int, int>>();
            // Find all the breakable wall gameobjects and store the xy location in the list as a tuple
            foreach(GameObject obj in childDynamicObjects)
            {
                if(obj.CompareTag("BreakableWall"))
                {
                    Vector3 loc = obj.transform.position;
                    Tuple<int, int> tup = new Tuple<int, int>((int) loc.x, (int) loc.y);
                    breakableWallsLoc.Add(tup);
                }
            }
            return breakableWallsLoc;
        }

        private Tuple<int, int> LoadExitLocation(GameObject[] childDynamicObjects)
        {
            Tuple<int, int> exitLoc = new Tuple<int, int>(-1, -1);
            // Find the xy location of the exit position
            foreach(GameObject obj in childDynamicObjects)
            {
                if(obj.CompareTag("Exit"))
                {
                    Vector3 loc = obj.transform.position;
                    exitLoc = new Tuple<int, int>((int) loc.x, (int) loc.y);
                }
            }

            return exitLoc;
        }

        private List<Tuple<int, int>> LoadEnemyLocations(GameObject[] childDynamicObjects)
        {
            // Storing enemy locations
            List<Tuple<int, int>> enemiesLoc = new List<Tuple<int, int>>();
            // Find all the enemy gameobjects and store the xy location in the list as a tuple
            foreach(GameObject obj in childDynamicObjects)
            {
                if(obj.CompareTag("Enemy"))
                {
                    Vector3 loc = obj.transform.position;
                    Tuple<int, int> tup = new Tuple<int, int>((int) loc.x, (int) loc.y);
                    enemiesLoc.Add(tup);
                }
            }
            return enemiesLoc;
        }

        // Create a deep copy of the game state
        public GameState DeepCopy()
        {
            GameState state = new GameState(this.playerScript, this.loaderScript, this.gameManager, this.boardManager, this.dynamicObjects, this.boardObjects);
            return state;
        }

        // Returns the list of floor locations 
        public List<Tuple<int, int>> GetFloor()
        {
            return stateData.FloorLoc;
        }

        // Returns the list of food locations
        public List<Tuple<int, int>> GetFood()
        {
            return stateData.FoodLoc;
        }

        // Returns the list of soda locations
        public List<Tuple<int, int>> GetSoda()
        {
            return stateData.SodaLoc;
        }

        // Returns the list of enemy locations
        public List<Tuple<int, int>> GetEnemies()
        {
            return stateData.EnemiesLoc;
        }

        // Returns the list of breakable wall locations
        public List<Tuple<int, int>> GetBreakableWalls()
        {
            return stateData.BreakableWallsLoc;
        }

        // Returns the exit location
        public Tuple<int, int> GetExitLoc()
        {
            return stateData.ExitLoc;
        }

        // Returns the amount of health left in the game
        public int GetHealthLeft()
        {
            return stateData.HealthLeft;
        }

        // Returns the position of the player
        public Tuple<int, int> GetPlayerPosition()
        {
            return this.playerPos;
        }

        // Returns the current number of enemies
        public int NumEnemies()
        {
            return stateData.EnemiesLoc.Count;
        }

        // Returns true if player reaches exit, false otherwise
        public bool IsOver()
        {
            Tuple<int, int> pos = Tuple.Create((int) this.playerPosition.x, (int) this.playerPosition.y);
            return (stateData.ExitLoc == pos);
        }

        // Returns true if current player position has an enemy at same location, false otherwise
        public bool CheckEnemyOnCurrentLoc()
        {
            if(this.stateData.EnemiesLoc.Any(tup => tup.Item1 == playerPos.Item1 && tup.Item2 == playerPos.Item2))
                return true;
            else
                return false;
        }

        // Compares the state data of this game state with passed game state
        public bool CompareStateData(GameState other)
        {
            return this.stateData.CompareData(other.StateData);
        }
    }
}
