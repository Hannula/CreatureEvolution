using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metaball : MonoBehaviour {

    public float innerRadius = 1f;
    public float featherRadius = 1f;
    public AnimationCurve featherCurve;
    public Color color;

    public float radius {
        get { return innerRadius + featherRadius; }
    }

    private void OnDrawGizmos()
    {
        // Draw radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, innerRadius);
    }

    public float ValueAt(Vector2 worldPosition)
    {
        float value = 0;
        float dist = Vector2.Distance(transform.position, worldPosition);

        if (dist < innerRadius)
        {
            // Return 1 if the point is inside the inner radius
            value = 1f;
        }
        else if (dist < radius)
        {
            // Use feather curve to calculate the value if the point is withing the feather radius
            value = featherCurve.Evaluate((dist - innerRadius) / featherRadius);
        }

        return value;
    }
}
