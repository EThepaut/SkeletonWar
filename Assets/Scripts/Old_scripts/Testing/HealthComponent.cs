using System.Collections;
using UnityEngine;
using static FloatingDamageText;

public class HealthComponent : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth = 100;

    public bool isEnemy = true;

    public SkinnedMeshRenderer[] meshRenderers;

    public GameObject floatingDamageTextPrefab;

    public GameObject healthBarPrefab; 
    private HealthBar healthBarInstance;

    public GameObject fireParticlePrefab;
    private GameObject activeFireParticle;

    public GameObject onSwitchPrefab;
    private GameObject onSwitchPrefabInstance;
   


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarPrefab != null)
        {
            var canvas = Instantiate(healthBarPrefab);
            healthBarInstance = canvas.GetComponent<HealthBar>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0)
        {
            if (isEnemy)
            {
                isEnemy = false;
                ChangeTeamFeedBack();
                currentHealth = maxHealth;
            }
            else
            {
                isEnemy = true;
                ChangeTeamFeedBack();
                currentHealth = maxHealth;
            }
        }
        if (healthBarInstance != null)
        {
            Vector3 worldPos = transform.position + Vector3.up * 2f; 
            healthBarInstance.SetWorldPosition(worldPos);

            if (Camera.main != null)
                healthBarInstance.transform.rotation = Quaternion.LookRotation(healthBarInstance.transform.position - Camera.main.transform.position);

            healthBarInstance.SetHealth(currentHealth, maxHealth);
        }
    }
    public void TakeDamage(float amount, Vector3? hitPosition = null)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (floatingDamageTextPrefab != null && hitPosition.HasValue)
        {
            FloatingDamageTextSpawner.Spawn(floatingDamageTextPrefab, hitPosition.Value, amount);
        }

        if (currentHealth <= 0)
        {
            if(healthBarInstance != null)
            {
                Destroy(healthBarInstance.gameObject);
                healthBarInstance = null;
            }
            if(onSwitchPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 1.75f;
                onSwitchPrefabInstance = Instantiate(onSwitchPrefab, spawnPos, Quaternion.identity);
                Destroy(onSwitchPrefabInstance, 1f);
            }

            bool newTeam = !isEnemy;
            GetComponent<SkeletonsBehaviour>()?.ChangeTeamAndRespawn(newTeam);
        }
    }

    private IEnumerator FireEffectCoroutine(float duration, float tickInterval, float tickDamage)
    {
        if(fireParticlePrefab != null && activeFireParticle == null)
        {
            activeFireParticle = Instantiate(fireParticlePrefab, transform);
            activeFireParticle.transform.localPosition = Vector3.zero;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            TakeDamage(tickDamage);
            elapsed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }

        if(activeFireParticle != null)
        {
            Destroy(activeFireParticle);
            activeFireParticle = null;
        }
    }
    public void ApplyFireEffect(float duration, float tickInterval, float tickDamage)
    {
        StartCoroutine(FireEffectCoroutine(duration, tickInterval, tickDamage));
    }
    private void ChangeTeamFeedBack()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            var mats = meshRenderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                mats[j].color = Color.blue;
            }
        }
        var skeleton = GetComponent<TestingSkeletonBehaviour>();
        if (skeleton != null)
            skeleton.SetOriginalColorToCurrent();
    }
}