using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sandbox
{
    public class CreatureChromosome
    {

        public CreatureGene[] Genes { get; set; }

        public float fitness = float.NegativeInfinity;

        public static CreatureChromosome SinglePointCrossover(CreatureChromosome a, CreatureChromosome b)
        {
            throw (new System.NotImplementedException());
        }

        public CreatureChromosome()
        {

        }
        /// <summary>
        /// Create new random CreatureChromosome with gene limits from base chromosome
        /// </summary>
        /// <returns></returns>
        public static CreatureChromosome CreateRandom(CreatureChromosome baseChromosome)
        {
            CreatureChromosome chromosome = new CreatureChromosome();
            chromosome.Genes = new CreatureGene[baseChromosome.Genes.Length];
            for(int i = 0; i < baseChromosome.Genes.Length; i++)
            {
                CreatureGene baseGene = baseChromosome.Genes[i];
                chromosome.Genes[i] = new CreatureGene(baseGene.Min, baseGene.Max);
            }

            Simulation.Log(chromosome.ToString());

            return chromosome;
        }


        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < Genes.Length; i++)
            {
                str += ((CreatureGeneKeys)i) + ":"+ Genes[i].ToString();

                // Add comma if not the last one
                if (i != Genes.Length - 1)
                {
                    str += ", ";
                }
            }

            return str;
        }


        public ActorClass ToActorClass()
        {
            int size = Genes[(int)CreatureGeneKeys.Size].Value;
            float resourceConsumption = size;
            int hitpoints = size;

            float speed = 5 + size;

            int evasion = 50 - size;

            float legSupport = 0;

            float resourceConsumptionEnergyCost = 1;
            float meatConsumptionEfficiency = 1;
            float plantConsumptionEfficiency = 0;

            float swimmingSpeed = 0;
            float ruggedLandNavigation = 1;
            float softLandNavigation = 1;
            float crampedNavigation = 1;
            float steepNavigation = 1;

            float diggingSpeed = 0;
            float climbingSpeed = 0;
            float divingSkill = swimmingSpeed;
               
            ActorClass actorClass = new ActorClass("Creature", hitpoints, size, evasion, size, swimmingSpeed, ruggedLandNavigation, softLandNavigation, crampedNavigation);
            actorClass.steepNavigation = steepNavigation;
            actorClass.diggingSpeed = diggingSpeed;
            actorClass.climbingSpeed = climbingSpeed;

            actorClass.resourceConsumption = resourceConsumption;
            actorClass.resourceConsumptionEnergyCost = resourceConsumptionEnergyCost;
            actorClass.meatConsumptionEfficiency = meatConsumptionEfficiency;
            actorClass.plantConsumptionEfficiency = plantConsumptionEfficiency;

            actorClass.visibility = 0.5f + size * 0.25f;
            actorClass.noise = 0;
            actorClass.odor = 0;

            actorClass.meatAmount = size * 8;

            // Observation
            actorClass.observationRange = 2;

            actorClass.lightVision = 0.8f;
            actorClass.darkVision = 0.1f;

            actorClass.smellSense = 0;
            actorClass.hearing = 0;

            actorClass.tracking = 0;


            actorClass.height = size;

            return actorClass;
        }

}

    [System.Serializable]
    public struct CreatureGene
    {
        public int Value {
            get; private set;
        }
        public int Min { get; private set; }
        public int Max { get; private set; }

        public CreatureGene(int min, int max)
        {
            Min = min;
            Max = max;
            Value = Random.Range(min, max + 1);
        }

        public CreatureGene(int value, int min, int max)
        {
            Value = value;
            Min = min;
            Max = max;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public enum CreatureGeneKeys
    {
        Size, // 1 - 10
        BodyWidth, // 50-200 // Body width relative to height
        Posture, // 0 for quadrupedal and 1 for bipedal 
        Skin, // 0 for hide, 1 for scales, 2 for feathers, 3 for fur
        SkinThickness, // 1-100
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