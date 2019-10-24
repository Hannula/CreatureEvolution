using UnityEngine;

using Utility;

namespace Pathfinding
{
    public class NavigationNode
    {
        public readonly NavigationSpace NavigationSpace;
        public readonly Vector3Int GridPosition;

        private Vector3 worldPosition;

        public Vector3 WorldPosition
        {
            get
            {
                return NavigationSpace.WorldPosition + worldPosition;
            }
            set
            {
                worldPosition = value;
            }
        }

        public NavigationNode(NavigationSpace navigationSpace, Vector3 worldPosition, Vector3Int gridPosition)
        {
            this.NavigationSpace = navigationSpace;
            this.worldPosition = worldPosition;
            this.GridPosition = gridPosition;
        }
    }

}
