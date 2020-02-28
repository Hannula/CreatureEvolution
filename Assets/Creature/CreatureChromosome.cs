using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sandbox
{
    public class CreatureChromosome
    {

        public List<CreatureGene> Genes { get; set; }

        public static CreatureChromosome SinglePointCrossover(CreatureChromosome a, CreatureChromosome b)
        {
            throw (new System.NotImplementedException());
        }

        public CreatureChromosome()
        {

        }
        /// <summary>
        /// Create new random CreatureChromosome
        /// </summary>
        /// <returns></returns>
        public static CreatureChromosome CreateRandom()
        {
            return new CreatureChromosome();
        }


    }

    [System.Serializable]
    public struct CreatureGene
    {
        public int Value
        {
            get { return Value; }
            set { Value = Mathf.Clamp(value, Min, Max); }
        }
        public int Min { get; private set; }
        public int Max { get; private set; }
    }

    public enum CreatureGeneKeys
    {
        Size, // 1 - 10
        BodyWidth, // 50-200 // Body width relative to height
        Posture, // 0 for quadrupedal and 1 for bipedal 
        Skin, // 0 for hide, 1 for scales, 2 for feathers, 3 for fur
        SkinThickess, // 1-100
        HeadWidth, // 10-50
        HeadHeight, // 10-50
        HeadPosition, // 0-200 - 0=head completely inside body, 100=head completely outside, but no neck, 200=long neck
        EyeNumber, // 0-10
        EyeSize, // 1-100
        EyePosition, // Eye position relative to head size
        MouthSize, // Mouth size relative to head size
        MouthType, // 0=Sharp teeth,1=blunt teeth, 2=beak, 3=trunk
        EarSize, // Ear size relative to head size
        EarPosition, // Ear y-position relative to head height
        ForelimbLength, // 20-300 - 100 = BodyHeight
        ForelimbThickness, // 1-50
        HindlimbLength, // 20-300 - 100 = BodyHeight
        HindlimbThickness, // 1-50
    }
}