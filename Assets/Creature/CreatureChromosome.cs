using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureChromosome
{
    public List<Vector3> lumps;
    public CreatureChromosome()
    {
        lumps = new List<Vector3>();
    }

    public CreatureChromosome(List<Vector3> lumps)
    {
        this.lumps = lumps;
    }

    /// <summary>
    /// Create new random CreatureChromosome
    /// </summary>
    /// <returns></returns>
    public static CreatureChromosome CreateRandom(int lumpMin, int lumpMax, float valueRange)
    {
        List<Vector3> lumps = new List<Vector3>();
        int lumpCount = Random.Range(lumpMin, lumpMax + 1);
        for (int i = 0; i < lumpCount; i++)
        {
            lumps.Add(new Vector3(
                Random.Range(0, valueRange),
                Random.Range(-valueRange, valueRange),
                Random.Range(0, valueRange))
                );
        }
        return new CreatureChromosome(lumps);
    }
}
