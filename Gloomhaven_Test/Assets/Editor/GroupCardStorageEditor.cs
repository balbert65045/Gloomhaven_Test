using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GroupCardStorage))]
public class GroupCardStorageEditor : Editor
{

    public SerializedProperty
        MyGroupCardStorage_Property,
        MyGroupCardStorageCount_Property;

    GroupCardStorage groupCardStorage;

    private void OnEnable()
    {
        MyGroupCardStorage_Property = serializedObject.FindProperty("MyGroupCardStorage");
        MyGroupCardStorageCount_Property = serializedObject.FindProperty("MyGroupCardStorage.Array.size");
        groupCardStorage = (GroupCardStorage)target;

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //serializedObject.Update();
        //EditorGUI.BeginChangeCheck();

        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Number of Characters");
        //int OldArrayNumber = MyGroupCardStorageCount_Property.intValue;
        //int ArrayNumber = EditorGUILayout.IntField(OldArrayNumber);
        //EditorGUILayout.EndHorizontal();

        //if (EditorGUI.EndChangeCheck())
        //    serializedObject.ApplyModifiedProperties();
        //serializedObject.ApplyModifiedProperties();
    }
}
