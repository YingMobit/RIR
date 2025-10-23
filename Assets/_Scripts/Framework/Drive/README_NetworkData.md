# NetworkData ���л�ʹ��ָ��

## ?? ����

`NetworkData` ����֧�����������л����ܣ�������֡ͬ�����紫�䡣

## ??? �ܹ����

### ˫�����ݽṹ

```
����ʱ�� (NetworkData)          ���л��� (NetworkDataPacket)
����������������������������������������������������������    ����������������������������������������������������������
Dictionary<int, InputQueue>  ��   int[] + PlayerInputData[]
�����ܣ��������л�                �����л����������紫��
```

## ?? ʹ������

### 1. �������˷���

```csharp
// ����֡����
NetworkData frameData = new NetworkData {
    NetworkFrameCount = 100
};

// ����������
frameData.PlayerInputs[playerId] = inputQueue;

// ���л�������
byte[] data = frameData.ToPacket().Serialize();
networkLibrary.Send(data);
```

### 2. �ͻ��˽���

```csharp
// �����ֽ�����
byte[] receivedData = networkLibrary.Receive();

// �����л�
NetworkDataPacket packet = NetworkDataPacket.Deserialize(receivedData);
NetworkData frameData = packet.ToNetworkData();

// ʹ������
foreach (var kvp in frameData.PlayerInputs) {
    int playerId = kvp.Key;
    InputQueue inputs = kvp.Value;
    // ��������...
}
```

### 3. �� Driver ����

```csharp
// �� Driver.cs ��
public class Driver : Singleton<Driver> {
    private NetworkBridge networkBridge = new NetworkBridge();
    
    protected override void Awake() {
        base.Awake();
        
        // ��������֡ȷ���¼�
        NetworkBridge.OnFrameConfirmed += OnServerFrameConfirmed;
    }
    
    void OnUpdate(long localFrameCount, double deltaTime) {
        // ������յ���������
        networkBridge.ProcessReceivedData();
        
        // ������֡����
        OnLogicUpdate((int)localFrameCount, (float)deltaTime);
        OnLateLogicUpdate((int)localFrameCount, (float)deltaTime);
    }
    
    // ���յ�������ȷ�ϵ�֡����ʱ����
    void OnServerFrameConfirmed(NetworkData frameData) {
        currentNetworkFrame = frameData.NetworkFrameCount;
        world.OnNetworkUpdate(frameData.NetworkFrameCount);
        
        // �ַ����뵽������ҵ� InputComponent
        ApplyNetworkInputs(frameData);
    }
}
```

## ?? ���ݽṹ˵��

### NetworkData (����ʱ)
- `int NetworkFrameCount`: ����֡��
- `Dictionary<int, InputQueue> PlayerInputs`: ���ID -> �������

### NetworkDataPacket (���л�)
- `int NetworkFrameCount`: ����֡��
- `int[] PlayerIDs`: ���ID����
- `PlayerInputData[] PlayerInputs`: ��Ӧ��������������

### PlayerInputData
- `FrameInputData[] Inputs`: ����ҵ���������֡����

## ?? ���л���ʽ

ʹ�� Unity �� `JsonUtility`�����ɵ� JSON ʾ����

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
        // ... �������
    ]
}
```

## ? �����Ż�����

### 1. ����ͬ�����Ƽ���
��Ҫ���������� InputQueue��ֻ���͵�ǰ֡�����룺

```csharp
// ��������
NetworkData frameData = new NetworkData {
    NetworkFrameCount = currentFrame
};

foreach (var player in players) {
    var queue = new InputQueue();
    queue.EnQueue(player.GetCurrentFrameInput());
    frameData.PlayerInputs[player.Id] = queue;
}
```

### 2. ʹ�ö��������л�����ѡ��
�����Ҫ�������ܣ������滻Ϊ���������л���

```csharp
// �� NetworkDataPacket �����
public byte[] SerializeBinary() {
    using (var stream = new MemoryStream()) {
        var formatter = new BinaryFormatter();
        formatter.Serialize(stream, this);
        return stream.ToArray();
    }
}
```

### 3. ѹ������ѡ��
���ڵʹ��������������ѹ����

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

## ?? ���Լ���

### �鿴���л���С
```csharp
byte[] data = frameData.ToPacket().Serialize();
Debug.Log($"Frame {frameData.NetworkFrameCount} size: {data.Length} bytes");
```

### ��֤����������
```csharp
NetworkData original = CreateFrameData();
byte[] serialized = original.ToPacket().Serialize();
NetworkData deserialized = NetworkDataPacket.Deserialize(serialized).ToNetworkData();

bool isValid = original.NetworkFrameCount == deserialized.NetworkFrameCount &&
               original.PlayerInputs.Count == deserialized.PlayerInputs.Count;
```

## ?? ע������

1. **Vector3 ���л�**��Unity �� `Vector3` �ѱ��Ϊ `[Serializable]`������ֱ�����л�
2. **InputQueue ����**�������л�ʱ�ᴴ��Ĭ������(180)�Ķ���
3. **�����ݴ���**����� `PlayerInputs` Ϊ null ��գ����л�������Ϊ��
4. **�̰߳�ȫ**��`NetworkBridge` �Ķ��в��������̰߳�ȫ�ģ���Ҫ�����̵߳���

## ?? ����ʾ��

�μ� `NetworkDataExample.cs` ��ȡ������ʹ��ʾ����
