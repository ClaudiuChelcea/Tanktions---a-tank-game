using Unity.Collections;
using Unity.Netcode;

public struct NetworkString : INetworkSerializable
{
    private FixedString32Bytes info;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
    }

    public override string ToString()
    {
        return info.ToString();
    }

    public static implicit operator string(NetworkString str)
    {
        return str.ToString();
    }

    public static implicit operator NetworkString(string str)
    {
        return new NetworkString { info = str };
    }
}