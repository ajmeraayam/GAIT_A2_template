using UnityEngine;

namespace Completed
{
    //Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
    public class PlayerAgent : MonoBehaviour
    {
        private Player player;

        void Start()
        {
            player = GetComponent<Player>();
        }

        private bool CanMove()
        {
            return !(player.isMoving || player.levelFinished || player.gameOver || GameManager.instance.doingSetup);
        }

        public void Update()
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
    }
}
