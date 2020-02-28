using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Utility
{
    [System.Serializable]
    public struct KeyValue<K, V>
    {
        public K Key;
        public V Value;

        public KeyValue(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }
}