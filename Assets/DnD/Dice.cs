using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice
{
    /// <summary>
    /// Throw n-sided dice x number of times
    /// </summary>
    /// <param name="sides">Number of sides</param>
    /// <param name="number">Number of throws</param>
    /// <returns>Total</returns>
    public static int Roll(int sides, int number=1)
    {
        int result = 0;

        // Throw "number" times
        for (int i = 0; i < number; i++)
        {
            result += Random.Range(1, sides + 1);
        }

        return result;
    }
}
