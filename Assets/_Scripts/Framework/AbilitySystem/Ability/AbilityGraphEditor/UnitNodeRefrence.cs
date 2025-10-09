using Sirenix.OdinInspector;
using System;

namespace GAS.Editor.AbilityEditor {
    [Serializable]
    public class UnitNodeRefrence {
        public int RunTimeToken;
        public UnitNodeRefrence(int runTimeToken) {
            RunTimeToken = runTimeToken;
        }
    }

    [Serializable]
    public struct UnitNodeRefrenceRecord {
        [ReadOnly] public string FieldName;
        public UnitNodeRefrence RunTimeToken;

        public UnitNodeRefrenceRecord(string fieldName,UnitNodeRefrence runtimeToken) { 
            FieldName = fieldName;
            RunTimeToken = runtimeToken;
        }
    }
}