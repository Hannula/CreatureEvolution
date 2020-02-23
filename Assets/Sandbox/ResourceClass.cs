using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ResourceClass
{
    public int id;
    public string name;
    public float plantAmount;
    public float meatAmount;
    public float gatheringDifficulty;

    public float visibility;

    // How deep or high this resource is located
    public float depth;

    // Hazards from eating this food
    public List<Attack> hazards;
}
