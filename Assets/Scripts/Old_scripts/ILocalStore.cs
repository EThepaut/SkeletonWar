using System;

public interface ILocalStore
{
    void SaveLocal(LocalPlayerData data);
    LocalPlayerData LoadLocal();
}
