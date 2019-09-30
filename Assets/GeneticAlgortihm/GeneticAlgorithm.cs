using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GA
{

    public class GeneticAlgorithm<T>
    {
        public int PopulationSize;

        private List<ChromosomeFitnessPair<T>> population;

        private int maximumGenerations;

        #region Delegate types

        /// <summary>
        /// Method for evaluating the chromosome
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public delegate float FitnessEvaluator(T x);

        /// <summary>
        /// Method for selecting the parents
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public delegate List<ChromosomeFitnessPair<T>> ParentSelector(List<ChromosomeFitnessPair<T>> population, int populationSize);


        /// <summary>
        /// Method for handling crossover between the chromosomes
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public delegate T RecombinationHandler(T x, T y);

        /// <summary>
        /// Method for handling mutation
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public delegate T Mutator(T x);

        /// <summary>
        /// Method for selecting the new generation
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public delegate List<ChromosomeFitnessPair<T>> NewGenerationSelector(List<ChromosomeFitnessPair<T>> parents, List<ChromosomeFitnessPair<T>> offspring, int populationSize);

        /// <summary>
        /// Method for checking when to stop the algorithm
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        public delegate bool TerminationCriterion(List<ChromosomeFitnessPair<T>> population);
        #endregion

        private FitnessEvaluator fitnessEvaluator;
        private ParentSelector parentSelector;
        private NewGenerationSelector newGenerationSelector;
        private RecombinationHandler breeder;
        private Mutator mutator;

        /// <summary>
        /// Run until a solution is found
        /// </summary>
        /// <returns></returns>
        public T Run()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Select parents, perform crossover and mutation and finally select the new generation
        /// </summary>
        /// <returns></returns>
        public bool RunSingleGeneration()
        {
            // Select parents
            List<ChromosomeFitnessPair<T>> parents = parentSelector(population, PopulationSize);

            // Crossover
            List<ChromosomeFitnessPair<T>> offspring =

            // Mutate

            // Calculate fitness for each chromosome
            CalculateFitnessValues(population);

            throw new NotImplementedException();
        }

        private List<ChromosomeFitnessPair<T>> CalculateFitnessValues(List<ChromosomeFitnessPair<T>> population)
        {
            foreach (ChromosomeFitnessPair<T> pair in population)
            {
                pair.Fitness = fitnessEvaluator(pair.Chromosome);
            }
            return population;
        }

    }

    public class ChromosomeFitnessPair<T>
    {
        public T Chromosome;
        public float Fitness;

        public ChromosomeFitnessPair(T value, float fitness)
        {
            Chromosome = value;
            Fitness = fitness;
        }
    }


}
