using System.Collections;
using UnityEngine;

namespace Completed
{
    using System;
    using System.Collections.Generic;

    public class GameState
    {
        // Loader instance
        private Loader loaderScript;
        public Loader LoaderScript { set { loaderScript = value; } }
        // GameManager game object
        private GameObject gameManager;
        // Board manager instance
        private BoardManager boardManager;
        // Dynamic Objects that are generated for each level
        private GameObject dynamicObjects;
        // Floor and Outer walls that are generated for each level
        private GameObject boardObjects;
        private GameStateData stateData;

        
        public GameState(Loader loader, GameObject gameManager, BoardManager boardManager, GameObject dynamicObjects, GameObject boardObjects)
        {
            this.loaderScript = loader;
            this.gameManager = gameManager;
            this.boardManager = boardManager;
            this.dynamicObjects = dynamicObjects;
            this.boardObjects = boardObjects;
            LoadMapObjects();
            /*loaderScript = GameObject.Find("Main Camera").GetComponent<Loader>();
            gameManager = loaderScript.gameManager;
            boardManager = gameManager.GetComponent<BoardManager>();
            dynamicObjects = boardManager.DynamicObjectsHolder;
            boardObjects = boardManager.BoardHolder;*/
        }

        private void LoadMapObjects()
        {
            GameObject[] childBoardObjects = boardObjects.GetComponentsInChildren<GameObject>();
            GameObject[] childDynamicObjects = dynamicObjects.GetComponentsInChildren<GameObject>();
            
            // Load all the gameobjects positions
            List<Tuple<int, int>> floorList = LoadFloorLocations(childBoardObjects);
            List<Tuple<int, int>> foodList = LoadFoodLocations(childDynamicObjects);
            List<Tuple<int, int>> sodaList = LoadSodaLocations(childDynamicObjects);
            List<Tuple<int, int>> breakableWallsList = LoadBreakableWallLocations(childDynamicObjects);
            List<Tuple<int, int>> enemiesList = LoadEnemyLocations(childDynamicObjects);
            Tuple<int, int> exit = LoadExitLocation(childDynamicObjects);

            stateData = new GameStateData(floorList, foodList, sodaList, breakableWallsList, enemiesList, exit);
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
            GameState state = new GameState(this.loaderScript, this.gameManager, this.boardManager, this.dynamicObjects, this.boardObjects);
            return state;
        }

        
    }
}
