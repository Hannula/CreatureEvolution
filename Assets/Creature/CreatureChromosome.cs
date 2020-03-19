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
        private string description = "";
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
            string str = description + "\n";
            for (int i = 0; i < Genes.Length; i++)
            {
                str += ((CreatureGeneKeys)i) + ":" + Genes[i].ToString();

                // Add comma if not the last one
                if (i != Genes.Length - 1)
                {
                    str += ",\n ";
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

            actorClass.coldLimit = 0;
            actorClass.heatLimit = 20;

            float resistanceCrush = 0;
            float resistanceSlash = 0;
            float resistancePiercing = 0;
            float resistanceFire = 0;

            float limbLength = GetGeneValue(CreatureGeneKeys.LimbLength) * 0.01f;
            float limbThickness = GetGeneValue(CreatureGeneKeys.LimbThickness) * 0.01f;

            actorClass.maxHitpoints *= 1 + (limbThickness) * 0.2f;
            actorClass.speed *= Mathf.Sqrt(1 + limbLength);

            actorClass.resourceConsumption += Mathf.Pow(1 + limbLength * limbThickness, 1.5f) * 3f;

            #endregion


            actorClass.resourceConsumptionEnergyCost = 10f;
            actorClass.meatConsumptionEfficiency = 0.5f;
            actorClass.plantConsumptionEfficiency = 0.5f;

            actorClass.visibility = 0.35f + height * 0.2f;
            actorClass.noise = 0.1f;
            actorClass.odor = 0.1f;

            #region Feet Type
            float kickDamage = 1;
            int feetType = GetGeneValue(CreatureGeneKeys.FeetType);
            description += "Foot type: ";
            switch (feetType)
            {
                case 0: // Hoof 
                    actorClass.noise += 0.4f;
                    actorClass.speed *= 1 + Mathf.Sqrt(limbLength);
                    actorClass.swimmingSpeed = 1f + 0.25f * size;
                    actorClass.softLandNavigation = 2f * limbThickness;
                    actorClass.ruggedLandNavigation = 4f * Mathf.Sqrt(1 + limbLength);
                    actorClass.steepNavigation = 4f * Mathf.Sqrt(1 + limbLength);
                    actorClass.meatConsumptionEfficiency -= 0.5f;
                    actorClass.plantConsumptionEfficiency += 0.5f;
                    actorClass.diggingSpeed = 2f;
                    kickDamage = 1.25f;
                    description += "Hoof";
                    break;
                case 1: // Webbed feet
                    actorClass.noise += 0.2f;
                    actorClass.speed *= 0.7f;
                    actorClass.swimmingSpeed = 8f + size;
                    actorClass.softLandNavigation = 10f * limbThickness;
                    actorClass.ruggedLandNavigation = 2f * Mathf.Sqrt(0.5f + limbLength);
                    actorClass.steepNavigation = 2f * limbLength;
                    actorClass.divingSkill = 10f;
                    actorClass.climbingSpeed = 2f;
                    actorClass.diggingSpeed = 1f;
                    kickDamage = 0.75f;
                    description += "Webbed foot";
                    break;
                case 2: // Toes
                    actorClass.speed *= 0.85f;
                    actorClass.swimmingSpeed = 2f + size * 0.5f;
                    actorClass.softLandNavigation = 5f * limbThickness;
                    actorClass.ruggedLandNavigation = 6f * limbLength;
                    actorClass.steepNavigation = 6f * limbLength;
                    actorClass.diggingSpeed = 10f;
                    actorClass.climbingSpeed = 10f;
                    kickDamage = 1f;
                    actorClass.coldLimit += 5;
                    description += "Toes";
                    break;
                case 3: // Claws
                    actorClass.noise += 0.1f;
                    actorClass.speed *= 1f;
                    actorClass.swimmingSpeed = 1.75f + size * 0.25f;
                    actorClass.softLandNavigation = 4f * limbThickness;
                    actorClass.ruggedLandNavigation = 4f * limbLength;
                    actorClass.steepNavigation = 4f * limbLength;
                    actorClass.meatConsumptionEfficiency += 0.35f;
                    actorClass.plantConsumptionEfficiency -= 0.35f;
                    actorClass.diggingSpeed = 2f;
                    actorClass.climbingSpeed = 5f;
                    kickDamage = 0.75f;
                    description += "Claws";
                    break;
                case 4: // Fins
                    actorClass.speed *= 0.3f;
                    actorClass.swimmingSpeed = 5f + size + limbThickness * 10f + limbLength * 5f;
                    actorClass.softLandNavigation = 3f * limbThickness;
                    actorClass.ruggedLandNavigation = 3f * limbLength;
                    actorClass.steepNavigation = 1f * limbLength;
                    actorClass.divingSkill = 20f;
                    kickDamage = 0.5f;
                    description += "Fins";
                    break;
            }
            #endregion

            #region Skin Type
            description += "\nSkin type: ";
            int skinType = GetGeneValue(CreatureGeneKeys.Skin);
            switch (skinType)
            {
                case 0: // Fur 
                    actorClass.noise *= 0.75f;
                    actorClass.odor *= 1.5f;
                    actorClass.heatLimit = -5;
                    actorClass.coldLimit = -15;
                    actorClass.softLandNavigation *= 0.6f;
                    actorClass.swimmingSpeed *= 0.75f;
                    description += "Fur";

                    resistanceCrush = 0.3f;
                    resistanceFire = 0.2f;
                    resistancePiercing = 0.1f;
                    resistanceSlash = 0.3f;

                    actorClass.resourceConsumption *= 1.2f;
                    break;
                case 1: // Skin
                    actorClass.heatLimit = +5;
                    actorClass.coldLimit = +5;
                    description += "Skin";
                    break;
                case 2: // Wet scales
                    actorClass.heatLimit = +5;
                    actorClass.coldLimit = -5;

                    actorClass.swimmingSpeed *= 1.25f;

                    description += "Small Scales";
                    break;

                case 3: // Armor scales
                    actorClass.heatLimit = +10;
                    actorClass.coldLimit = +10;

                    actorClass.speed *= 0.6f;
                    actorClass.swimmingSpeed *= 0.25f;

                    resistanceCrush = 0.5f;
                    resistanceFire = 0.5f;
                    resistancePiercing = 0.1f;
                    resistanceSlash = 0.5f;
                    actorClass.resourceConsumption *= 1.3f;
                    description += "Armor Scales";
                    break;
                case 4: // Feathers
                    actorClass.swimmingSpeed *= 1.35f;

                    kickDamage = 0.75f;
                    description += "Feathers";

                    resistanceCrush = 0.2f;
                    break;
                case 5: // Magic scales
                    actorClass.heatLimit = +10;
                    actorClass.coldLimit = -10;

                    resistanceCrush = 0.2f;
                    resistanceFire = 1f;
                    resistancePiercing = 0.2f;
                    resistanceSlash = 0.4f;

                    actorClass.resourceConsumption *= 3f;

                    description += "Magic Scales";
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
            actorClass.patternColor = new Color(GetGeneValue(CreatureGeneKeys.PatternColorRed) / 255f, GetGeneValue(CreatureGeneKeys.PatternColorGreen) / 255f, GetGeneValue(CreatureGeneKeys.PatternColorBlue) / 255f);

            #region Attacks
            int hitBonus = 20 / (int)(1 + eyePosition * 20);

            // Kick
            kickDamage *= size * (1 + limbLength * 0.25f + limbThickness * 0.5f);
            float kickDamageBonus = kickDamage;
            Attack kick = new Attack("Kick", Mathf.CeilToInt(limbLength * 2 + hitBonus), DamageTypes.Crushing, Mathf.CeilToInt(kickDamage), Mathf.CeilToInt(kickDamageBonus));

            // Add extra slashing damage for claws
            if (feetType == 3)
            {
                kick.AddDamage(new Attack.Damage(DamageTypes.Slashing, Mathf.CeilToInt(kickDamage), Mathf.CeilToInt(kickDamageBonus)));
            }

            actorClass.AddAttacks(kick);


            #endregion

            #region Resistances
            actorClass.resistances[DamageTypes.Crushing] = resistanceCrush;
            actorClass.resistances[DamageTypes.Slashing] = resistanceSlash;
            actorClass.resistances[DamageTypes.Piercing] = resistancePiercing;
            actorClass.resistances[DamageTypes.Fire] = resistanceFire;

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
        LimbLength, // 20-300 - 100 = Size
        LimbThickness, // 1-50
        BaseColorRed, // 0-255
        BaseColorGreen, // 0-255
        BaseColorBlue, // 0-255
        PatternColorRed, // 0-255
        PatternColorGreen, // 0-255
        PatternColorBlue, // 0-255
        FeetType, // 0 for hoof, 1 for webbed foot, 2 for toes, 3 for claws

    }

}