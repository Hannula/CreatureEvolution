using UnityEngine;

using Utility;
using Pathfinding;

public class PathfindingManager : MonoBehaviour
{
    /// <summary>The singleton instance of this class.</summary>
    public static PathfindingManager Instance { get; private set; }

    [SerializeField]
    private Vector3Int gridDimensions;
    [SerializeField]
    private Vector3 worldDimensions;

    public NavigationSpace NavigationSpace { get; private set; }

    [SerializeField]
    private bool drawGrid;

    private void Awake()
    {
        NavigationSpace = new NavigationSpace(gridDimensions.x, gridDimensions.y, gridDimensions.z, worldDimensions, transform.position, (space, vec, point) => new NavigationNode(space, vec, point));

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            throw new System.InvalidOperationException("An instance of " + typeof(PathfindingManager).Name + " already exists.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        NavigationSpace.WorldPosition = transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (drawGrid && NavigationSpace != null)
        {
            foreach (NavigationNode n in NavigationSpace.Nodes)
            {
                WorldNode wn = (WorldNode)n;
                if (wn.costDictionary.Count > 0)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(n.WorldPosition, NavigationSpace.WorldToPointScale * 0.2f);
                }
                else
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawCube(n.WorldPosition, NavigationSpace.WorldToPointScale * 0.1f);
                }

            }
        }
    }
}
