using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GA;
using Sandbox;
using System;
using System.Linq;

public class CreatureEvolution
{
    private int populationSize;
    private GeneticAlgorithm<CreatureChromosome> GA;
    private List<KeyIntRangePair> geneLimits;
    private CreatureChromosome baseChromosome;
    private int currentChromosomeIndex;
    private List<CreatureChromosome> population;
  
    public CreatureEvolution(int populationSize, List<KeyIntRangePair> geneLimits)
    {
        this.populationSize = populationSize;
        this.geneLimits = geneLimits;

        baseChromosome = new CreatureChromosome();
        baseChromosome.Genes = new CreatureGene[Enum.GetNames(typeof(CreatureGeneKeys)).Length];
        // Add gene limits
        foreach (KeyIntRangePair geneLimit in geneLimits)
        {
            try
            {
                string key = geneLimit.Key;
                int index = (int)Enum.Parse(typeof(CreatureGeneKeys), key);
                baseChromosome.Genes[index] = new CreatureGene(geneLimit.Min, geneLimit.Max);
                Simulation.Log(key + " set to " + geneLimit.Min + "-" + geneLimit.Max);
            }
            catch (Exception e)
            {
                Simulation.Log(e.Message);
            }
        }

        // Create a new genetic algorithm
        GA = new GeneticAlgorithm<CreatureChromosome>(populationSize, GetCreatureFitness, GeneticAlgorithm<CreatureChromosome>.FitnessProportionateSelection, SinglePointCrossover, Mutate);

        // Generate the initial population
        population = new List<CreatureChromosome>();
        for (int i = 0; i < populationSize; i++)
        {
            population.Add(CreatureChromosome.CreateRandom(baseChromosome));
        }
    }



    public static float GetCreatureFitness(CreatureChromosome chromosome)
    {
        return chromosome.fitness;
    }

    public static CreatureChromosome Mutate(CreatureChromosome x)
    {
        // Select a random gene to mutate
        CreatureGene gene = x.Genes[UnityEngine.Random.Range(0, x.Genes.Length)];
        gene.Mutate(0.5f);

        return x;
    }

    public static CreatureChromosome SinglePointCrossover(CreatureChromosome x, CreatureChromosome y)
    {
        Simulation.Log("Combining chromosomes " + x.Name + x.fitness + " and " + y.Name + y.fitness);
        int length = x.Genes.Length;
        int point = UnityEngine.Random.Range(1, length - 1);

        CreatureGene[] newGenes = new CreatureGene[length];

        for(int i = 0; i < length; i++)
        {
            CreatureGene gene;
            // Pick the gene from one of the parents
            if (i < point)
            {
                // Genes before random point are taken from the first chromosome
                gene = x.Genes[i];
            }
            else
            {
                // Genes after random point are taken from the second chromosome
                gene = y.Genes[i];
            }
            // Add gene to new genes array
            newGenes[i] = gene;
        }

        // Create new chromosome with new genes
        CreatureChromosome newChromosome = new CreatureChromosome(newGenes);

        return newChromosome;
    }

    public CreatureChromosome GetNext()
    {
        // Advance genetic algorithm when every chromosome from the current population is explored
        if (currentChromosomeIndex >= population.Count)
        {
            if (GA.Generation == 0)
            {
                // Set initial population if this is the first generation
                GA.SetPopulation(population);
            }
            else
            {
                GA.CalculateFitness();
            }
            // Generate new generation after fitness values have been calculated
            GA.Recombine();

            // Get new generation population
            population = GA.GetPopulation();
            currentChromosomeIndex = 0;
        }
        CreatureChromosome chromosome = population[currentChromosomeIndex++];
        chromosome.Name = "Creature " + currentChromosomeIndex + " Gen " + GA.Generation;
        return chromosome;
    }
}

