using Drive;
using kcp2k;
using System.Collections.Generic;

public class ClientFSMContext { 
    public KcpServer kcpServer;
    public Dictionary<int, int> ConnectionID_PlayerIDMap;
    public Dictionary<int, int> PlayerID_ConnectionMap;
    public int PlayerCount;
    public List<NetworkMessage> networkMessage;

    public ClientFSMContext(KcpServer kcpServer, Dictionary<int, int> connectionID_PLayerIDMap, Dictionary<int,int> playerID_ConnectionIDMap, int playerCount, List<NetworkMessage> networkMessage) {
        this.kcpServer = kcpServer;
        ConnectionID_PlayerIDMap = connectionID_PLayerIDMap;
        PlayerID_ConnectionMap = playerID_ConnectionIDMap;
        PlayerCount = playerCount;
        this.networkMessage = networkMessage;
    }
}