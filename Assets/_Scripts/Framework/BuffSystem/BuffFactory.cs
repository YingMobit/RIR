using Utility;
/// <summary>
/// Buff工厂类
/// </summary>
public class BuffFactory : Singleton<BuffFactory> {
    public TBuff CreateBuff<TBuff>(int id,BuffRunTimeData runTimeData) where TBuff : Buff, new() {
        BuffConfigData configData = BuffConfigDataLoader.Instance.LoadBuffConfigData(id);
        TBuff buff = new();
        runTimeData.ActualDuration = configData.Duration;
        buff.RunTimeData = runTimeData;
        buff.ConfigData = configData;
        return buff;
    }
}