using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Pathfinding
{
    public class NavigationSpace
    {
        public Vector3 WorldPosition;

        public readonly Vector3 WorldDimensions;
        public readonly Vector3 PointToWorldScale;
        public readonly Vector3 WorldToPointScale;

        private NavigationNode[,,] nodeGrid;

        /// <summary>All the nodes in this navigation space.</summary>
        public IEnumerable<NavigationNode> Nodes { get { foreach (NavigationNode n in nodeGrid) yield return n; } }

        /// <param name="width">The number of nodes on the X axis</param>
        /// <param name="height">The number of nodes on the Y axis</param>
        /// <param name="depth">The number of nodes on the Z axis</param>
        public NavigationSpace(int width, int height, int depth, Vector3 worldDimensions, Vector3 worldPosition, Func<NavigationSpace, Vector3, Vector3Int, NavigationNode> nodeCreator)
        {
            this.WorldPosition = worldPosition;
            this.WorldDimensions = worldDimensions;
            this.PointToWorldScale = new Vector3(worldDimensions.x / width, worldDimensions.y / height, worldDimensions.z / depth);
            this.WorldToPointScale = new Vector3(width / worldDimensions.x, height / worldDimensions.y, depth / worldDimensions.z);
            this.nodeGrid = new NavigationNode[width, height, depth];

            // Create new nodes to fill the entire node grid
            for (int x = 0; x < nodeGrid.GetLength(0); x++)
            {
                float worldX = x * PointToWorldScale.x;
                for (int z = 0; z < nodeGrid.GetLength(2); z++)
                {
                    float worldZ = z * PointToWorldScale.z;
                    for (int y = 0; y < nodeGrid.GetLength(1); y++)
                    {
                        float worldY = y * PointToWorldScale.y;
                        nodeGrid[x, y, z] = nodeCreator(this, new Vector3(worldX, worldY, worldZ), new Vector3Int(x, y, z));
                    }
                }
            }
        }

        /// <summary>
        /// Converts a world position into a point in the navigation space.
        /// Gets the node at that point or null if the point is outside the navigation space's dimensions.
        /// </summary>
        public NavigationNode GetNode(Vector3 worldPosition)
        {
            Vector3Int p = WorldPositionToGridPosition(worldPosition);
            return GetNode(p.x, p.y, p.z);
        }

        /// <summary>
        /// Gets the node at a point in the navigation space or null if the point is outside its dimensions.
        /// </summary>
        public NavigationNode GetNode(Vector3Int gridPosition)
        {
            return GetNode(gridPosition.x, gridPosition.y, gridPosition.z);
        }

        /// <summary>
        /// Gets the node at a point in the navigation space or null if the point is outside its dimensions.
        /// </summary>
        public NavigationNode GetNode(int x, int y, int z)
        {
            return IsInside(x, y, z) ? nodeGrid[x, y, z] : null;
        }

        /// <summary>
        /// Converts a world position into a point in the navigation space.
        /// Gets the node at that point clamped within the dimensions of the navigation space.
        /// </summary>
        public NavigationNode GetNodeClamped(Vector3 worldPosition)
        {
            Vector3Int p = WorldPositionToGridPosition(worldPosition);
            return GetNodeClamped(p.x, p.y, p.z);
        }

        /// <summary>
        /// Gets the node at a point in the navigation space.
        /// The position is clamped within the dimensions of the navigation space.
        /// </summary>
        public NavigationNode GetNodeClamped(Vector3Int gridPosition)
        {
            return GetNodeClamped(gridPosition.x, gridPosition.y, gridPosition.z);
        }

        /// <summary>
        /// Gets the node at a point in the navigation space.
        /// The position is clamped within the dimensions of the navigation space.
        /// </summary>
        public NavigationNode GetNodeClamped(int x, int y, int z)
        {
            x = Mathf.Clamp(x, 0, nodeGrid.GetLength(0) - 1);
            y = Mathf.Clamp(y, 0, nodeGrid.GetLength(1) - 1);
            z = Mathf.Clamp(z, 0, nodeGrid.GetLength(2) - 1);
            return nodeGrid[x, y, z];
        }

        /// <summary>
        /// Gets the not-null neighbor nodes of the given node. Both diagonal and nondiagonal neighbors are included.
        /// Doesn't include the node itself or nodes outside the navigation space dimensions.
        /// </summary>
        public IEnumerable<NavigationNode> GetNodeNeighbors(NavigationNode node)
        {
            Vector3Int p = node.GridPosition;

            for (int x = p.x - 1; x <= p.x + 1; ++x)
            {
                for (int y = p.y - 1; y <= p.y + 1; ++y)
                {
                    for (int z = p.z - 1; z <= p.z + 1; ++z)
                    {
                        // Don't consider this node itself.
                        if (!(x == p.x && y == p.y && z == p.z))
                        {
                            NavigationNode neighbor = GetNode(x, y, z);
                            if (neighbor != null)
                            {
                                yield return neighbor;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the navigation space coordinates of a point are within the dimensions of this space
        /// </summary>
        public bool IsInside(Vector3Int p)
        {
            return IsInside(p.x, p.y, p.z);
        }

        /// <summary>
        /// Checks if navigation space coordinates are within the dimensions of this space
        /// </summary>
        public bool IsInside(int x, int y, int z)
        {
            return x >= 0 && x < nodeGrid.GetLength(0)
                && y >= 0 && y < nodeGrid.GetLength(1)
                && z >= 0 && z < nodeGrid.GetLength(2);
        }

        /// <summary>
        /// Converts a world position to a navigation space point.
        /// Does not care if the point is outside the dimensions of this space.
        /// </summary>
        public Vector3Int WorldPositionToGridPosition(Vector3 worldPosition)
        {
            Vector3 truePosition = worldPosition - this.WorldPosition;
            int x = Mathf.RoundToInt(truePosition.x * WorldToPointScale.x);
            int y = Mathf.RoundToInt(truePosition.y * WorldToPointScale.y);
            int z = Mathf.RoundToInt(truePosition.z * WorldToPointScale.z);
            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Converts a navigation space point to a world space position.
        /// Does not care if the point is outside the dimensions of this space.
        /// </summary>
        public Vector3 GridPositionToWorldPosition(Vector3Int point)
        {
            float x = point.x * PointToWorldScale.x;
            float y = point.y * PointToWorldScale.y;
            float z = point.z * PointToWorldScale.z;
            return new Vector3(x, y, z) + this.WorldPosition;
        }
    }
}