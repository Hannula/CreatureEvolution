using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Pathfinding
{
    [Serializable]
    public struct TypeCostPair
    {
        public NodeTypes type;
        public float cost;

        /// <summary>
        /// Creates a new Type-Cost pair with type t and cost c
        /// </summary>
        /// <param name="t"></param>
        /// <param name="c"></param>
        public TypeCostPair(NodeTypes t, float c)
        {
            type = t;
            cost = c;
        }
    }

    public enum NodeTypes
    {
        Snow,
        SantaBuilding,
        SantaBunker,
        HostileBuilding,
        Air,
        Slow,
        Damage,
        Fire,
        Ice,
        Trap,
    }

}
