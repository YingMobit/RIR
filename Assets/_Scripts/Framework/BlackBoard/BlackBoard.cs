using PoolingSystem.ReferencePool;
using RollBackSystem;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Pool;

public class BlackBoard : IReference<BlackBoard> , IRollBackable {
    #region Struct Define
    internal struct UnmanagedValueHead {
        public int TypeID;
        public int Size;
        public int Align;

        public UnmanagedValueHead(int typeID,int size,int align) {
            TypeID = typeID;
            Size = size;
            Align = align;
        }
    }
    #endregion
    #region IReference

    public uint ReferenceType => ReferenceTypes.BLACKBOARD;

    int IReference.IndexInRefrencePool { get; set; }
    public void OnRecycle() {
        ManagedFields.Clear();
        UnmanagedFields.Clear();
        repository.Clear();
        currentRepositorySize = 0;
    }

    public IReference GetNewInstance() {
        return new BlackBoard();
    }

    public void Dispose() {
        OnRecycle();
        ManagedFields = null;
        UnmanagedFields = null;
        repository = null;
    }
    #endregion 

    Dictionary<int,object> ManagedFields = new();
    Dictionary<int,UnmanagedValueHead> UnmanagedFields = new();
    static readonly Dictionary<Type,int> TypeIDMap = new();
    List<byte> repository=new(256);
    int currentRepositorySize = 0;
    static int currentTypeID = 0;

    #region API
    public void Set<ManagedFieldType>(int id,ManagedFieldType newValue,object _ = null) where ManagedFieldType : class {
        if(ManagedFields.ContainsKey(id)) {
            var originValue = ManagedFields[id] as ManagedFieldType;
            if(originValue != null) {
                originValue = newValue;
                ManagedFields[id] = originValue;
            } else {
                Debug.LogError($"BlackBoard Set Error: Type Mismatch, ID:{id}, Expect:{originValue.GetType().Name}, Given:{newValue.GetType().Name}");
            }
        } else {
            ManagedFields.Add(id,newValue);
        }
    }

    public void Set<UnManagedFieldType>(int id,UnManagedFieldType newValue) where UnManagedFieldType : unmanaged {
        if(UnmanagedFields.ContainsKey(id)) {
            var headInfo = UnmanagedFields[id];
            if(headInfo.TypeID != TypeIDMap[typeof(UnManagedFieldType)]) {
                Debug.LogError($"BlackBoard Set Error: Type Mismatch, ID:{id}, Expect:{headInfo.TypeID}, Given:{TypeIDMap[typeof(UnManagedFieldType)]}");
                return;
            }
            WriteUnManagedFields(headInfo,ref newValue);
        } else {
            int typeID = GetTypeID<UnManagedFieldType>();
            int size = Unsafe.SizeOf<UnManagedFieldType>();
            int align = currentRepositorySize;
            currentRepositorySize += size;
            var newHeadInfo = new UnmanagedValueHead(typeID,size,align);
            WriteUnManagedFields(newHeadInfo,ref newValue);
            UnmanagedFields.Add(id,newHeadInfo);
        }
    }

    public ManagedFieldType Get<ManagedFieldType>(int id,object _ = null) where ManagedFieldType : class {
        if(ManagedFields.ContainsKey(id)) {
            var obj = ManagedFields[id];
            ManagedFieldType value;
            try {
                value = (ManagedFieldType)obj;
            } catch(InvalidCastException) {
                Debug.LogError($"BlackBoard Get Error: Type Mismatch, ID:{id}, Expect:{typeof(ManagedFieldType).Name}, Given:{obj.GetType().Name}");
                return null;
            }
            return value;
        } else {
            Debug.LogError($"BlackBoard Get Error: ID Not Found, ID:{id}");
            return null;
        }
    }

