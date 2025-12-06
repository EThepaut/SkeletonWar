using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerClass", menuName = "Scriptable Objects/ClassData")]
public class PlayerClassData : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("JumpSettings")]
    public int maxJumps = 2;
    public bool canSlowFall = false;
    public float slowFallGravityScale = 0.3f;

    [Header("Physics")]
    public float normalGravityScale = 1f;
}
