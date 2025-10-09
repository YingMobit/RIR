using System;

namespace GAS {
    [Flags]
    public enum TaskStatus { 
        UnStart=1,
        Running=1<<1,
        Suceeded=1<<2,
        Failed=1<<3,
        Finished = Suceeded|Failed
    }

    public static class TaskStatusExtension {
        public static bool IsFinished(this TaskStatus taskStatus) {
            return (taskStatus & TaskStatus.Finished) != 0;
        }
    }
}
