using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;

    [Header("Bullet settings ")]
    public GameObject bulletObject;
    public float lifeTime = 1f;

    public Transform weaponTip;
    public string playerLayerName = "Player";

    private float lastFireTime;

    public AudioSource audioData;

    public void Fire()
    {
        if (weaponData == null || !CanFire()) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        int layerMask = ~(1 << playerLayer);

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000);
        }

        Vector3 spawnPosition = weaponTip.transform.position;
        GameObject bullet = Instantiate(bulletObject, spawnPosition, Quaternion.identity);

        Vector3 direction = (targetPoint - spawnPosition).normalized;

        audioData.Play();

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * weaponData.bulletSpeed;
        }

        Destroy(bullet, lifeTime);

        lastFireTime = Time.time;
    }

    private bool CanFire()
    {
        float timeBetweenShots = 1f / weaponData.fireRate;
        return Time.time >= lastFireTime + timeBetweenShots;
    }

    public WeaponData GetData()
    { 
        return weaponData;
    }
}