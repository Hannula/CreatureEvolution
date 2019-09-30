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

        public GeneticAlgorithm(FitnessEvaluator fitnessEvaluator, ParentSelector parentSelector, RecombinationHandler breeder, Mutator mutator)
        {
            this.fitnessEvaluator = fitnessEvaluator;
            this.parentSelector = parentSelector;
            this.breeder = breeder;
            this.mutator = mutator;
        }

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
            //List<ChromosomeFitnessPair<T>> offspring =

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

        /// <summary>
        /// Generic parent selection method
        /// </summary>
        /// <param name="population"></param>
        /// <param name="populationSize"></param>
        /// <returns></returns>
        public List<ChromosomeFitnessPair<T>> FitnessProportionateSelection(List<ChromosomeFitnessPair<T>> parentCandidates, int populationSize)
        {
            List<ChromosomeFitnessPair<T>> parents = new List<ChromosomeFitnessPair<T>>();

            // Calculate the sum of fitnesses
            float totalFitness = 0;
            foreach (ChromosomeFitnessPair<T> parentCandidate in parentCandidates)
            {
                totalFitness += parentCandidate.Fitness;
            }

            // Get random parents
            for(int i = 0; i < populationSize; i++)
            {
                float currentFitness = 0;
                float targetFitness = UnityEngine.Random.Range(0f, totalFitness);
                int j = 0;
                bool found = false;
                // Get random parent with chance proportional to it's fitness
                do
                {
                    ChromosomeFitnessPair<T> parentCandidate = parentCandidates[j++];
                    currentFitness += Mathf.Max(0, parentCandidate.Fitness);
                    if (targetFitness <= currentFitness)
                    {
                        parents.Add(parentCandidate);
                        found = true;
                    }
                }
                while (!found && j < parentCandidates.Count);
            }

            return parents;
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
