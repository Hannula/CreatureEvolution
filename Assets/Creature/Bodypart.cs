using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public class Bodypart
    {
        List<Vector3> lumps;

        public float maximumOverlap;

        public bool needsUpdate;

        // Dimensions
        float height;
        float maxWidth;
        float minWidth;

        float topWidth;
        float bottomWidth;

        float area;


        // Distance matrix
        float[,] lumpDistances;

        public void Update()
        {
            CalculateLumpDistances();
        }

        /// <summary>
        /// Calculate distance between every pair of lumps
        /// </summary>
        private void CalculateLumpDistances()
        {
            lumpDistances = new float[lumps.Count, lumps.Count];
            // Iterate every pair of lumps
            for (int i = 0; i < lumps.Count; i++)
            {

                for (int j = i; j < lumps.Count; j++)
                {
                    float dist = 0;

                    // Calculate distance if lumps are no the same
                    if (i != j)
                    {
                        Vector3.Distance(lumps[i], lumps[j]);
                    }
                    // Save distance to distance matrix
                    lumpDistances[i, j] = dist;
                    lumpDistances[j, i] = dist;
                }
            }
        }

        /// <summary>
        /// Remove overlapping lumps that are inside other lumps.
        /// </summary>
        private void RemoveOverlapping()
        {
            foreach (Vector3 lump in lumps)
            {

            }

            throw new NotImplementedException();
        }

    }
}
