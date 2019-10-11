using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GA;

public class CreatureEvolution : MonoBehaviour
{
    public int populationSize;
    private GeneticAlgorithm<CreatureChromosome> GA;

    [Header("Fitness parameters")]
    public ParameterValueRange lumpCountRange;
    public ParameterValueRange lumpRadiusRange;
    public ParameterValueRange lumpDistanceToNearestRange;
    public ParameterValueRange lumpXRange;
    public ParameterValueRange lumpYRange;
    public float lumpMutationChance = 5;

    [Header("Visualization")]
    public bool update = true;
    public SimpleMesh[] simpleMeshes;


    void Start()
    {
        // Create new genetic algorithm
        GA = new GeneticAlgorithm<CreatureChromosome>(populationSize, EvaluateCreatureFitness, GeneticAlgorithm<CreatureChromosome>.FitnessProportionateSelection, SinglePointCrossover, Mutate);

        // Generate the initial population
        List<CreatureChromosome> initialPopulation = new List<CreatureChromosome>();
        for (int i = 0; i < populationSize; i++)
        {
            initialPopulation.Add(CreatureChromosome.CreateRandom(1, 5, 0.5f));
        }

        GA.SetPopulation(initialPopulation);

        UpdateMeshes();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GA.RunSingleGeneration();
        }
    }

    public float EvaluateCreatureFitness(CreatureChromosome chromosome)
    {
        float fitness = 0;
        // Individual lumps
        foreach (Vector3 lump in chromosome.lumps)
        {
            fitness += lumpXRange.Score(lump.x);
            fitness += lumpYRange.Score(lump.y);
            // z is used for radius
            fitness += lumpRadiusRange.Score(lump.z);
        }
        fitness /= chromosome.lumps.Count;

        // Lump Count
        fitness += lumpCountRange.Score(chromosome.lumps.Count);
        return fitness;
    }

    public CreatureChromosome Mutate(CreatureChromosome x)
    {
        float removeChance = lumpMutationChance / x.lumps.Count;
        // Remove or move random lumps
        for (int i = x.lumps.Count - 1; i >= 0; i--)
        {
            Vector3 lump = x.lumps[i];
            if (Random.Range(0, 100) < removeChance)
            {
                x.lumps.RemoveAt(i);
            }
            else if (Random.Range(0, 100) < lumpMutationChance)
            {

                x.lumps[i] = new Vector3(lump.x + Random.Range(-0.3f, 0.3f), lump.y + Random.Range(-0.3f, 0.3f), lump.z + Random.Range(-0.3f, 0.3f));
            }
        }

        if (Random.Range(0, 100) < lumpMutationChance)
        {
            // Add new lump
            x.lumps.Insert(Random.Range(0, x.lumps.Count - 1), new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)));
        }
        return x;
    }

    public CreatureChromosome SinglePointCrossover(CreatureChromosome x, CreatureChromosome y)
    {
        int pointX = Random.Range(1, x.lumps.Count);
        int pointY = Random.Range(1, y.lumps.Count);

        // Combine lists of vectors
        List<Vector3> lumps = x.lumps.GetRange(0, pointX);
        if (y.lumps.Count > 0)
        {
            lumps.AddRange(y.lumps.GetRange(pointY, y.lumps.Count));
        }

        return new CreatureChromosome(lumps);
    }

    public void UpdateMeshes()
    {
        int meshCount = Mathf.Min(populationSize, simpleMeshes.Length);
        List<CreatureChromosome> chromosomes = GA.GetPopulation();

        for (int i = 0; i < meshCount; i++)
        {
            simpleMeshes[i].SetLumps(chromosomes[i].lumps);
        }
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
