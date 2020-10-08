namespace Completed
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CustomComparer : IEqualityComparer<GameState>
    {
        public bool Equals(GameState state1, GameState state2)
        {
            if(ReferenceEquals(state1, state2))
                return true;
            if(ReferenceEquals(state1, null))
                return false;
            if(ReferenceEquals(state2, null))
                return false;
            if(state1.GetType() != state2.GetType())
                return false;

            if(state1.GetPlayerPosition().Item1 == state2.GetPlayerPosition().Item1 && state1.GetPlayerPosition().Item2 == state2.GetPlayerPosition().Item2 && state1.CompareStateData(state2))
                return true;
            else
                return false;
        }

        public int GetHashCode(GameState state)
        {
            int hash = state.GetPlayerPosition().Item1 + state.GetPlayerPosition().Item2;
            hash *= 23;
            return hash;
        }
    }
}
