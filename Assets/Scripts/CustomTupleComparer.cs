namespace Completed
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CustomTupleComparer : IEqualityComparer<Tuple<int, int>>
    {
        public bool Equals(Tuple<int, int> tuple1, Tuple<int, int> tuple2)
        {
            if(ReferenceEquals(tuple1, tuple2))
                return true;
            if(ReferenceEquals(tuple1, null))
                return false;
            if(ReferenceEquals(tuple2, null))
                return false;
            if(tuple1.GetType() != tuple2.GetType())
                return false;

            if(tuple1.Item1 == tuple2.Item1 && tuple1.Item2 == tuple2.Item2)
                return true;
            else
                return false;
        }

        public int GetHashCode(Tuple<int, int> tuple)
        {
            int hash = tuple.Item1 + tuple.Item2;
            hash *= 23;
            return hash;
        }
    }
}