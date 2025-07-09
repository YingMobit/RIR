using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InputKeyConfig",menuName = "ScriptableObject/InputKeyConfig",order = 1)]
public class InputKeyConfig : ScriptableObject {
    public SerializableDictionary<PlayerInputType,KeyCode> Configs = new();
}
