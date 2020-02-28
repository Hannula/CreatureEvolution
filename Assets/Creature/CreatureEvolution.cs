using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GA;
using Sandbox;
using System;

public class CreatureEvolution
{
    private int populationSize;
    private GeneticAlgorithm<CreatureChromosome> GA;
    private List<KeyIntRangePair> geneLimits;
    private CreatureChromosome baseChromosome;

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
        GA = new GeneticAlgorithm<CreatureChromosome>(populationSize, EvaluateCreatureFitness, GeneticAlgorithm<CreatureChromosome>.FitnessProportionateSelection, SinglePointCrossover, Mutate);

        // Generate the initial population
        List<CreatureChromosome> initialPopulation = new List<CreatureChromosome>();
        for (int i = 0; i < populationSize; i++)
        {
            initialPopulation.Add(CreatureChromosome.CreateRandom(baseChromosome));
        }

        // GA.SetPopulation(initialPopulation);
    }

    public static float EvaluateCreatureFitness(CreatureChromosome chromosome)
    {
        throw (new NotImplementedException());
    }

    public static CreatureChromosome Mutate(CreatureChromosome x)
    {
        throw (new NotImplementedException());
    }

    public static CreatureChromosome SinglePointCrossover(CreatureChromosome x, CreatureChromosome y)
    {
        throw (new NotImplementedException());
    }
}

[System.Serializable]
public struct ParameterValueRange
{
    public float min;
    public float max;
    public float baseScore;
    public float penalty;
    public float power;

    public bool Inside(float value)
    {
        return value >= min && value <= max;
    }

    public float Score(float value)
    {
        bool inside = Inside(value);
        float totalPenalty = Mathf.Pow(DistanceToRange(value) * penalty, power);

        return baseScore - totalPenalty;
    }

    public float DistanceToRange(float value)
    {
        if (Inside(value))
        {
            return 0;
        }
        else
        {
            if (value < min)
                return min - value;
            else
                return value - max;
        }
    }

}
