using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// �����л����ֵ�ʵ��
[System.Serializable]
public class SerializableDictionary<TKey, TValue> {
    [SerializeField,OdinSerialize] List<TKey> keys = new List<TKey>();
    [SerializeField,OdinSerialize] List<TValue> values = new List<TValue>();
    private Dictionary<TKey, TValue> dictionary;
    public Dictionary<TKey,TValue> Dictionary {
        get {
            if(dictionary == null) {
                if(keys.Count != values.Count) {
                    Debug.LogError("Keys and values count do not match.");
                    return null;
                }
                dictionary = new Dictionary<TKey,TValue>();
                for(int i = 0; i < keys.Count; i++) {
                    if(!dictionary.ContainsKey(keys[i])) {
                        dictionary.Add(keys[i],values[i]);
                    }
                }
            }
            return dictionary;
        }
    }
}