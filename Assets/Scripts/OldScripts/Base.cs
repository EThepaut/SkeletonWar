using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Base : MonoBehaviour
{
    private float currentHP = 0;
    private BoxCollider baseCollider;
    private HealthComponent health;
    private int SkeletonInBase;

    private Dictionary<Collider, float> skeletonLastAttackTime = new Dictionary<Collider, float>();
    private float attackCooldown = 3f;

    void Start()
    {
        health = GetComponentInChildren<HealthComponent>();
        if (health == null)
        {
            Debug.LogError("HealthComponent non trouvé!");
            return;
        }

        currentHP = health.maxHealth;
        SkeletonInBase = 0;
        baseCollider = GetComponentInChildren<BoxCollider>();

        if (baseCollider != null)
        {
            if (!baseCollider.isTrigger)
            {
                Debug.LogWarning("Le collider de la base n'est pas configuré comme trigger! Correction automatique...");
                baseCollider.isTrigger = true;
            }
        }
        else
        {
            Debug.LogError("Aucun BoxCollider trouvé!");
        }

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger enter with: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Skeleton"))
        {
            SkeletonInBase++;

            if (!skeletonLastAttackTime.ContainsKey(other))
            {
                skeletonLastAttackTime[other] = 0f;
            }
            Debug.Log("A skeleton is attacking your base !");
            Debug.Log($"Skeletons in base: {SkeletonInBase}");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Skeleton"))
        {
            if (CanSkeletonAttack(other))
            {
                ApplySkeletonDamage(other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Trigger exit with: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Skeleton"))
        {
            SkeletonInBase--;

            if (skeletonLastAttackTime.ContainsKey(other))
            {
                skeletonLastAttackTime.Remove(other);
            }
            Debug.Log($"Skeletons in base: {SkeletonInBase}");
        }
    }

    void Update()
    {
        if (currentHP <= 0)
        {
            Debug.Log("Base destroyed!");
            Destroy(gameObject);
        }
    }

    private bool CanSkeletonAttack(Collider skeleton)
    {
        if (!skeletonLastAttackTime.ContainsKey(skeleton))
        {
            skeletonLastAttackTime[skeleton] = 0f;
            return true;
        }

        float timeSinceLastAttack = Time.time - skeletonLastAttackTime[skeleton];
        return timeSinceLastAttack >= attackCooldown;
    }

    private void ApplySkeletonDamage(Collider skeleton)
    {
        currentHP -= 3;
        if (health != null)
        {
            health.TakeDamage(3);
        }

        skeletonLastAttackTime[skeleton] = Time.time;

        Debug.Log($"Skeleton {skeleton.name} attacked base! Base HP: {currentHP}");
    }
}