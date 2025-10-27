using Drive;
using kcp2k;
using System.Collections.Generic;

public class ClientFSMContext { 
    public KcpClient kcpClient;
    public List<NetworkMessage> networkMessage;
    public Dictionary<int,int> PlayerID_CharactorIDMap;

    public ClientFSMContext(KcpClient kcpClient, List<NetworkMessage> networkMessage) {
        this.kcpClient = kcpClient;
        this.networkMessage = networkMessage;
        PlayerID_CharactorIDMap = new();
    }
}