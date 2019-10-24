using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class PathfindingObstacle : MonoBehaviour
    {
        public bool Visualize = true;
        // Points that the obstacle
        public Vector3[] Points;
        public TypeCostPair[] TypeCosts;
        private HashSet<WorldNode> affectedNodes;

        private void Start()
        {
            affectedNodes = new HashSet<WorldNode>();
            MarkPoints();
        }

        public void OnDestroy()
        {
            ReleasePoints();
        }

        public void OnDisable()
        {
            ReleasePoints();
        }

        public void OnEnabled()
        {
            MarkPoints();
        }

        // Marks the obstacle on the grid
        public void MarkPoints()
        {
            // First clear the previous position
            ReleasePoints();
            foreach (Vector3 vec in Points)
            {
                // Find the node in the point
                WorldNode n = (WorldNode)PathfindingManager.Instance.NavigationSpace.GetNode(vec + transform.position);
                if (n != null)
                {
                    // Mark this obstacle for the node
                    n.AddObstacle(this);
                    // Add the node for this obstacle
                    affectedNodes.Add(n);
                }
            }
        }

        /// <summary>
        /// Remove this obstacle from all the affected nodes and then clear the affected nodes set
        /// </summary>
        public void ReleasePoints()
        {
            // Loop through every affected node
            foreach (WorldNode n in affectedNodes)
            {
                // Remove this obstacle from the node's memory
                n.RemoveObstacle(this);
            }
            // Clear the affected nodes set
            affectedNodes.Clear();
        }

        private void OnDrawGizmos()
        {
            if (Visualize && Points != null && PathfindingManager.Instance != null)
            {
                NavigationSpace navSpace = PathfindingManager.Instance.NavigationSpace;

                foreach (Vector3 vec in Points)
                {
                    Vector3 realPosition = navSpace.GridPositionToWorldPosition(navSpace.WorldPositionToGridPosition(vec + transform.position));
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(realPosition, Vector3.one * 0.5f);
                }
            }
        }
    }
}
