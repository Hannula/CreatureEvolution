using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Utility;

namespace Pathfinding
{
    public class WorldNode : NavigationNode
    {
        protected NavigationSpace space;
        protected Vector3 worldPosition;
        protected HashSet<PathfindingObstacle> obstacleSet;

        public Dictionary<NodeTypes, float> costDictionary;

        protected bool updateRequired = true;

        public WorldNode(NavigationSpace space, Vector3 worldPos, Vector3Int gridPos) : base(space, worldPos, gridPos)
        {
            obstacleSet = new HashSet<PathfindingObstacle>();
            costDictionary = new Dictionary<NodeTypes, float>();
        }

        /// <summary>
        /// Add new obstacle to the node and prepare to update costs
        /// </summary>
        /// <param name="obstacle"></param>
        public void AddObstacle(PathfindingObstacle obstacle)
        {
            if (!obstacleSet.Contains(obstacle))
            {
                obstacleSet.Add(obstacle);
                updateRequired = true;
            }
        }

        /// <summary>
        /// Remove obstacle from the node and prepare to update costs
        /// </summary>
        /// <param name="obstacle"></param>
        public void RemoveObstacle(PathfindingObstacle obstacle)
        {
            if (obstacleSet.Contains(obstacle))
            {
                obstacleSet.Remove(obstacle);
                updateRequired = true;
            }
        }

        /// <summary>
        /// Recalculates the costs for this node
        /// </summary>
        public void UpdateCosts()
        {
            costDictionary.Clear();
            foreach (PathfindingObstacle obs in obstacleSet)
            {
                TypeCostPair[] costs = obs.TypeCosts;
                foreach (TypeCostPair pair in costs)
                {
                    NodeTypes type = pair.type;
                    float cost = pair.cost;
                    AddToCost(type, cost);
                }
            }

            updateRequired = false;
        }

        /// <summary>
        /// Add more cost to the cost type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="v"></param>
        private void AddToCost(NodeTypes t, float v)
        {
            if (costDictionary.ContainsKey(t))
            {
                costDictionary[t] += v;
            }
            else
            {
                costDictionary[t] = v;
            }
        }

        /// <summary>
        /// Get the total traversal cost by multiplying the agent's costs by the corresponding node's costs
        /// </summary>
        /// <param name="agentCostDict"></param>
        /// <returns></returns>
        public float GetTraversalCost(Dictionary<NodeTypes, float> agentCostDict)
        {
            if (updateRequired)
            {
                UpdateCosts();
            }

            Dictionary<NodeTypes, float> smallerDict = costDictionary;
            Dictionary<NodeTypes, float> largerDict = agentCostDict;
            float totalCost = 0f;
            // Flip if neccessary
            if (agentCostDict.Count < costDictionary.Count)
            {
                smallerDict = agentCostDict;
                largerDict = costDictionary;
            }

            // Loop through all the type costs pairs in the smaller dict
            foreach (NodeTypes t in smallerDict.Keys)
            {
                float value;
                if (largerDict.TryGetValue(t, out value))
                {
                    // Increase the cost
                    totalCost += value * smallerDict[t];
                }
            }

            return totalCost;
        }
    }

}