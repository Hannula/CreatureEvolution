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


            float headSize = GetGeneValue(CreatureGeneKeys.HeadSize) * 0.01f;
            float resourceConsumption = 2f * size * (1 + headSize);

            float height = (size + size * headSize) * 0.3f;

            ActorClass actorClass = new ActorClass("Creature");
            actorClass.size = size;
            actorClass.height = size;
            actorClass.meatAmount = height * 6;
            actorClass.maxHitpoints = Mathf.CeilToInt(10 * size);
            actorClass.resourceConsumption = resourceConsumption;
            actorClass.steepNavigation = 1;
            actorClass.ruggedLandNavigation = 1;
            actorClass.softLandNavigation = 1;
            actorClass.crampedNavigation = 1;

            actorClass.diggingSpeed = 0;
            actorClass.divingSkill = 0;
            actorClass.climbingSpeed = 0;
            actorClass.evasion = Mathf.FloorToInt(50f - headSize * 10f); // Reduced evasion for having bigger head

            actorClass.coldLimit = 5;
            actorClass.heatLimit = 25;

            float resistanceCrush = 0;
            float resistanceSlash = 0;
            float resistancePiercing = 0;
            float resistanceFire = 0;


            #region Legs
            actorClass.speed = 5 + (size * 0.5f);

            float limbLength = GetGeneValue(CreatureGeneKeys.LimbLength) * 0.01f;
            float limbThickness = GetGeneValue(CreatureGeneKeys.LimbThickness) * 0.01f;

            actorClass.maxHitpoints += size * (1 + (limbThickness) * 0.2f); // Add health for thick legs
            actorClass.speed *= Mathf.Sqrt(1 + limbLength); // Add speed for long legs

            actorClass.resourceConsumption += Mathf.Pow(1 + limbLength + limbThickness, 1.5f) * size * 0.25f;

            actorClass.coldLimit -= limbThickness * 10;
            actorClass.coldLimit += limbLength * 3f;
            #endregion


            actorClass.resourceConsumptionEnergyCost = 10f;
            actorClass.meatConsumptionEfficiency = 0.5f;
            actorClass.plantConsumptionEfficiency = 0.5f;


            actorClass.visibility = 0.35f + height * 0.5f;
            actorClass.noise = 0.5f;
            actorClass.odor = 0.5f;

            #region Feet Type
            float kickDamage = 1;
            int feetType = GetGeneValue(CreatureGeneKeys.FeetType);
            description += "\nFoot type: ";
            switch (feetType)
            {
                case 0: // Hoof 
                    actorClass.noise += 0.5f;
                    actorClass.speed *= 1 + Mathf.Sqrt(limbLength);
                    actorClass.swimmingSpeed = 1f + 0.25f * size;
                    actorClass.softLandNavigation = 2f * limbThickness;
                    actorClass.ruggedLandNavigation = 4f * Mathf.Sqrt(1 + limbLength);
                    actorClass.steepNavigation = 4f * Mathf.Sqrt(1 + limbLength);
                    actorClass.meatConsumptionEfficiency -= 0.5f;
                    actorClass.plantConsumptionEfficiency += 0.5f;
                    actorClass.diggingSpeed = 2f;
                    actorClass.divingSkill -= 8f;
                    kickDamage = 1.25f;
                    description += "Hoof";
                    break;
                case 1: // Webbed feet
                    actorClass.noise += 0.25f;
                    actorClass.speed *= 0.5f;
                    actorClass.swimmingSpeed = 6f + size;
                    actorClass.softLandNavigation = 10f * limbThickness;
                    actorClass.ruggedLandNavigation = 2f * Mathf.Sqrt(0.5f + limbLength);
                    actorClass.steepNavigation = 2f * limbLength;
                    actorClass.divingSkill += 4f;
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
                    actorClass.noise -= 0.15f;
                    description += "Toes";
                    break;
                case 3: // Claws
                    actorClass.noise += 0.3f;
                    actorClass.speed *= 1f;
                    actorClass.swimmingSpeed = 1.75f + size * 0.25f;
                    actorClass.softLandNavigation = 4f * limbThickness;
                    actorClass.ruggedLandNavigation = 4f * limbLength;
                    actorClass.steepNavigation = 4f * limbLength;
                    actorClass.meatConsumptionEfficiency += 0.35f;
                    actorClass.plantConsumptionEfficiency -= 0.35f;
                    actorClass.diggingSpeed = 2f;
                    actorClass.climbingSpeed = 5f;
                    actorClass.divingSkill -= 3f;
                    kickDamage = 0.75f;
                    description += "Claws";
                    break;
                case 4: // Fins

                    actorClass.speed *= 0.3f;
                    actorClass.swimmingSpeed = 5f + size + limbThickness * 10f + limbLength * 5f;
                    actorClass.softLandNavigation = 3f * limbThickness;
                    actorClass.ruggedLandNavigation = 3f * limbLength;
                    actorClass.steepNavigation = 1f * limbLength;
                    actorClass.divingSkill += 8f;
                    kickDamage = 0.5f;
                    description += "Fins";
                    break;
            }
            #endregion

            #region Skin Type
            description += "\nSkin type: ";
            int skinType = GetGeneValue(CreatureGeneKeys.SkinType);
            switch (skinType)
            {
                case 0: // Fur 
                    actorClass.noise *= 0.75f;
                    actorClass.odor *= 1.5f;
                    actorClass.heatLimit -= 5;
                    actorClass.coldLimit -= 15;
                    actorClass.softLandNavigation *= 0.6f;
                    actorClass.swimmingSpeed *= 0.75f;
                    description += "Fur";

                    resistanceCrush = 0.3f;
                    resistanceFire = 0.2f;
                    resistancePiercing = 0.1f;
                    resistanceSlash = 0.3f;

                    actorClass.resourceConsumption *= 1.25f;

                    actorClass.divingSkill -= 3f;
                    break;
                case 1: // Skin
                    actorClass.heatLimit += 5;
                    actorClass.coldLimit += 5;
                    description += "Skin";
                    break;
                case 2: // Wet scales
                    actorClass.heatLimit += 5;
                    actorClass.coldLimit -= 5;
                    actorClass.divingSkill += 3.5f;
                    actorClass.swimmingSpeed *= 1.25f;
                    actorClass.resourceConsumption *= 1.35f;
                    actorClass.odor *= 1.5f;
                    resistanceFire = 0.9f;
                    description += "Small Scales";
                    break;

                case 3: // Armor scales
                    actorClass.heatLimit += 10;
                    actorClass.coldLimit += 10;

                    actorClass.speed *= 0.6f;
                    actorClass.swimmingSpeed *= 0.25f;

                    resistanceCrush = 0.5f;
                    resistanceFire = 0.5f;
                    resistancePiercing = 0.1f;
                    resistanceSlash = 0.5f;
                    resistanceFire = 1f;
                    actorClass.resourceConsumption *= 1.5f;
                    actorClass.divingSkill -= 10f;
                    actorClass.noise *= 1.5f;
                    description += "Armor Scales";
                    break;
                case 4: // Feathers
                    actorClass.swimmingSpeed *= 1.3f;
                    actorClass.resourceConsumption *= 1.25f;
                    kickDamage = 0.75f;
                    description += "Feathers";
                    actorClass.noise *= 0.5f;
                    resistanceCrush = 0.2f;
                    break;

                case 5: // Magic scales
                    actorClass.heatLimit += 10;
                    actorClass.coldLimit -= 10;

                    resistanceCrush = 0.2f;
                    resistanceFire = 1f;
                    resistancePiercing = 0.2f;
                    resistanceSlash = 0.4f;
                    actorClass.noise *= 1.5f;
                    actorClass.resourceConsumption *= 2.5f;

                    description += "Magic Scales";
                    break;
            }
            #endregion


            #region Eyes
            float eyeSize = GetGeneValue(CreatureGeneKeys.EyeSize) * 0.01f;
            int eyeNumber = GetGeneValue(CreatureGeneKeys.EyeNumber);
            float eyeHeight = height;

            resistancePiercing -= eyeNumber * eyeSize * 0.33f; // Large eyes make creature less resilient to attacks
            resistanceSlash -= eyeNumber * eyeSize * 0.15f;


            actorClass.lightVision = eyeSize * Mathf.Sqrt(eyeNumber);
            actorClass.darkVision = Mathf.Pow(eyeSize, 2) * eyeNumber * 0.1f;

            actorClass.tracking = eyeNumber; // Number of eyes help tracking

            actorClass.resourceConsumption += size * (Mathf.Pow(1 + eyeSize, 2f) + Mathf.Pow(eyeNumber, 3f)) * 0.1f;

            // Observation
            actorClass.observationRange = 2.5f + eyeSize; // Eye size increases observation range

            if (eyeNumber >= 2) // Add bonus for stereoscopic vision
            {
                actorClass.lightVision *= 1.25f;
                actorClass.darkVision *= 1.25f;
                actorClass.observationRange += 1f;
                actorClass.tracking += 1f;
            }
            #endregion


            #region Ears
            int earType = GetGeneValue(CreatureGeneKeys.EarType);
            description += "\nEar type: ";
            switch (earType)
            {
                case 0: // No ear 
                    description += "No ears";
                    break;
                case 1: // Ear hole
                    actorClass.hearing = 0.25f;
                    actorClass.resourceConsumption += size * 0.05f;
                    description += "Ear hole";
                    break;
                case 2: // Large ear
                    actorClass.visibility += 0.3f;
                    actorClass.heatLimit += 10;
                    actorClass.coldLimit += 15;
                    actorClass.hearing = 0.5f;
                    actorClass.tracking += 0.35f;
                    actorClass.resourceConsumption += size * 0.2f;

                    actorClass.odor *= 1.25f;
                    actorClass.noise *= 1.25f;

                    actorClass.swimmingSpeed *= 0.6f;
                    description += "Large ears";
                    break;
                case 3: // Aimed ears
                    actorClass.coldLimit += 5;
                    actorClass.visibility += 0.15f;
                    actorClass.hearing = 0.65f;
                    actorClass.tracking += 1.5f;
                    actorClass.resourceConsumption += size * 0.3f;
                    actorClass.swimmingSpeed *= 0.8f;
                    description += "Aimed ears";
                    break;
                case 4: // Small ears
                    actorClass.coldLimit += 2;
                    actorClass.hearing = 0.35f;
                    actorClass.tracking += 0.25f;
                    actorClass.resourceConsumption += size * 0.15f;
                    actorClass.swimmingSpeed *= 0.9f;
                    description += "Small ears";
                    break;
            }
            #endregion

            #region Mouth
            int mouthType = GetGeneValue(CreatureGeneKeys.MouthType);
            description += "\nMouth type: ";
            float biteCrush = (4 + size * headSize * 0.65f);
            float bitePiercing = (4 + size * headSize * 0.65f);
            int biteAttackBonus = -10;
            switch (mouthType)
            {
                case 0: // Snout
                    description += "Snout";
                    actorClass.observationRange += 0.5f;
                    actorClass.smellSense = 0.5f;
                    actorClass.diggingSpeed += 1f;
                    biteCrush *= 0.75f;
                    bitePiercing *= 0.25f;
                    actorClass.divingSkill -= 3f;
                    actorClass.swimmingSpeed *= 0.8f;
                    break;
                case 1: // Trunk
                    description += "Trunk";
                    actorClass.evasion -= 10;
                    actorClass.observationRange += 0.25f;
                    actorClass.smellSense = 0.25f;
                    actorClass.diggingSpeed += 2f;
                    actorClass.climbingSpeed += 2f; // Trunk makes it easier to get food from trees
                    actorClass.swimmingSpeed *= 0.6f;
                    actorClass.meatConsumptionEfficiency -= 0.25f;
                    actorClass.plantConsumptionEfficiency += 0.25f;
                    actorClass.resourceConsumption += size * 0.1f;
                    biteAttackBonus += 5;
                    biteCrush *= 0.9f;
                    bitePiercing *= 0.1f;
                    actorClass.resourceConsumptionEnergyCost -= 3f;
                    actorClass.divingSkill -= 5f;

                    break;
                case 2: // Beak
                    description += "Beak";
                    actorClass.diggingSpeed += 0.5f;
                    actorClass.smellSense = 0.2f;
                    actorClass.meatConsumptionEfficiency += 0.2f;
                    actorClass.plantConsumptionEfficiency -= 0.33f;
                    actorClass.evasion -= 6;
                    biteCrush *= 0.25f;
                    bitePiercing *= 1f;
                    actorClass.resourceConsumption += size * 0.3f;
                    actorClass.resourceConsumptionEnergyCost += 3f;

                    biteAttackBonus += 5;

                    break;
                case 3: // Fangs
                    description += "Fangs";
                    actorClass.smellSense = 0.5f;
                    actorClass.observationRange += 0.25f;
                    actorClass.meatConsumptionEfficiency += 0.5f;
                    actorClass.plantConsumptionEfficiency -= 0.5f;
                    biteCrush *= 0.75f;
                    bitePiercing *= 0.75f;
                    break;

            }

            Attack bite = new Attack("Bite", biteAttackBonus, DamageTypes.Crushing, Mathf.CeilToInt(biteCrush), Mathf.CeilToInt(biteCrush));

            bite.AddDamage(new Attack.Damage(DamageTypes.Piercing, Mathf.CeilToInt(bitePiercing), Mathf.CeilToInt(bitePiercing)));

            actorClass.AddAttacks(bite);
            #endregion

            // Base color and pattern color
            actorClass.baseColor = new Color(GetGeneValue(CreatureGeneKeys.BaseColorRed) / 255f, GetGeneValue(CreatureGeneKeys.BaseColorGreen) / 255f, GetGeneValue(CreatureGeneKeys.BaseColorBlue) / 255f);
            actorClass.patternColor = new Color(GetGeneValue(CreatureGeneKeys.PatternColorRed) / 255f, GetGeneValue(CreatureGeneKeys.PatternColorGreen) / 255f, GetGeneValue(CreatureGeneKeys.PatternColorBlue) / 255f);

            #region Attacks
            // Kick
            kickDamage *= size * (1 + limbLength * 0.25f + limbThickness * 0.5f);
            float kickDamageBonus = kickDamage;
            Attack kick = new Attack("Kick", Mathf.CeilToInt(limbLength * 2), DamageTypes.Crushing, Mathf.CeilToInt(kickDamage), Mathf.CeilToInt(kickDamageBonus));

            // Add extra slashing damage for claws
            if (feetType == 3)
            {
                kick.AddDamage(new Attack.Damage(DamageTypes.Slashing, Mathf.CeilToInt(kickDamage), Mathf.CeilToInt(kickDamageBonus)));
            }

            actorClass.AddAttacks(kick);


            #endregion

            actorClass.divingSkill = Mathf.Max(0, actorClass.divingSkill);
            actorClass.Initialize();

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

        public CreatureGene(CreatureGene original)
        {
            Min = original.Min;
            Max = original.Max;
            Value = original.Value;

            Range = original.Range;
            Ratio = original.Ratio;

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

            if (Range == 0)
            {
                Ratio = 1;
            }
            else
            {
                Ratio = (Value - Min) / (float)Range;
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public enum CreatureGeneKeys
    {
        Size, // 1 - 10
        SkinType, // 0 for hide, 1 for scales, 2 for feathers, 3 for fur
        HeadSize, // 10-50
        EyeNumber, // 0-10
        EyeSize, // 1-100
        MouthType, // 0=Sharp teeth,1=blunt teeth, 2=beak, 3=trunk
        EarType,
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