using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GA
{

    public class GeneticAlgorithm<T>
    {
        public int PopulationSize;

        private List<ChromosomeValuePair<T>> population;

        private int maximumGenerations;

        #region Delegate types

        /// <summary>
        /// Method for evaluating the chromosome
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public delegate float FitnessEvaluator(T x);

        /// <summary>
        /// Method for selecting the offspring
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public delegate List<T> OffspringSelector(List<ChromosomeValuePair<T>> population);


        /// <summary>
        /// Method for handling crossover between the chromosomes
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public delegate T Breeder(T x, T y);

        /// <summary>
        /// Method for handling mutation
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public delegate T Mutate(T x);

        /// <summary>
        /// Method for checking when to stop the algorithm
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public delegate bool TerminationCriterion(List<ChromosomeValuePair<T>> population);
        #endregion

        private FitnessEvaluator fitnessEvaluator;

        public T Run()
        {

            return population[0].Chromosome;
        }

        public bool RunSingle()
        {

            return true;
        }

        private List<ChromosomeValuePair<T>> CalculateFitnessValues(List<ChromosomeValuePair<T>> population)
        {
            foreach (ChromosomeValuePair<T> pair in population)
            {
                pair.Fitness = fitnessEvaluator(pair.Chromosome);
            }
            return population;
        }

    }

    public class ChromosomeValuePair<T>
    {
        public T Chromosome;
        public float Fitness;

        public ChromosomeValuePair(T value, float fitness)
        {
            Chromosome = value;
            Fitness = fitness;
        }
    }


}
