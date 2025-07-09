using System;
using UnityEngine;

public interface IPoolable
{
    Type Type { get; }
    public GameObject Entity { get; }
}
