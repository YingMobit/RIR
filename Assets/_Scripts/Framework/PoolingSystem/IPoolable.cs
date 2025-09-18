using System;

public interface IPoolable : IDisposable {
    /// <summary>
    /// 此项来自于PoolableObjectTypeCollection中的常量
    /// </summary>
    public int PoolableType { get; }
}

public static class PoolableObjectTypeCollection {
    public const int GameObject = 0;
    public const int AbilityExcutionTask = 1;
    public const int BlackBoard = 2;
    public const int AbilityRuntimeContext = 3;
}