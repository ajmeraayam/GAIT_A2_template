using UnityEngine;

namespace Completed
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    //Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
    public class PlayerAgent : MonoBehaviour
    {
        private Player player;
        private MCTS mcts;
        // Loader instance
        private Loader loaderScript;
        // GameManager game object
        private GameObject gameManager;
        // Board manager instance
        private BoardManager boardManager;
        // Dynamic Objects that are generated for each level
        private GameObject dynamicObjects;
        // Floor and Outer walls that are generated for each level
        private GameObject boardObjects;
        private GameManager gameManagerScript;
        private DistanceCalculator calculator;

        void Start()
        {
            player = GetComponent<Player>();
            mcts = null;
            loaderScript = GameObject.Find("Main Camera").GetComponent<Loader>();
            gameManager = loaderScript.gameManager;
            boardManager = gameManager.GetComponent<BoardManager>();
            dynamicObjects = GameObject.Find("DynamicObjects");
            boardObjects = GameObject.Find("Board");
            gameManagerScript = gameManager.GetComponent<GameManager>();
            print("Player agent initialized. Coroutine starting");
            StartCoroutine(MCTSCoroutine());
        }

        private bool CanMove()
        {
            return !(player.isMoving || player.levelFinished || player.gameOver || GameManager.instance.doingSetup);
        }

        public void StoreDistanceCalculator(DistanceCalculator calc)
        {
            this.calculator = calc;
        }

        private IEnumerator MCTSCoroutine()
        {
            yield return new WaitForSeconds(5f);
            while(true)
            {
                if(CanMove())
                {
                    this.mcts = new MCTS(this.calculator);
                    GameState gameState = new GameState(this.player, this.loaderScript, this.gameManager, this.boardManager, this.dynamicObjects, this.boardObjects);
                    
                    int iter = 0;
                    float sleepTime = 0.25f / this.mcts.training_threshold;
                    while(iter < this.mcts.training_threshold)
                    {
                        this.mcts.RunNextTrainingIteration(gameState);
                        iter++;
                        yield return new WaitForSeconds(sleepTime);
                    }

                    string direction = "";
                    direction = this.mcts.GetNextDirection(gameState);
                    
                    print("Direction - " + direction);

                    if(direction.Equals("NORTH"))
                    {
                        player.AttemptMove<Wall>(0, 1);
                    }
                    else if(direction.Equals("SOUTH"))
                    {
                        player.AttemptMove<Wall>(0, -1);
                    }
                    else if(direction.Equals("EAST"))
                    {
                        player.AttemptMove<Wall>(1, 0);
                    }
                    else if(direction.Equals("WEST"))
                    {
                        player.AttemptMove<Wall>(-1, 0);
                    }
                    else
                    {
                        continue;
                    }
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    print("Setup or movement animation going on. Sleeping for 2 seconds");
                    yield return new WaitForSeconds(2f);
                }
            }
        }
    }
}
