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

        void Start()
        {
            player = GetComponent<Player>();
            mcts = null;
            loaderScript = GameObject.Find("Main Camera").GetComponent<Loader>();
            gameManager = loaderScript.gameManager;
            boardManager = gameManager.GetComponent<BoardManager>();
            //dynamicObjects = boardManager.DynamicObjectsHolder;
            dynamicObjects = GameObject.Find("DynamicObjects");
            //boardObjects = boardManager.BoardHolder;
            boardObjects = GameObject.Find("Board");
            print("Player agent initialized. Coroutine starting");
            StartCoroutine(MCTSCoroutine());
        }

        private bool CanMove()
        {
            return !(player.isMoving || player.levelFinished || player.gameOver || GameManager.instance.doingSetup);
        }

        /*public void Update()
        {
            //If it's not the player's turn, exit the function.
            if (!CanMove())
            {
                return;
            }

            //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
            int horizontal = (int)(Input.GetAxisRaw("Horizontal"));

            //Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
            int vertical = (int)(Input.GetAxisRaw("Vertical"));

            if (horizontal < 0)
            {
                player.AttemptMove<Wall>(-1, 0);
            }
            else if (horizontal > 0)
            {
                player.AttemptMove<Wall>(1, 0);
            }
            else if (vertical < 0)
            {
                player.AttemptMove<Wall>(0, -1);
            }
            else if (vertical > 0)
            {
                player.AttemptMove<Wall>(0, 1);
            }
        }

        /*public void Update()
        {
            print("Entering Update method");
            //If it's not the player's turn, exit the function.
            if (!CanMove())
            {
                return;
            }
            
            string direction = "";
            // Calling MCTS first time in the game
            if(this.mcts == null)
            {
                this.mcts = new MCTS();
                GameState gameState = new GameState(this.player, this.loaderScript, this.gameManager, this.boardManager, this.dynamicObjects, this.boardObjects);
                print("Started training tree");
                this.mcts.TrainTree(gameState);
            }
            // If MCTS is processing the tree, no action should be taken
            else if(this.mcts != null && this.mcts.processing)
            {
                return;
            }
            else if(this.mcts != null && !this.mcts.processing)
            {
                direction = this.mcts.GetNextDirection();
                print("Direction - " + direction);
                this.mcts = null;
            }

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
                return;
            }
        }*/

        private IEnumerator MCTSCoroutine()
        {
            yield return new WaitForSeconds(2f);
            while(true)
            {
                if(CanMove())
                {
                    this.mcts = new MCTS();
                    GameState gameState = new GameState(this.player, this.loaderScript, this.gameManager, this.boardManager, this.dynamicObjects, this.boardObjects);
                    print("Starting training");
                    
                    int iter = 0;
                    float sleepTime = 1f / this.mcts.training_threshold;
                    while(iter < this.mcts.training_threshold)
                    {
                        this.mcts.RunNextTrainingIteration(gameState);
                        iter++;
                        yield return new WaitForSeconds(sleepTime);
                    }

                    print("Current position - " + gameState.GetPlayerPosition());

                    List<Tuple<GameState, Tuple<float, int>>> successorRewards = this.mcts.SuccessorMeanRewards(gameState);
                    foreach(Tuple<GameState, Tuple<float, int>> tup in successorRewards)
                    {
                        print("Position - " + tup.Item1.GetPlayerPosition() + " Reward - " + tup.Item2);
                    }

                    List<Tuple<GameState, Tuple<float, int>>> successorCumulativeRewards = this.mcts.SuccessorCumulativeRewards(gameState);
                    foreach(Tuple<GameState, Tuple<float, int>> tup in successorCumulativeRewards)
                    {
                        print("Position - " + tup.Item1.GetPlayerPosition() + " Cumulative Reward - " + tup.Item2);
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
