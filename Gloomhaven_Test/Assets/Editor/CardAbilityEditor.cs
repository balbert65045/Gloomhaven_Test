using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombatCardAbility))]
public class CombatCardAbilityEditor : Editor
{
    public SerializedProperty
        Lost_Property,
        AbilityType_Property,
        Actions_Property,
        ActionsCount_Property;

    CombatCardAbility cardAbility;

    private void OnEnable()
    {
        Lost_Property = serializedObject.FindProperty("LostAbility");
        Actions_Property = serializedObject.FindProperty("Actions");
        ActionsCount_Property = serializedObject.FindProperty("Actions.Array.size");
        cardAbility = (CombatCardAbility)target;

    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(Lost_Property);
        //EditorGUILayout.PropertyField(Actions_Property, new GUIContent("Actions Field"), false);
        EditorGUILayout.BeginHorizontal();
        int OldArrayNumber = ActionsCount_Property.intValue;
        EditorGUILayout.LabelField("Number of Actions");
        int ArrayNumber = EditorGUILayout.IntField(OldArrayNumber);
        if (ArrayNumber != OldArrayNumber)
            ActionsCount_Property.intValue = ArrayNumber;
        EditorGUILayout.EndHorizontal();

        //ActionType actionType = (ActionType)Actions_Property.enumValueIndex + 1;
        for (int i = 1; i <= Actions_Property.arraySize; i++)
        {
            SerializedProperty ActionType_Property = Actions_Property.GetArrayElementAtIndex(i-1).FindPropertyRelative("thisActionType");
            EditorGUILayout.PropertyField(ActionType_Property, new GUIContent("Action " + i));
            ActionType actionType = (ActionType)ActionType_Property.enumValueIndex + 1;
            switch (actionType)
            {
                case ActionType.Attack:
                    SerializedProperty ActionAOE_Property = Actions_Property.GetArrayElementAtIndex(i - 1).FindPropertyRelative("thisAOE");
                    SerializedProperty ActionAOEType_Property = ActionAOE_Property.FindPropertyRelative("thisAOEType");
                    EditorGUILayout.PropertyField(ActionAOEType_Property);

                    SerializedProperty ActionTargets_Property = ActionAOE_Property.FindPropertyRelative("Targets");
                    EditorGUILayout.PropertyField(ActionTargets_Property);

                    SerializedProperty ActionAttackRange_Property = Actions_Property.GetArrayElementAtIndex(i - 1).FindPropertyRelative("Range");
                    EditorGUILayout.PropertyField(ActionAttackRange_Property);

                    SerializedProperty ActionAttackAmount_Property = ActionAOE_Property.FindPropertyRelative("Damage");
                    EditorGUILayout.PropertyField(ActionAttackAmount_Property);
                    break;
                case ActionType.Movement:
                    SerializedProperty ActionMovementAmount_Property = Actions_Property.GetArrayElementAtIndex(i - 1).FindPropertyRelative("Range");
                    EditorGUILayout.PropertyField(ActionMovementAmount_Property);
                    break;
                case ActionType.Heal:

                    SerializedProperty ActionHealAOE_Property = Actions_Property.GetArrayElementAtIndex(i - 1).FindPropertyRelative("thisAOE");
                    SerializedProperty ActionHealAOEType_Property = ActionHealAOE_Property.FindPropertyRelative("thisAOEType");
                    EditorGUILayout.PropertyField(ActionHealAOEType_Property);

                    SerializedProperty ActionHealRange_Property = Actions_Property.GetArrayElementAtIndex(i - 1).FindPropertyRelative("Range");
                    EditorGUILayout.PropertyField(ActionHealRange_Property);

                    SerializedProperty ActionHealAmount_Property = ActionHealAOE_Property.FindPropertyRelative("Damage");
                    EditorGUILayout.PropertyField(ActionHealAmount_Property);
                    break;
                case ActionType.Shield:

                    SerializedProperty ActionShieldAOE_Property = Actions_Property.GetArrayElementAtIndex(i - 1).FindPropertyRelative("thisAOE");
                    SerializedProperty ActionShieldAOEType_Property = ActionShieldAOE_Property.FindPropertyRelative("thisAOEType");
                    EditorGUILayout.PropertyField(ActionShieldAOEType_Property);

                    SerializedProperty ActionShieldRange_Property = Actions_Property.GetArrayElementAtIndex(i - 1).FindPropertyRelative("Range");
                    EditorGUILayout.PropertyField(ActionShieldRange_Property);

                    SerializedProperty ActionShieldAmount_Property = ActionShieldAOE_Property.FindPropertyRelative("Damage");
                    EditorGUILayout.PropertyField(ActionShieldAmount_Property);
                    break;
            }
        }



        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();

    }
}
