#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerCubeController), true)]
public class PlayerCubeControllerEditor : Editor
{
    private SerializedProperty m_Speed;
    private SerializedProperty m_ApplyVerticalInputToZAxis;
    private SerializedProperty m_MouseSensitivity;
    private SerializedProperty m_PlayerCamera;
    private SerializedProperty m_Gravity;
    private SerializedProperty m_JumpHeight;
    private SerializedProperty m_GroundCheck;
    private SerializedProperty m_GroundDistance;
    private SerializedProperty m_GroundLayerMask;
    private SerializedProperty m_Cube;

    private void OnEnable()
    {
        m_Speed = serializedObject.FindProperty(nameof(PlayerCubeController.Speed));
        m_ApplyVerticalInputToZAxis = serializedObject.FindProperty(nameof(PlayerCubeController.ApplyVerticalInputToZAxis));
        m_MouseSensitivity = serializedObject.FindProperty(nameof(PlayerCubeController.mouseSensitivity));
        m_PlayerCamera = serializedObject.FindProperty(nameof(PlayerCubeController.playerCamera));
        m_Gravity = serializedObject.FindProperty(nameof(PlayerCubeController.gravity));
        m_JumpHeight = serializedObject.FindProperty(nameof(PlayerCubeController.jumpHeight));
        m_GroundCheck = serializedObject.FindProperty(nameof(PlayerCubeController.groundCheck));
        m_GroundDistance = serializedObject.FindProperty(nameof(PlayerCubeController.groundDistance));
        m_GroundLayerMask = serializedObject.FindProperty(nameof(PlayerCubeController.groundLayerMask));
        m_Cube = serializedObject.FindProperty(nameof(PlayerCubeController.cube));
    }

    public override void OnInspectorGUI()
    {
        var playerCubeController = target as PlayerCubeController;

        serializedObject.Update();

        EditorGUILayout.LabelField("Player Cube Controller", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Display custom properties
        DisplayPlayerCubeControllerProperties();

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Network Transform Settings", EditorStyles.boldLabel);

        // Display the rest of inspector (NetworkTransform properties)
        DrawDefaultInspector();
    }

    private void DisplayPlayerCubeControllerProperties()
    {
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_Speed);
        EditorGUILayout.PropertyField(m_ApplyVerticalInputToZAxis);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_MouseSensitivity);
        EditorGUILayout.PropertyField(m_PlayerCamera);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Jump & Gravity", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_Gravity);
        EditorGUILayout.PropertyField(m_JumpHeight);
        EditorGUILayout.PropertyField(m_GroundCheck);
        EditorGUILayout.PropertyField(m_GroundDistance);
        EditorGUILayout.PropertyField(m_GroundLayerMask);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_Cube);
    }
}
#endif