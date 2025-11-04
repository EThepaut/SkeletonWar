using UnityEngine;
using System.Collections;
public class TestingSkeletonBehaviour : MonoBehaviour
{
    private Animator mAnimator;
    public Transform player; 

    private enum TestState { Idle, Rotate, MoveToPlayer }
    private TestState currentState = TestState.Idle;

    public float rotateSpeed = 90f; 
    public float moveSpeed = 2f;

    public SkinnedMeshRenderer[] meshRenderers; 

    private Color[][] originalColors;

    private Rigidbody rb;

    void Start()
    {
        mAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            originalColors = new Color[meshRenderers.Length][];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var mats = meshRenderers[i].materials;
                originalColors[i] = new Color[mats.Length];
                for (int j = 0; j < mats.Length; j++)
                    originalColors[i][j] = mats[j].color;
            }
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case TestState.Rotate:
                if (mAnimator != null)
                    mAnimator.SetInteger("IntSpeed", 6);
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
                break;
            case TestState.MoveToPlayer:
                if (mAnimator != null)
                    mAnimator.SetInteger("IntSpeed", 3);
                if (player != null && rb != null)
                {
                    Vector3 direction = (player.position - transform.position).normalized;
                    rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
                    if (direction != Vector3.zero)
                    {
                        Quaternion lookRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                    }
                }
                break;
            case TestState.Idle:
                if (mAnimator != null)
                    mAnimator.SetInteger("IntSpeed", 0);
                if (rb != null)
                    rb.linearVelocity = Vector3.zero;
                break;
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
    public void RotateTest()
    {
        currentState = TestState.Rotate;
    }

    public void MoveToPlayerTest()
    {
        currentState = TestState.MoveToPlayer;
    }

    public void IdleTest()
    {
        currentState = TestState.Idle;
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
}