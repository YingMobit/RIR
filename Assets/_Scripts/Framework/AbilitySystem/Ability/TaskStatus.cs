namespace AbilitySystem{ 
    internal enum TaskStatus { 
        UnStart,
        Running,
        Suceeded,
        Failed,
        Finished = Suceeded|Failed
    }
}
