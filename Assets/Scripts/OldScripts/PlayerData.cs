using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string login;
    public string pseudo;
    public int weaponIndex;
    public int hpMax;
    public int hp;
    public int damage;
    public int defense;
    public int speed;
    public int stage;
    public int level;
    public int xp;
    public bool volumeSound;
}

[System.Serializable]
public class LocalPlayerData
{
    public string playerId;
    public int weaponIndex;
    public bool volumeSound;
}

[System.Serializable]
public class RemotePlayerData
{
    public string login;
    public string pseudo;
    public int hpMax;
    public int hp;
    public int damage;
    public int defense;
    public int speed;
    public int stage;
    public int level;
    public int xp;
}
