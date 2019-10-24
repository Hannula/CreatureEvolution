using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
namespace Pathfinding
{
    /// <summary> A generic implementation of the A* algorithm to find the shortest path between two nodes </summary>
    public class AStar<TNode>
    {
        /// <summary> Possible states after each step </summary>
        public enum State
        {
            pathFound,
            noPathExists,
            inProgress
        }

        public delegate IEnumerable<TNode> NeighborFinder(TNode node);
        public delegate N DistanceMeasurer<N>(TNode node1, TNode node2);

        // Nodes that are going to be explored next. Contains the "f" value for each node.
        private PriorityQueue<TNode> open = new PriorityQueue<TNode>();

        // Nodes that have already been explored
        public HashSet<TNode> closed = new HashSet<TNode>();

        // A map representing the predecessors of nodes, according to which the final path can be constructed
        private Dictionary<TNode, TNode> previousNodes = new Dictionary<TNode, TNode>();

        // The length of the so far shortest path from start to a node, representing its "g" value
        private Dictionary<TNode, float> minCosts = new Dictionary<TNode, float>();


        // Methods according to which the algorithm should work, set in the constructor
        private NeighborFinder neighborFinder;
        private DistanceMeasurer<float> distanceMeasurer;
        private DistanceMeasurer<float> heuristicEstimate;

        // Start and goal nodes, set in the Start method
        private TNode startNode;
        private TNode goalNode;

        private bool initialized = false;

        /// <param name="neighborFinder"> Defines which nodes are accessible from a node </param>
        /// <param name="distanceMeasurer"> Defines how to measure the distance between two nodes </param>
        /// <param name="heuristicEstimate"> The heuristic method that is used to estimate the length of the shortest path between two nodes </param>
        public AStar(NeighborFinder neighborFinder, DistanceMeasurer<float> distanceMeasurer, DistanceMeasurer<float> heuristicEstimate)
        {
            this.neighborFinder = neighborFinder;
            this.distanceMeasurer = distanceMeasurer;
            this.heuristicEstimate = heuristicEstimate;
        }

        /// <summary> Initialize the algorithm with a start and a goal. </summary>
        public AStar<TNode> Start(TNode startNode, TNode goalNode)
        {
            this.startNode = startNode;
            this.goalNode = goalNode;

            open.Clear();
            open.Add(startNode, heuristicEstimate(startNode, goalNode));
            closed.Clear();
            previousNodes.Clear();
            minCosts.Clear();
            minCosts[startNode] = 0;

            initialized = true;

            return this;
        }

        /// <summary>
        /// Proceed one step, or loop, in calculating the result.
        /// Return the state of the algorithm after the step.
        /// The Start() method has to have been called before calling Step()
        /// </summary>
        public State Step()
        {
            if (!initialized)
            {
                throw new InvalidOperationException("Start() needs to be called at least once before calling Step()");
            }

            if (!open.Empty)
            {
                // If the first node in the open queue is the goal, a path has been found
                if (open.Peek().Equals(goalNode))
                {
                    return State.pathFound;
                }

                // Choose the first node in the queue to be looked at this round. The node can now marked as closed.
                TNode current = open.Pop();
                closed.Add(current);

                foreach (TNode neighbor in neighborFinder(current))
                {
                    // Skip neighbors that are already closed
                    if (closed.Contains(neighbor))
                    {
                        continue;
                    }

                    // Calculate the length of the path from start to this neighbor and compare it to the previously calculated length (positive infinity by default)
                    float tentativeMinCost = minCosts[current] + distanceMeasurer(current, neighbor);
                    if (tentativeMinCost < GetDictValue<float>(minCosts, neighbor, float.PositiveInfinity))
                    {
                        // Write the new length down as the new minimum, mark the node as open with a new f value
                        // and mark the predecessor of this neighbor
                        minCosts[neighbor] = tentativeMinCost;
                        open.Add(neighbor, tentativeMinCost + heuristicEstimate(neighbor, goalNode));
                        previousNodes[neighbor] = current;
                    }
                }
                return State.inProgress;
            }
            return State.noPathExists;
        }

        /// <summary>
        /// Perform Step() until the algorithm has finished, i.e. a path is found or any path can't be found.
        /// Return the state of the algorithm after the final step.
        /// </summary>
        public State Finish()
        {
            while (true)
            {
                State state = this.Step();
                if (state != State.inProgress)
                {
                    return state;
                }
            }
        }

        /// <summary>
        /// Construct the currently best path beginning with the start node and return it.
        /// If the algorithm wasn't finished yet, the path will end with a node other than the goal node.
        /// </summary>
        public List<TNode> CurrentPath()
        {
            if (!open.Empty)
            {
                return ConstructPath(open.Peek());
            }
            return null;
        }

        /// <summary> Directly start and finish the algorithm, returning the path if found or null if no path can be found </summary>
        public List<TNode> FindPath(TNode startNode, TNode goalNode)
        {
            if (Start(startNode, goalNode).Finish() == State.pathFound)
            {
                return CurrentPath();
            }
            return null;
        }




        /// <summary> Construct a path from the start node to the given node according to previousNodes </summary>
        private List<TNode> ConstructPath(TNode node)
        {
            if (node.Equals(startNode))
            {
                return new List<TNode>() { node };
            }
            if (!previousNodes.ContainsKey(node))
            {
                return null;
            }
            else
            {
                List<TNode> totalPath = new List<TNode>();
                bool done = false;
                while (!done)
                {
                    totalPath.Insert(0, node);
                    if (previousNodes.ContainsKey(node))
                    {
                        node = previousNodes[node];
                    }
                    else
                    {
                        done = true;
                    }
                }
                return totalPath;
            }
        }

        /// <summary> Return the value for a key in the dictionary, or a default value if the key doesn't exist </summary>
        private N GetDictValue<N>(Dictionary<TNode, N> dictionary, TNode key, N defaultValue)
        {
            try
            {
                return dictionary[key];
            }
            catch
            {
                return defaultValue;
            }
        }

    }
}
