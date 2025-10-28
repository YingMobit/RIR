using Drive;
using Drive.Serialization;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ClientWaitOutofRoomState : ClientState {
    bool idAllocated = false;
    bool started = false;
    Stopwatch stopwatch;

    public override void EnterState(ClientFSM clientFSM, ClientFSMContext context) {
        stopwatch = new();
        stopwatch.Start();
    }

    public override void UpdateState(ClientFSM clientFSM,ClientFSMContext context) {
        foreach(var message in context.networkMessage) {
            if(message.NetworkMessageType == NetworkMessageType.CommandMessage) {
                var command = ProtobufSerializer.Deserialize<NetworkCommandMessgae>(message.DataStream);
                if(command.CommandID == NetworkCommandMessgae.NETWORKCOMMAND_HOSTASSIGNMENT) {
                    NetworkManager.Instance.IsHost = true;
                } else if(command.CommandID == NetworkCommandMessgae.NETWORKCOMMAND_HOSTSTARTGAME) {
                    if(idAllocated) {
                        clientFSM.SwitchState(ClientEnum.CharactorChoose);
                    } else {
                        Debug.LogError("PlayerID not allocated yet, cannot switch to CharactorChoose state");
                    }
                } 
            } else if(message.NetworkMessageType == NetworkMessageType.PlayerIDAllocationMessage) {
                GlobalEventCenter.Instance.Invoke<IRecivedPlayerIDAllocationEventData>(new RecivedPlayerIDAllocationEventData(
                    ProtobufSerializer.Deserialize<NetworkPlayerIDAllocationMessage>(message.DataStream).PlayerID)
                );
                idAllocated = true;
            } else {
                Debug.LogError($"Unsupported message type in this serverstate: {message.NetworkMessageType},current state: {ClientEnum.WaitOutofRoom}");
            }
        }

        if(stopwatch.ElapsedMilliseconds >= 15000 && NetworkManager.Instance.IsHost && !started) { 
            started = true;
            NetworkManager.Instance.SendNetworkMessage(new NetworkMessage() {
                NetworkMessageType = NetworkMessageType.CommandMessage,
                DataStream = ProtobufSerializer.Serialize(new NetworkCommandMessgae() { CommandID = NetworkCommandMessgae.NETWORKCOMMAND_HOSTSTARTGAME })
            });
        }
    }

    public override void ExitState(ClientFSM clientFSM, ClientFSMContext context) {

    }
}

public interface IRecivedPlayerIDAllocationEventData : IEventData {
    int PlayerID { get; }
}

public struct RecivedPlayerIDAllocationEventData : IRecivedPlayerIDAllocationEventData {
    public int PlayerID { get; private set; }
    public RecivedPlayerIDAllocationEventData(int playerID) {
        PlayerID = playerID;
    }
}

