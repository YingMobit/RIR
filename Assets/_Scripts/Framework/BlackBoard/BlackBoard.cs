using PoolingSystem.ReferencePool;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class BlackBoard : IReference<BlackBoard> {
    #region Struct Define
    private struct UnmanagedValueHead {
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
        for(int i=0;i < data.Length; i++) {
            data[i] = repository[headInfo.Align + i];
        }
        UnmanagedValueType value;
        value = MemoryMarshal.Read<UnmanagedValueType>(data);
        return value;
    }
}