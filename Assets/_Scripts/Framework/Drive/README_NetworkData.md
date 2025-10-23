# NetworkData 序列化使用指南

## ?? 概述

`NetworkData` 现在支持完整的序列化功能，可用于帧同步网络传输。

## ??? 架构设计

### 双层数据结构

```
运行时层 (NetworkData)          序列化层 (NetworkDataPacket)
─────────────────────────────    ─────────────────────────────
Dictionary<int, InputQueue>  →   int[] + PlayerInputData[]
高性能，不可序列化                可序列化，用于网络传输
```

## ?? 使用流程

### 1. 服务器端发送

```csharp
// 创建帧数据
NetworkData frameData = new NetworkData {
    NetworkFrameCount = 100
};

// 添加玩家输入
frameData.PlayerInputs[playerId] = inputQueue;

// 序列化并发送
byte[] data = frameData.ToPacket().Serialize();
networkLibrary.Send(data);
```

### 2. 客户端接收

```csharp
// 接收字节数组
byte[] receivedData = networkLibrary.Receive();

// 反序列化
NetworkDataPacket packet = NetworkDataPacket.Deserialize(receivedData);
NetworkData frameData = packet.ToNetworkData();

// 使用数据
foreach (var kvp in frameData.PlayerInputs) {
    int playerId = kvp.Key;
    InputQueue inputs = kvp.Value;
    // 处理输入...
}
```

### 3. 与 Driver 集成

```csharp
// 在 Driver.cs 中
public class Driver : Singleton<Driver> {
    private NetworkBridge networkBridge = new NetworkBridge();
    
    protected override void Awake() {
        base.Awake();
        
        // 订阅网络帧确认事件
        NetworkBridge.OnFrameConfirmed += OnServerFrameConfirmed;
    }
    
    void OnUpdate(long localFrameCount, double deltaTime) {
        // 处理接收的网络数据
        networkBridge.ProcessReceivedData();
        
        // 正常的帧更新
        OnLogicUpdate((int)localFrameCount, (float)deltaTime);
        OnLateLogicUpdate((int)localFrameCount, (float)deltaTime);
    }
    
    // 当收到服务器确认的帧数据时调用
    void OnServerFrameConfirmed(NetworkData frameData) {
        currentNetworkFrame = frameData.NetworkFrameCount;
        world.OnNetworkUpdate(frameData.NetworkFrameCount);
        
        // 分发输入到各个玩家的 InputComponent
        ApplyNetworkInputs(frameData);
    }
}
```

## ?? 数据结构说明

### NetworkData (运行时)
- `int NetworkFrameCount`: 网络帧号
- `Dictionary<int, InputQueue> PlayerInputs`: 玩家ID -> 输入队列

### NetworkDataPacket (序列化)
- `int NetworkFrameCount`: 网络帧号
- `int[] PlayerIDs`: 玩家ID数组
- `PlayerInputData[] PlayerInputs`: 对应的输入数据数组

### PlayerInputData
- `FrameInputData[] Inputs`: 该玩家的所有输入帧数据

## ?? 序列化格式

使用 Unity 的 `JsonUtility`，生成的 JSON 示例：

```json
{
    "NetworkFrameCount": 100,
    "PlayerIDs": [1, 2, 3],
    "PlayerInputs": [
        {
            "Inputs": [
                {
                    "KeyCodeinputs": 1,
                    "LocalFrameCount": 300,
                    "NetworkFrameCount": 100,
                    "AimDirection": {"x":0,"y":0,"z":1}
                }
            ]
        },
        // ... 其他玩家
    ]
}
```

## ? 性能优化建议

### 1. 增量同步（推荐）
不要发送完整的 InputQueue，只发送当前帧的输入：

```csharp
// 服务器端
NetworkData frameData = new NetworkData {
    NetworkFrameCount = currentFrame
};

foreach (var player in players) {
    var queue = new InputQueue();
    queue.EnQueue(player.GetCurrentFrameInput());
    frameData.PlayerInputs[player.Id] = queue;
}
```

### 2. 使用二进制序列化（可选）
如果需要更高性能，可以替换为二进制序列化：

```csharp
// 在 NetworkDataPacket 中添加
public byte[] SerializeBinary() {
    using (var stream = new MemoryStream()) {
        var formatter = new BinaryFormatter();
        formatter.Serialize(stream, this);
        return stream.ToArray();
    }
}
```

### 3. 压缩（可选）
对于低带宽场景，可以添加压缩：

```csharp
using System.IO.Compression;

public byte[] SerializeCompressed() {
    byte[] json = Serialize();
    using (var output = new MemoryStream()) {
        using (var gzip = new GZipStream(output, CompressionMode.Compress)) {
            gzip.Write(json, 0, json.Length);
        }
        return output.ToArray();
    }
}
```

## ?? 调试技巧

### 查看序列化大小
```csharp
byte[] data = frameData.ToPacket().Serialize();
Debug.Log($"Frame {frameData.NetworkFrameCount} size: {data.Length} bytes");
```

### 验证数据完整性
```csharp
NetworkData original = CreateFrameData();
byte[] serialized = original.ToPacket().Serialize();
NetworkData deserialized = NetworkDataPacket.Deserialize(serialized).ToNetworkData();

bool isValid = original.NetworkFrameCount == deserialized.NetworkFrameCount &&
               original.PlayerInputs.Count == deserialized.PlayerInputs.Count;
```

## ?? 注意事项

1. **Vector3 序列化**：Unity 的 `Vector3` 已标记为 `[Serializable]`，可以直接序列化
2. **InputQueue 容量**：反序列化时会创建默认容量(180)的队列
3. **空数据处理**：如果 `PlayerInputs` 为 null 或空，序列化后数组为空
4. **线程安全**：`NetworkBridge` 的队列操作不是线程安全的，需要在主线程调用

## ?? 完整示例

参见 `NetworkDataExample.cs` 获取完整的使用示例。
