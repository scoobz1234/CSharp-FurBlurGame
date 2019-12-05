using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SpriteTrail))]
[CanEditMultipleObjects]
public class SpriteTrailEditor : Editor
{
    SerializedProperty m_CurrentTrailPreset;
    SerializedProperty m_TrailParent;
    SerializedProperty m_SpriteToDuplicate;
    SerializedProperty m_LayerName;
    SerializedProperty m_ZMoveStep;
    SerializedProperty m_ZMoveMax;
    SerializedProperty m_TrailOrderInLayer;
    SerializedProperty m_HideTrailOnDisabled;
    SerializedProperty m_TrailActivationCondition;
    SerializedProperty m_TrailDisactivationCondition;
    SerializedProperty m_StartIfUnderVelocity;
    SerializedProperty m_VelocityStartIsLocalSpace;
    SerializedProperty m_VelocityNeededToStart;
    SerializedProperty m_StopIfOverVelocity;
    SerializedProperty m_VelocityStopIsLocalSpace;
    SerializedProperty m_VelocityNeededToStop;
    SerializedProperty m_TrailActivationDuration;
    SerializedProperty m_TrailName;



    void OnEnable()
    {
        m_CurrentTrailPreset            = serializedObject.FindProperty("m_CurrentTrailPreset");
        m_TrailParent                   = serializedObject.FindProperty("m_TrailParent");
        m_SpriteToDuplicate             = serializedObject.FindProperty("m_SpriteToDuplicate");
        m_LayerName                     = serializedObject.FindProperty("m_LayerName");
        m_ZMoveStep                     = serializedObject.FindProperty("m_ZMoveStep");
        m_ZMoveMax                      = serializedObject.FindProperty("m_ZMoveMax");
        m_TrailOrderInLayer             = serializedObject.FindProperty("m_TrailOrderInLayer");
        m_HideTrailOnDisabled           = serializedObject.FindProperty("m_HideTrailOnDisabled");
        m_TrailActivationCondition      = serializedObject.FindProperty("m_TrailActivationCondition");
        m_TrailDisactivationCondition   = serializedObject.FindProperty("m_TrailDisactivationCondition");
        m_StartIfUnderVelocity          = serializedObject.FindProperty("m_StartIfUnderVelocity");
        m_VelocityStartIsLocalSpace     = serializedObject.FindProperty("m_VelocityStartIsLocalSpace");
        m_VelocityNeededToStart         = serializedObject.FindProperty("m_VelocityNeededToStart");
        m_StopIfOverVelocity            = serializedObject.FindProperty("m_StopIfOverVelocity");
        m_VelocityStopIsLocalSpace      = serializedObject.FindProperty("m_VelocityStopIsLocalSpace");
        m_VelocityNeededToStop          = serializedObject.FindProperty("m_VelocityNeededToStop");
        m_TrailActivationDuration       = serializedObject.FindProperty("m_TrailActivationDuration");
        m_TrailName                     = serializedObject.FindProperty("m_TrailName");
     }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SpriteTrail TrailSettingsScript = target as SpriteTrail;

        EditorGUILayout.PropertyField(m_TrailName);
        GUILayout.Space(15);

        EditorGUILayout.PropertyField(m_CurrentTrailPreset);
        if (TrailSettingsScript.m_CurrentTrailPreset == null)
        {
            EditorGUILayout.HelpBox("You need to assign a preset (Current trail preset).\n You can create one, or use one of the preset In the folder : \nSpriteTrail/PREFAB/TRAIL_PRESETS", MessageType.Warning, true);
            GUILayout.Space(15);
        }

        EditorGUILayout.PropertyField(m_HideTrailOnDisabled);
        //GUILayout.Space(15);
        EditorGUILayout.PropertyField(m_TrailActivationCondition);
        switch (TrailSettingsScript.m_TrailActivationCondition)
        {
            case TrailActivationCondition.AlwaysEnabled:
                break;
            case TrailActivationCondition.Manual:
                break;
            case TrailActivationCondition.VelocityMagnitude:
                EditorGUILayout.PropertyField(m_VelocityNeededToStart);
                EditorGUILayout.PropertyField(m_StartIfUnderVelocity);
                EditorGUILayout.PropertyField(m_VelocityStartIsLocalSpace);
                GUILayout.Space(15);
                break;
        }

        
        EditorGUILayout.PropertyField(m_TrailDisactivationCondition);
        switch (TrailSettingsScript.m_TrailDisactivationCondition)
        {
            case TrailDisactivationCondition.Manual:
                break;
            case TrailDisactivationCondition.Time:
                EditorGUILayout.PropertyField(m_TrailActivationDuration);
                GUILayout.Space(15);
                break;
            case TrailDisactivationCondition.VelocityMagnitude:
                EditorGUILayout.PropertyField(m_VelocityNeededToStop);
                EditorGUILayout.PropertyField(m_StopIfOverVelocity);
                EditorGUILayout.PropertyField(m_VelocityStopIsLocalSpace);
                GUILayout.Space(15);
                break;
        }

        


        EditorGUILayout.PropertyField(m_TrailParent);
        EditorGUILayout.PropertyField(m_SpriteToDuplicate);
        EditorGUILayout.PropertyField(m_LayerName);
        EditorGUILayout.PropertyField(m_ZMoveStep);
        EditorGUILayout.PropertyField(m_ZMoveMax);
        EditorGUILayout.PropertyField(m_TrailOrderInLayer);

        serializedObject.ApplyModifiedProperties();
    }
}