using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Collections.Generic;

    public class GameState
    {
        private Vector3 playerPosition;
        public Vector3 PlayerPosition { get { return playerPosition; } }
        private Tuple<int, int> playerPos;
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

        public List<string> GetLegalActions()
        {
            return AgentRules.GetLegalActions(playerPos, this.stateData);
        }

        public GameState GenerateSuccessor(string action)
        {
            GameStateData stateData = new GameStateData(this.stateData);
            // Applies the action to the player (doesn't change the physical position of player in the game).
            // Takes the action, generates the successor position for the player (i.e. next position)
            // Then makes updates to the game state data according to the position (i.e. reduce health, 
            // remove food or soda if there were any, at that given position)
            GameState successorState = AgentRules.ApplyAction(playerPos, this, stateData, action);
            return successorState;
        }

        private void LoadMapObjects()
        {
            Transform[] childBoardTransforms = boardObjects.GetComponentsInChildren<Transform>();
            Transform[] childDynamicTransforms = dynamicObjects.GetComponentsInChildren<Transform>();
            
            GameObject[] childBoardObjects = new GameObject[childBoardTransforms.Length];
            for(int i = 0; i < childBoardTransforms.Length; i++)
            {
                childBoardObjects[i] = childBoardTransforms[i].gameObject;
            }

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

        public GameState DeepCopy()
        {
            GameState state = new GameState(this.playerScript, this.loaderScript, this.gameManager, this.boardManager, this.dynamicObjects, this.boardObjects);
            return state;
        }

        public List<Tuple<int, int>> GetFloor()
        {
            return stateData.FloorLoc;
        }

        public List<Tuple<int, int>> GetFood()
        {
            return stateData.FoodLoc;
        }

        public List<Tuple<int, int>> GetSoda()
        {
            return stateData.SodaLoc;
        }

        public List<Tuple<int, int>> GetEnemies()
        {
            return stateData.EnemiesLoc;
        }

        public List<Tuple<int, int>> GetBreakableWalls()
        {
            return stateData.BreakableWallsLoc;
        }

        public Tuple<int, int> GetExitLoc()
        {
            return stateData.ExitLoc;
        }

        public int GetHealthLeft()
        {
            return stateData.HealthLeft;
        }

        public Tuple<int, int> GetPlayerPosition()
        {
            return this.playerPos;
        }

        public int NumEnemies()
        {
            return stateData.EnemiesLoc.Count;
        }

        public bool IsOver()
        {
            Tuple<int, int> pos = Tuple.Create((int) this.playerPosition.x, (int) this.playerPosition.y);
            return (stateData.ExitLoc == pos);
        }

        /*public override bool Equals(object obj)
        {
            return this.Equals(obj as GameState);
        }

        public bool Equals(GameState other)
        {
            if(this.stateData.Equals(other.StateData) && this.playerPos == other.playerPos)
                return true;
            else
                return false;
        }*/

        public bool CheckEnemyOnCurrentLoc()
        {
            if(this.stateData.EnemiesLoc.Contains(this.playerPos))
                return true;
            else
                return false;
        }

        /*public override int GetHashCode()
        {
            int hash = (int) this.playerPosition.x + (int) this.playerPosition.y;
            hash *= 23;
            hash += this.stateData.GetHashCode(); 
            return hash;
        }
        public override int GetHashCode()
        {
            return 1;
        }*/

        public bool CompareStateData(GameState other)
        {
            return this.stateData.CompareData(other.StateData);
        }
    }
}
