using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GA
{

    public class GeneticAlgorithm<T>
    {
        public int populationSize;

        private List<ChromosomeFitnessPair<T>> population;

        private int maximumGenerations;

        private int generation = 0;

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
        private RecombinationHandler recombinator;
        private Mutator mutator;

        public GeneticAlgorithm(int populationSize, FitnessEvaluator fitnessEvaluator, ParentSelector parentSelector, RecombinationHandler recombinator, Mutator mutator)
        {
            this.populationSize = populationSize;
            this.fitnessEvaluator = fitnessEvaluator;
            this.parentSelector = parentSelector;
            this.recombinator = recombinator;
            this.mutator = mutator;
        }

        /// <summary>
        /// Set new population of T-type chromosomes and make them into chromosome-fitness-pairs
        /// </summary>
        /// <param name="newPopulation"></param>
        public void SetPopulation(List<T> newPopulation)
        {
            population = new List<ChromosomeFitnessPair<T>>();
            foreach (T chromosome in newPopulation)
            {
                // Wrap chromosome inside ChromosomeFitnessPair
                population.Add(new ChromosomeFitnessPair<T>(chromosome, 1));
            }

            // Finally calculate fitness
            CalculateFitnessValues(population);

            generation = 0;
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
        /// Select parents, perform crossover and mutation, and finally calculate new fitness values
        /// </summary>
        /// <returns></returns>
        public void RunSingleGeneration()
        {
            Debug.Log("Generation " + generation);
            // Select parents
            List<ChromosomeFitnessPair<T>> parents = parentSelector(population, populationSize);
            Debug.Log("Number of parents: " + parents.Count);

            // Crossover and mutation
            List<ChromosomeFitnessPair<T>> offspring = new List<ChromosomeFitnessPair<T>>();

            for (int i = 0; i < parents.Count; i++)
            {
                // Get parents
                ChromosomeFitnessPair<T> a = parents[i];
                ChromosomeFitnessPair<T> b = parents[(i + 1) % parents.Count];

                // Recombine
                T chromosome = recombinator(a.Chromosome, b.Chromosome);

                // Mutate
                chromosome = mutator(chromosome);

                offspring.Add(new ChromosomeFitnessPair<T>(chromosome, 1));
            }

            Debug.Log("Number of offspring: " + offspring.Count);

            // Mutate
            // Calculate fitness for each chromosome
            CalculateFitnessValues(population);

            generation += 1;
        }

        public List<T> GetPopulation()
        {
            List<T> pop = new List<T>();
            foreach (ChromosomeFitnessPair<T> pair in population)
            {
                pop.Add(pair.Chromosome);
            }
            return pop;
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
        public static List<ChromosomeFitnessPair<T>> FitnessProportionateSelection(List<ChromosomeFitnessPair<T>> parentCandidates, int populationSize)
        {
            List<ChromosomeFitnessPair<T>> parents = new List<ChromosomeFitnessPair<T>>();

            // Calculate the sum of fitnesses
            float totalFitness = 0;
            foreach (ChromosomeFitnessPair<T> parentCandidate in parentCandidates)
            {
                totalFitness += parentCandidate.Fitness;
            }

            // Get random parents
            for (int i = 0; i < populationSize; i++)
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
