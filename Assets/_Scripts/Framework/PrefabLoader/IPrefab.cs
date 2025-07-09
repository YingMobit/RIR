using System;
using UnityEngine;

public interface IPrefab<PrefabType> : IPrefab
{
    public PrefabType Copy();
}

public interface IPrefab{
    public Type Type { get; }
}
