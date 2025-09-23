using System;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAbilityConfigData",menuName = "ScriptableObject/PlayerAbilityConfigData",order = 3)]
public class PlayerAttributeConfigData : ScriptableObject {
    [Header("ÒÆ¶¯")]
    public float WalkSpeed;
    public float RunSpeed;
    public float JumpHeight;
    public float JumpVerticalImpulse;
}
