using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;
/// <summary>
/// Buff构建工具类，用于加载Buff配置数据
/// </summary>
public class BuffConfigDataLoader : Singleton<BuffConfigDataLoader> {
    private const string BuffConfigDataRootPath = "ScriptableObject/Buff/BuffConfigData";
    private BuffConfigData[] ConfigDatas;

    private bool initialized = false;

    public BuffConfigData LoadBuffConfigData(int id) {
        if(!initialized)
            Init();
        BuffConfigData template = GetBuffConfigData(id);
        BuffConfigData data = new();
        template.CopyTo(data);
        return data;
    }

    protected override void Awake() {
        base.Awake();
        if(!initialized) {
            Init();
        }
    }

    private void Init() {
        ConfigDatas = Resources.LoadAll<BuffConfigData>(BuffConfigDataRootPath);
        ConfigDatas = ConfigDatas.OrderBy(data => data.ID).ToArray();
        initialized = true;
    }

    private BuffConfigData GetBuffConfigData(int id) {
        int left = 0, right = ConfigDatas.Length - 1;
        while(left <= right) {
            if(id < ConfigDatas[(right + left) / 2].ID) {
                right = (right + left) / 2 - 1;
            } else if(id > ConfigDatas[(right + left) / 2].ID) {
                left = (right + left) / 2 + 1;
            } else {
                return ConfigDatas[(right + left) / 2];
            }
        }

        return null;
    }
}