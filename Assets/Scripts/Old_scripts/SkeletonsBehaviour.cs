using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SkeletonsBehaviour : MonoBehaviour
{
    private Animator mAnimator;
    private NavMeshAgent navMeshAgent;
    private int _maxHP = 2;
    private int _currentHP;
    private float timer;

    public SkinnedMeshRenderer[] meshRenderers;

    private Color[][] originalColors;

    [Header("Spawn Settings")]
    public GameObject skeletonPrefab;
    public Transform spawnPoint;
    public Transform enemyBase;
    public float spawnRadius = 50f;
    public float waterDistance = 1f;
    public LayerMask waterLayerMask;

    void Start()
    {
        mAnimator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        _currentHP = _maxHP;

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            originalColors = new Color[meshRenderers.Length][];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var mats = meshRenderers[i].materials;
                originalColors[i] = new Color[mats.Length];
            }
            SetOriginalColorToCurrent();
        }
    }

    void Update()
    {
        UpdateAnimation();

        if (enemyBase != null && navMeshAgent != null && !navMeshAgent.hasPath)
        {
            GoToEnemyBase(enemyBase.position);
        }
    }

    void UpdateAnimation()
    {
        if (mAnimator != null && navMeshAgent != null)
        {
            if (navMeshAgent.hasPath == false)
            {
                mAnimator.SetInteger("IntSpeed", 0);
            }
            else if (navMeshAgent.remainingDistance < 35)
            {
                mAnimator.SetInteger("IntSpeed", 3);
            }
            else if (navMeshAgent.remainingDistance > 35)
            {
                mAnimator.SetInteger("IntSpeed", 6);
            }
        }
    }

    public bool SpawnArmyInNavMesh()
    {
        if (skeletonPrefab == null)
        {
            Debug.LogError("skeletonPrefab n'est pas assign� dans l'inspecteur!");
            return false;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("spawnPoint n'est pas assign� dans l'inspecteur!");
            return false;
        }

        if (enemyBase == null)
        {
            Debug.LogError("enemyBase n'est pas assign� dans l'inspecteur!");
            return false;
        }
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;

        Vector3 spawnPosition = spawnPoint.position + randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPosition, out hit, spawnRadius, NavMesh.AllAreas))
        {
            if (hit.position.y <= 70 || hit.position.y >= 80.5)
                return false;

            GameObject skeleton = Instantiate(skeletonPrefab, hit.position, Quaternion.identity);

            SkeletonsBehaviour skeletonBehaviour = skeleton.GetComponent<SkeletonsBehaviour>();
            if (skeletonBehaviour != null)
            {
                skeletonBehaviour.enemyBase = this.enemyBase;
            }

            NavMeshAgent skeletonAgent = skeleton.GetComponent<NavMeshAgent>();
            if (skeletonAgent != null)
            {
                skeletonAgent.SetDestination(enemyBase.position);
            }
        }
        else
        {
            Debug.LogWarning("Position de spawn invalide pour le squelette");
            return false;
        }
        return true;
    }

    void GoToEnemyBase(Vector3 enemyBasePosition)
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.SetDestination(enemyBasePosition);
        }
    }

    public void TakeDamage(int damage)
    {
        _currentHP -= damage;
        if (_currentHP <= 0)
        {
            mAnimator.SetTrigger("TrDie");
            Die();
        }
    }

    public void SetOriginalColorToCurrent()
    {
        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var mats = meshRenderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    originalColors[i][j] = mats[j].color;
                }
            }
        }
    }

    private IEnumerator HitFeedbackCoroutine(float duration)
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            var mats = meshRenderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
                mats[j].color = Color.red;
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            var mats = meshRenderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
                mats[j].color = originalColors[i][j];
        }
    }
    //gere la duration ici
    public void ShowHitFeedback(float duration = 0.15f)
    {
        StartCoroutine(HitFeedbackCoroutine(duration));
    }
    void Die()
    {
        if (mAnimator != null)
        {
            mAnimator.SetTrigger("Die");
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }

        Destroy(gameObject, 2f);
    }

    public void ChangeTeamAndRespawn(bool toEnemy)
    {
        ArmyManager armyManager = FindFirstObjectByType<ArmyManager>();
        if (armyManager == null) return;

        GameObject newPrefab = armyManager.GetPrefabByTeam(toEnemy);

        GameObject newSkeleton = Instantiate(newPrefab, transform.position, Quaternion.identity);

        SkeletonsBehaviour newBehaviour = newSkeleton.GetComponent<SkeletonsBehaviour>();
        if (newBehaviour != null)
        {
            newBehaviour.enemyBase = armyManager.enemyBase;
        }
        Destroy(gameObject);
    }
}