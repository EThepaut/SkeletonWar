using UnityEngine;

public class BaiscBullet : MonoBehaviour
{
    public GameObject hitParticlePrefab;
    public float hitParticleLifetime = 2f;
    public string ignoreTag = "Testing";
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet a touché : " + collision.gameObject.name);

        if (hitParticlePrefab != null && !collision.gameObject.CompareTag(ignoreTag))
        {
            ContactPoint contact = collision.contacts[0];
            GameObject hitParticle = Instantiate(hitParticlePrefab, contact.point, Quaternion.identity);
            Destroy(hitParticle, hitParticleLifetime);
        }

        var skeleton = collision.gameObject.GetComponentInParent<SkeletonsBehaviour>();
        if (skeleton != null)
        {
            skeleton.ShowHitFeedback();

            var health = skeleton.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.TakeDamage(25f, skeleton.transform.position + Vector3.up * 2f);
                Debug.Log("current skeleton health = " + health.currentHealth);
            }
        }

        if (collision.gameObject.CompareTag(ignoreTag))
            return;

        Destroy(gameObject);
    }
}