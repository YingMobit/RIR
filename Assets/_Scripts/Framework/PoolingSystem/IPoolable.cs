using System;

public interface IPoolable : IDisposable {
    //此项来自于PoolableObjectTypeCollection中的常量
    public int PoolableType { get; }
}

public static class PoolableObjectTypeCollection {
    public const int GameObject = 0;
    public const int Task = 1;
    public const int BlackBoard = 2;
}