using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Sandbox
{
    public class CreatureChromosome
    {
        public string Name { get; set; }
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
        /// Create new creature chromosome with given genes
        /// </summary>
        /// <param name="genes"></param>
        public CreatureChromosome(CreatureGene[] genes)
        {
            this.Genes = genes;
        }

        /// <summary>
        /// Create new random CreatureChromosome with gene limits from base chromosome
        /// </summary>
        /// <returns></returns>
        public static CreatureChromosome CreateRandom(CreatureChromosome baseChromosome)
        {
            CreatureChromosome chromosome = new CreatureChromosome();
            chromosome.Genes = new CreatureGene[baseChromosome.Genes.Length];
            for (int i = 0; i < baseChromosome.Genes.Length; i++)
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
                str += ((CreatureGeneKeys)i) + ":" + Genes[i].ToString();

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
            float size = GetGeneValue(CreatureGeneKeys.Size) * 0.1f;

            float bodyWidth = GetGeneValue(CreatureGeneKeys.BodyWidth) * 0.01f;
            float headHeight = GetGeneValue(CreatureGeneKeys.HeadHeight) * 0.01f;
            float headWidth = GetGeneValue(CreatureGeneKeys.HeadWidth) * 0.01f;
            float resourceConsumption = 5 * size * bodyWidth;

            float height = size + size * headHeight + size;

            ActorClass actorClass = new ActorClass("Creature");
            actorClass.size = size;
            actorClass.height = size;
            actorClass.maxHitpoints = 5 + size * 5 * bodyWidth + headWidth * 2 + headHeight * 2;
            actorClass.resourceConsumption = resourceConsumption;
            actorClass.steepNavigation = 1;
            actorClass.ruggedLandNavigation = 1;
            actorClass.softLandNavigation = 1;
            actorClass.crampedNavigation = 1;

            actorClass.diggingSpeed = 1;
            actorClass.climbingSpeed = 1;
            #region Legs
            actorClass.speed = 5 + (size * 0.5f);

            float hindlimbLength = GetGeneValue(CreatureGeneKeys.HindlimbLength) * 0.01f;
            float hindlimbThickness = GetGeneValue(CreatureGeneKeys.HindlimbThickness) * 0.01f;

            float forelimbLength = GetGeneValue(CreatureGeneKeys.ForelimbLength) * 0.01f;
            float forelimbThickness = GetGeneValue(CreatureGeneKeys.ForelimbThickness) * 0.01f;

            actorClass.maxHitpoints *= 1 + (hindlimbThickness + forelimbThickness) * 0.2f;
            actorClass.speed *= Mathf.Sqrt(1 + hindlimbLength + forelimbLength);

            actorClass.speed /= 1 + Mathf.Abs(forelimbLength - hindlimbLength);

            actorClass.resourceConsumption += Mathf.Pow(1 + hindlimbLength * hindlimbThickness + forelimbLength * forelimbThickness, 1.5f) * 4f;

            #endregion


            actorClass.ruggedLandNavigation += forelimbLength + hindlimbLength * 2f;
            actorClass.softLandNavigation += forelimbThickness + hindlimbThickness * 2f;
            actorClass.resourceConsumptionEnergyCost = 5f;
            actorClass.meatConsumptionEfficiency = 0.5f;
            actorClass.plantConsumptionEfficiency = 0.5f;

            actorClass.visibility = 0.35f + height * 0.2f;
            actorClass.noise = 0.1f;
            actorClass.odor = 0.1f;

            #region Feet Type
            float kickDamage = 1;
            int feetType = GetGeneValue(CreatureGeneKeys.FeetType);
            switch (feetType)
            {
                case 1: // Hoof 
                    actorClass.noise += 0.4f;
                    actorClass.speed *= 1 + Mathf.Sqrt(forelimbLength + hindlimbLength);
                    actorClass.swimmingSpeed = 1f + 0.25f * size;
                    actorClass.softLandNavigation = 1.5f * (hindlimbThickness + forelimbThickness);
                    actorClass.ruggedLandNavigation = 4f * (hindlimbLength + forelimbLength);
                    actorClass.steepNavigation = 4f * (hindlimbLength + forelimbLength);
                    kickDamage = 1.25f;
                    break;
                case 2: // Webbed feet
                    actorClass.noise += 0.2f;
                    actorClass.speed *= 0.7f;
                    actorClass.swimmingSpeed = 8f + size;
                    actorClass.softLandNavigation = 10f * (hindlimbThickness + forelimbThickness);
                    actorClass.ruggedLandNavigation = 1f * (hindlimbLength + forelimbLength);
                    actorClass.steepNavigation = 1f * (hindlimbLength + forelimbLength);
                    kickDamage = 0.75f;
                    break;
                case 3: // Toes
                    actorClass.speed *= 0.85f;
                    actorClass.swimmingSpeed = 2f + size * 0.5f;
                    actorClass.softLandNavigation = 3f * (hindlimbThickness + forelimbThickness);
                    actorClass.ruggedLandNavigation = 6f * (hindlimbLength + forelimbLength);
                    actorClass.steepNavigation = 6f * (hindlimbLength + forelimbLength);
                    kickDamage = 1f;
                    break;
                case 4: // Claws
                    actorClass.noise += 0.1f;
                    actorClass.speed *= 1f;
                    actorClass.swimmingSpeed = 1.75f + size * 0.25f;
                    actorClass.softLandNavigation = 4f * (hindlimbThickness + forelimbThickness);
                    actorClass.ruggedLandNavigation = 4f * (hindlimbLength + forelimbLength);
                    actorClass.steepNavigation = 4f * (hindlimbLength + forelimbLength);
                    kickDamage = 0.75f;
                    break;
            }
            #endregion

            actorClass.meatAmount = size * bodyWidth * 5;


            float eyeSize = GetGeneValue(CreatureGeneKeys.EyeSize) * 0.01f;
            int eyeNumber = GetGeneValue(CreatureGeneKeys.EyeNumber);
            float eyePosition = GetGeneValue(CreatureGeneKeys.EyePosition) * 0.01f;
            float eyeHeight = height;

            actorClass.lightVision = eyeSize * 0.25f + Mathf.Sqrt(eyeNumber) * 0.5f + eyePosition * headWidth;
            actorClass.darkVision = eyeSize * 0.5f + eyeNumber * 0.15f;

            actorClass.tracking = 2 / (0.1f + eyePosition);

            actorClass.resourceConsumption += size * eyeSize * 6f + Mathf.Pow(eyeNumber, 2) * 3f;

            // Observation
            actorClass.observationRange = 2 + Mathf.Sqrt(1 + eyeHeight);

            actorClass.smellSense = 0;
            float earSize = GetGeneValue(CreatureGeneKeys.EarSize) * 0.01f;
            actorClass.hearing = earSize;
            actorClass.resourceConsumption += earSize * 5f;

            // Base color and pattern color
            actorClass.baseColor = new Color(GetGeneValue(CreatureGeneKeys.BaseColorRed) / 255f, GetGeneValue(CreatureGeneKeys.BaseColorGreen) / 255f, GetGeneValue(CreatureGeneKeys.BaseColorBlue) / 255f);
            actorClass.patternColor = new Color(GetGeneRatio(CreatureGeneKeys.PatternColorRed), GetGeneRatio(CreatureGeneKeys.PatternColorGreen), GetGeneRatio(CreatureGeneKeys.PatternColorBlue));

            #region Attacks
            int hitBonus = 20 / (int)(1 + eyePosition * 20);

            // Kick
            kickDamage *= size * (1 + (hindlimbLength * 0.25f) + (hindlimbThickness * 0.5f));
            float kickDamageBonus = kickDamage;
            Attack kick = new Attack("Kick", Mathf.CeilToInt(hindlimbLength * 2 + hitBonus), DamageTypes.Crushing, Mathf.CeilToInt(kickDamage), Mathf.CeilToInt(kickDamageBonus));

            // Add extra slashing damage for claws
            if (feetType == 3)
            {
                kick.AddDamage(new Attack.Damage(DamageTypes.Slashing, Mathf.CeilToInt(kickDamage), Mathf.CeilToInt(kickDamageBonus)));
            }

            actorClass.AddAttacks(kick);


            #endregion

            return actorClass;
        }

        public int GetGeneValue(CreatureGeneKeys geneKey)
        {
            int index = (int)geneKey;

            return Genes[index].Value;
        }

        public float GetGeneRatio(CreatureGeneKeys geneKey)
        {
            int index = (int)geneKey;

            return Genes[index].Ratio;
        }


        public string ToString()
        {
            string str = Name;
            foreach (CreatureGeneKeys geneKey in Enum.GetValues(typeof(CreatureGeneKeys)))
            {
                CreatureGene gene = Genes[(int)geneKey];

                str += "\n";
            }

            return str;
        }
    }

    [System.Serializable]
    public struct CreatureGene
    {
        public int Value
        {
            get; private set;
        }

        public float Ratio { get; private set; }
        public int Min { get; private set; }
        public int Max { get; private set; }

        public int Range { get; private set; }
        public CreatureGene(int min, int max)
        {
            Min = min;
            Max = max;
            Value = UnityEngine.Random.Range(min, max + 1);

            Range = Max - Min;

            if (Range == 0)
            {
                Ratio = 1;
            }
            else
            {
                Ratio = (Value - Min) / (float)Range;
            }
        }

        public CreatureGene(int value, int min, int max)
        {
            Value = value;
            Min = min;
            Max = max;

            Range = Max - Min;

            if (Range == 0)
            {
                Ratio = 1;
            }
            else
            {
                Ratio = (Value - Min) / (float)Range;
            }
        }

        public int RandomValue()
        {
            return UnityEngine.Random.Range(Min, Max + 1);
        }

        public void Mutate(float ratio)
        {
            int Amount = Mathf.CeilToInt(Range * ratio);
            int maxMutation = Mathf.Clamp(Value + Amount, Min, Max);
            int minMutation = Mathf.Clamp(Value - Amount, Min, Max);

            Value = UnityEngine.Random.Range(minMutation, maxMutation + 1);
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
        Skin, // 0 for hide, 1 for scales, 2 for feathers, 3 for fur
        SkinThickness, // 1-100
        HeadWidth, // 10-50
        HeadHeight, // 10-50
        EyeNumber, // 0-10
        EyeSize, // 1-100
        EyePosition, // Eye position relative to head size
        MouthSize, // Mouth size relative to head size
        MouthType, // 0=Sharp teeth,1=blunt teeth, 2=beak, 3=trunk
        EarSize, // Ear size relative to head size
        EarPosition, // Ear y-position relative to head height
        ForelimbLength, // 20-300 - 100 = Size
        ForelimbThickness, // 1-50
        HindlimbLength, // 20-300 - 100 = Size
        HindlimbThickness, // 1-50
        BaseColorRed, // 0-255
        BaseColorGreen, // 0-255
        BaseColorBlue, // 0-255
        PatternColorRed, // 0-255
        PatternColorGreen, // 0-255
        PatternColorBlue, // 0-255
        FeetType, // 0 for hoof, 1 for webbed foot, 2 for toes, 3 for claws

    }

}