/// <summary>
/// 任务执行情况，用于任务层<-->执行层，执行层<-->技能控制器层的通信
/// </summary>
public enum TaskStatus
{
    ToBeContinue,
    Success,
    Failed
}