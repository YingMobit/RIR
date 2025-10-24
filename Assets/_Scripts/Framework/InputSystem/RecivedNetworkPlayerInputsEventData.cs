using Drive;

public interface IRecivedNetworkPlayerInputsEventData : IEventData {
    public NetworkPlayerInputsDownLinkMessage NetworkPlayerInputsDownLinkMessage { get; }
}

public struct RecivedNetworkPlayerInputsEventData : IRecivedNetworkPlayerInputsEventData {
    private NetworkPlayerInputsDownLinkMessage networkPlayerInputsDownLinkMessage;
    public NetworkPlayerInputsDownLinkMessage NetworkPlayerInputsDownLinkMessage => networkPlayerInputsDownLinkMessage;

    public RecivedNetworkPlayerInputsEventData(NetworkPlayerInputsDownLinkMessage message) {
        networkPlayerInputsDownLinkMessage = message;
    }
}