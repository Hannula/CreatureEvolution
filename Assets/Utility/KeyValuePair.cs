using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct KeyValuePair<K, V>
{
    public K Key { get; set; }
    public V Value { get; set; }
}