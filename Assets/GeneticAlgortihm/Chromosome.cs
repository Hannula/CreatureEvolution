using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GA
{
    public struct SimpleChromosome
    {
        private string Genes;

        public static SimpleChromosome SinglepointCrossover(SimpleChromosome x, SimpleChromosome y)
        {
            return x;
        }
    }
}
