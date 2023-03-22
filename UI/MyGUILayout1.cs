using UnityEngine;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using brickgame;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static partial class MyGUILayout
{
#if UNITY_EDITOR
    public static void PropertyField_Table(SerializedProperty prop,string[] subfields)
    {
        GUILayout.BeginHorizontal();
        //GUILayout.Label(prop.name);
        int n = prop.arraySize;
        GUILayout.Label(prop.name);
        var ret = GUILayout.TextField(""+n);
        int nn;
        if (Int32.TryParse(ret, out nn))
        {
            prop.arraySize = nn;
        }
        GUILayout.EndHorizontal();

        var old = GUI.color;
        GUI.color = Color.green;
        // header
        GUILayout.BeginHorizontal();
        foreach (var col in subfields)
        {
            GUILayout.Label(col);
        }
        GUILayout.EndHorizontal();
        GUI.color = old;
     

        // --
        for (int i = 0; i < prop.arraySize; i++)
        {
          
            var ele =  prop.GetArrayElementAtIndex(i);

            GUILayout.BeginHorizontal();

            foreach (var col in subfields)
            {
                var p = ele.FindPropertyRelative(col);
                EditorGUILayout.PropertyField(p,new GUIContent(""));
               //GUILayout.Label(col);

            }
            GUILayout.EndHorizontal();

        }
    }
#endif

}