    public UnmanagedFiledType Get<UnmanagedFiledType>(int id) where UnmanagedFiledType : unmanaged {
        if(UnmanagedFields.ContainsKey(id)) {
            var headInfo = UnmanagedFields[id];
            if(headInfo.TypeID != TypeIDMap[typeof(UnmanagedFiledType)]) {
                Debug.LogError($"BlackBoard Get Error: Type Mismatch, ID:{id}, Expect:{headInfo.TypeID}, Given:{TypeIDMap[typeof(UnmanagedFiledType)]}");
                return default;
            }
            return ReadUnManagedFields<UnmanagedFiledType>(headInfo);
        } else {
            Debug.LogError($"BlackBoard Get Error: ID Not Found, ID:{id}");
            return default;
        }
    }

    private static int GetTypeID<Type>() where Type : unmanaged {
        System.Type type = typeof(Type);
        if(TypeIDMap.ContainsKey(type)) {
            return TypeIDMap[type];
        } else {
            TypeIDMap.Add(type,currentTypeID);
            return currentTypeID++;
        }
    }

    private void WriteUnManagedFields<UnmanagedValueType>(in UnmanagedValueHead headInfo,ref UnmanagedValueType newValue) where UnmanagedValueType : unmanaged {
        Span<byte> data = headInfo.Size <= 256 ? stackalloc byte[headInfo.Size] : new byte[headInfo.Size];
        MemoryMarshal.Write(data,ref newValue);
        for(int i = 0; i < data.Length; i++) {
            if(headInfo.Align + i < repository.Count) {
                repository[headInfo.Align + i] = data[i];
            } else {
                repository.Add(data[i]);
            }
        }
    }

    private UnmanagedValueType ReadUnManagedFields<UnmanagedValueType>(in UnmanagedValueHead headInfo) where UnmanagedValueType : unmanaged {
        Span<byte> data = headInfo.Size <= 256 ? stackalloc byte[headInfo.Size] : new byte[headInfo.Size];
        for(int i = 0; i < data.Length; i++) {
            data[i] = repository[headInfo.Align + i];
        }
        UnmanagedValueType value;
        value = MemoryMarshal.Read<UnmanagedValueType>(data);
        return value;
    }
    #endregion

    #region IRollBackable
    internal class BlackBoardSnapShot : ISnapShot, IReference<BlackBoardSnapShot> {
        // 拆分为键值对列表
        internal List<int> ManagedFieldKeysCopy;
        internal List<object> ManagedFieldValuesCopy;
        internal List<int> UnmanagedFieldKeysCopy;
        internal List<UnmanagedValueHead> UnmanagedFieldValuesCopy;
        internal List<byte> repositoryCopy;
        internal int currentRepositoryCopy;
        public int LocalizedLogicFrameCount { get; set; }

        public uint ReferenceType => ReferenceTypes.BLACKBOARDSNAPSHOT;

        int IReference.IndexInRefrencePool { get; set; }

        public void Release() {
            ReferencePoolingCenter.Instance.ReleaseReference(this);
        }

        public void Dispose() {
            OnRecycle();
        }

        public IReference GetNewInstance() {
            var res = new BlackBoardSnapShot();
            res.ManagedFieldKeysCopy = ListPool<int>.Get();
            res.ManagedFieldValuesCopy = ListPool<object>.Get();
            res.UnmanagedFieldKeysCopy = ListPool<int>.Get();
            res.UnmanagedFieldValuesCopy = ListPool<UnmanagedValueHead>.Get();
            res.repositoryCopy = ListPool<byte>.Get();
            return res;
        }

        public void OnRecycle() {
            if(ManagedFieldKeysCopy != null) {
                ListPool<int>.Release(ManagedFieldKeysCopy);
                ManagedFieldKeysCopy = null;
            }
            if(ManagedFieldValuesCopy != null) {
                ListPool<object>.Release(ManagedFieldValuesCopy);
                ManagedFieldValuesCopy = null;
            }
            if(UnmanagedFieldKeysCopy != null) {
                ListPool<int>.Release(UnmanagedFieldKeysCopy);
                UnmanagedFieldKeysCopy = null;
            }
            if(UnmanagedFieldValuesCopy != null) {
                ListPool<UnmanagedValueHead>.Release(UnmanagedFieldValuesCopy);
                UnmanagedFieldValuesCopy = null;
            }
            if(repositoryCopy != null) {
                ListPool<byte>.Release(repositoryCopy);
                repositoryCopy = null;
            }
        }
    }

