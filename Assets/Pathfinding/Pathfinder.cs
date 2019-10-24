using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

public class Pathfinder : MonoBehaviour
{
    public float ClimbHeight = 0.25f;
    // Pathfinding
    private List<NavigationNode> path;
    private AStar<NavigationNode> astar;

    [SerializeField, Tooltip("Individual traversal costs for each node type")]
    private TypeCostPair[] traversalCosts;
    private Dictionary<NodeTypes, float> nodeTraversalCosts = new Dictionary<NodeTypes, float>();

    private void Awake()
    {
        astar = new AStar<NavigationNode>(NodeNeighbors, TraversalCostBetweenNodes, DistanceBetweenNodes);

        // Fill the actual traversal cost dictionary with values from the now deserialized temporary data structure
        foreach (TypeCostPair pair in traversalCosts)
        {
            nodeTraversalCosts.Add(pair.type, pair.cost);
        }
        // Forget the temporary data structure now that it has done its job
        traversalCosts = null;
    }

    public List<NavigationNode> FindPath(Vector3 worldPosition)
    {
        path = astar.FindPath(PathfindingManager.Instance.NavigationSpace.GetNodeClamped(transform.position), PathfindingManager.Instance.NavigationSpace.GetNodeClamped(worldPosition));
        return path;
    }

    /// <summary>Calculates the euclidean distance between two nodes. Used for pathfinding.</summary>
    private static float DistanceBetweenNodes(NavigationNode n1, NavigationNode n2)
    {
        return Vector3.Distance(n1.WorldPosition, n2.WorldPosition);
    }

    /// <summary>The neighbors of a node. Used for pathfinding.</summary>
    private IEnumerable<NavigationNode> NodeNeighbors(NavigationNode node)
    {
        return node.NavigationSpace.GetNodeNeighbors(node).Where(n => (n.WorldPosition.y - node.WorldPosition.y < ClimbHeight));
    }

    /// <summary>
    /// True traversal cost between nodes.
    /// Considers both the distance between and the node traversal cost multiplier.
    /// Used for pathfinding.
    /// </summary>
    private float TraversalCostBetweenNodes(NavigationNode n1, NavigationNode n2)
    {
        float distance = DistanceBetweenNodes(n1, n2);
        return distance + distance * ((WorldNode)n2).GetTraversalCost(nodeTraversalCosts);
    }
}
