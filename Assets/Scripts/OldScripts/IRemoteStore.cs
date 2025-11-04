using System;

public interface IRemoteStore 
{
    void SaveRemote(string playerId, RemotePlayerData data);
    void LoadRemote(string playerId, Action<RemotePlayerData> onResult, Action<Exception> onError);
}