    public ISnapShot SnapShot(int localizedLogicFrameCount) {
        var blackboardSnapShot = ReferencePoolingCenter.Instance.GetReference<BlackBoardSnapShot>();
        blackboardSnapShot.LocalizedLogicFrameCount = localizedLogicFrameCount;

        // 快照非托管字段的数据仓库
        if(blackboardSnapShot.repositoryCopy.Capacity < repository.Capacity)
            blackboardSnapShot.repositoryCopy.Capacity = repository.Capacity;
        blackboardSnapShot.repositoryCopy.Clear();
        blackboardSnapShot.repositoryCopy.AddRange(repository);
        blackboardSnapShot.currentRepositoryCopy = currentRepositorySize;

        // 预扩容并保存托管字段的引用关系
        if(blackboardSnapShot.ManagedFieldKeysCopy.Capacity < ManagedFields.Count) {
            blackboardSnapShot.ManagedFieldKeysCopy.Capacity = ManagedFields.Count;
            blackboardSnapShot.ManagedFieldValuesCopy.Capacity = ManagedFields.Count;
        }
        blackboardSnapShot.ManagedFieldKeysCopy.Clear();
        blackboardSnapShot.ManagedFieldValuesCopy.Clear();
        foreach(var pair in ManagedFields) {
            blackboardSnapShot.ManagedFieldKeysCopy.Add(pair.Key);
            blackboardSnapShot.ManagedFieldValuesCopy.Add(pair.Value);
        }

        // 预扩容并快照非托管字段的头信息
        if(blackboardSnapShot.UnmanagedFieldKeysCopy.Capacity < UnmanagedFields.Count) {
            blackboardSnapShot.UnmanagedFieldKeysCopy.Capacity = UnmanagedFields.Count;
            blackboardSnapShot.UnmanagedFieldValuesCopy.Capacity = UnmanagedFields.Count;
        }
        blackboardSnapShot.UnmanagedFieldKeysCopy.Clear();
        blackboardSnapShot.UnmanagedFieldValuesCopy.Clear();
        foreach(var pair in UnmanagedFields) {
            blackboardSnapShot.UnmanagedFieldKeysCopy.Add(pair.Key);
            blackboardSnapShot.UnmanagedFieldValuesCopy.Add(pair.Value);
        }

        return blackboardSnapShot;
    }

    public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
        var blackboardSnapShot = snapShot as BlackBoardSnapShot;
        if(blackboardSnapShot == null) {
            Debug.LogError("BlackBoard RollBack Error: Invalid SnapShot Type");
            return;
        }

        // 回滚非托管字段的数据仓库
        repository.Clear();
        repository.AddRange(blackboardSnapShot.repositoryCopy);
        currentRepositorySize = blackboardSnapShot.currentRepositoryCopy;

        // 回滚托管字段的引用关系（不回滚对象内部状态）
        ManagedFields.Clear();
        for(int i = 0; i < blackboardSnapShot.ManagedFieldKeysCopy.Count; i++) {
            ManagedFields.Add(blackboardSnapShot.ManagedFieldKeysCopy[i], blackboardSnapShot.ManagedFieldValuesCopy[i]);
        }

        // 回滚非托管字段
        UnmanagedFields.Clear();
        for(int i = 0; i < blackboardSnapShot.UnmanagedFieldKeysCopy.Count; i++) {
            UnmanagedFields.Add(blackboardSnapShot.UnmanagedFieldKeysCopy[i], blackboardSnapShot.UnmanagedFieldValuesCopy[i]);
        }
    }
    #endregion
}