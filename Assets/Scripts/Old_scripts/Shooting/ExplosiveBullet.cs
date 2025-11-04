using UnityEngine;
using System.Collections.Generic;

public class ExplosiveBullet : MonoBehaviour
{
    private float explosionRadius;
    public float fireDuration;
    public float fireTickInterval;
    private float fireTickDamage;
    public GameObject explosionEffectPrefab;
    public LayerMask enemyLayer;
    public WeaponData weaponData;

    private void Start()
    {
        explosionRadius = weaponData.zone;
        fireTickDamage = weaponData.damage;
    }

    void OnCollisionEnter(Collision collision)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int overlayLayer = LayerMask.NameToLayer("Overlay");

        if (collision.gameObject.layer == playerLayer || collision.gameObject.layer == overlayLayer)
            return;
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);
        HashSet<HealthComponent> alreadyHit = new HashSet<HealthComponent>();

        foreach (var hit in hits)
        {
            var health = hit.GetComponentInParent<HealthComponent>();
            if (health != null && !alreadyHit.Contains(health))
            {
                alreadyHit.Add(health);
                health.ApplyFireEffect(fireDuration, fireTickInterval, fireTickDamage);
            }
        }

        Destroy(gameObject);
    }
}