using System;
using UnityEngine;

/// <summary>
/// 预制体接口
/// </summary>
/// <typeparam name="PrefabType">可以唯一标识这个预制体的类型</typeparam>
public interface IPrefab<PrefabType> : IPrefab {
    public string TagName { get => "Defualt"; }
    public PrefabType Copy();
}

public interface IPrefab {
    public Type Type { get; }
}
