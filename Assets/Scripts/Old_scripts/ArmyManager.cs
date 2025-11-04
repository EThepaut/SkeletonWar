using UnityEngine;

public class ArmyManager : MonoBehaviour
{
    [Header("Références")]
    public GameObject enemySkeletonPrefab;
    public GameObject allySkeletonPrefab;
    public GameObject skeletonPrefab;
    public Transform spawnPoint;
    public Transform enemyBase;

    [Header("Paramètres de l'armée")]
    public int armySize = 50;
    private int currentArmySize = 0;
    public float spawnRadius = 30f;

    void Start()
    {
        SpawnArmy();
    }

    public void SpawnArmy()
    {
        if (skeletonPrefab == null || spawnPoint == null || enemyBase == null)
        {
            Debug.LogError("Certaines références ne sont pas assignées dans ArmyManager!");
            return;
        }

        GameObject spawnManager = new GameObject("SpawnManager");
        SkeletonsBehaviour spawner = spawnManager.AddComponent<SkeletonsBehaviour>();

        spawner.skeletonPrefab = skeletonPrefab;
        spawner.spawnPoint = spawnPoint;
        spawner.enemyBase = enemyBase;
        spawner.spawnRadius = spawnRadius;
        while (currentArmySize < armySize)
            if (spawner.SpawnArmyInNavMesh() == true)
            {
                currentArmySize++;
            }
    }

    public GameObject GetPrefabByTeam(bool isEnemy)
    {
        return isEnemy ? enemySkeletonPrefab : allySkeletonPrefab;
    }
}