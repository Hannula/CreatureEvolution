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

    public static CreatureChromosome CreateRandom()
    {
        List<Vector3> lumps = new List<Vector3>();
        int lumpCount = Random.Range(1, 10);
        for(int i = 0; i < lumpCount; i++)
        {
            lumps.Add(new Vector3(Random.Range(0, 10), Random.Range(-10, 10), Random.Range(0, 5)));
        }
        return new CreatureChromosome(lumps);
    }
}
