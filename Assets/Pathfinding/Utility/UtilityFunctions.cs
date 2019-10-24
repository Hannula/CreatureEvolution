using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class Range
    {
        public float Min;
        public float Max;
        public Range(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float GetRandom()
        {
            return UnityEngine.Random.Range(Min, Max);
        }

    }
    /// <summary>
    /// Utility Functions.
    /// </summary>
    public static class UtilityFunctions
    {
        /// <summary>
        /// Shuffles a list in place.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T element = list[i];
                int newPosition = UnityEngine.Random.Range(0, list.Count);
                T otherElement = list[newPosition];
                list[i] = otherElement;
                list[newPosition] = element;
            }
        }

        /// <summary>Creates a shuffled copy of a list. Does not modify the original list.</summary>
        public static List<T> Shuffled<T>(this IList<T> list)
        {
            List<T> newList = new List<T>(list);
            Shuffle(newList);
            return newList;
        }

        /// <summary>Pops out the first element in the list.</summary>
        public static T Pop<T>(this IList<T> list)
        {
            if (list != null && list.Count > 0)
            {
                T element = list[0];
                list.RemoveAt(0);
                return element;
            }
            return default(T);


        }

        /// <summary>Returns a random element from the list.</summary>
        public static T GetRandomElement<T>(this IList<T> list, T defaultValue = default(T))
        {
            if (list.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, list.Count);
                return list[index];
            }
            return defaultValue;
        }

        /// <summary>Makes the string start with capital letter.</summary>
        public static string CapitalizeFirst(this string str)
        {
            if (String.IsNullOrEmpty(str))
                return str;
            if (str.Length == 1)
                return str.ToUpper();
            return str.Remove(1).ToUpper() + str.Substring(1);
        }
    }
}
