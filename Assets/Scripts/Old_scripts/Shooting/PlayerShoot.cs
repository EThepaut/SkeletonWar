using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("Bullet settings ")]
    public GameObject bulletObject;
    public float bulletSpeed = 10f;
    public float lifeTime = 1f;

    public GameObject weaponTip;

    public string playerLayerName = "Player";

    void Shoot()
    {
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

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
        Destroy(bullet, lifeTime);

        /*
        // Crosshair système enlever les comms pour remettre


        // Vector3 spawnPosition = Camera.main.transform.position;
        // GameObject bullet = Instantiate(bulletObject, spawnPosition, Quaternion.identity);
        // Vector3 direction = (targetPoint - spawnPosition).normalized;
        // if (rb != null)
        // {
        //     rb.linearVelocity = direction * bulletSpeed;
        // }
        // Destroy(bullet, lifeTime);
        */
    }
}