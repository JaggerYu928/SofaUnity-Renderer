﻿using UnityEngine;
using UnityEditor;
using SofaUnity;
using System.Collections.Generic;


/// <summary>
/// Editor Class to define the creation and UI of SDeformableMesh GameObject
/// </summary>
[CustomEditor(typeof(SComponentObject), true)]
public class SComponentEditor : Editor
{
    /// <summary>
    /// Method to set the UI of the SDeformableMesh GameObject
    /// </summary>
    public override void OnInspectorGUI()
    {
        SComponentObject _object = (SComponentObject)this.target;

        List<SData> datas = _object.datas;

        foreach (SData entry in datas)
        {
            if (entry.getType() == "string")
            {
                EditorGUILayout.TextField(entry.nameID, _object.impl.getStringValue(entry.nameID));
            }
            else if (entry.getType() == "bool")
            {
                EditorGUILayout.Toggle(entry.nameID, _object.impl.getBoolValue(entry.nameID));
            }
            else if (entry.getType() == "Vec3d" || entry.getType() == "Vec3f")
            {
                EditorGUILayout.Vector3Field(entry.nameID, _object.impl.getVector3fValue(entry.nameID));
            }
            else if (entry.getType() == "vector < float >" || entry.getType() == "vector<float>")
            {
                entry.dataSize = _object.impl.getVecfSize(entry.nameID);
                EditorGUILayout.TextField(entry.nameID, "vector<float> size " + entry.dataSize);
            }
            else if (entry.getType() == "vector < int >" || entry.getType() == "vector<int>")
            {
                entry.dataSize = _object.impl.getVecfSize(entry.nameID);
                EditorGUILayout.TextField(entry.nameID, "vector<int> size " + entry.dataSize);
            }
            else if (entry.getType() == "float")
            {
                EditorGUILayout.FloatField(entry.nameID, _object.impl.getFloatValue(entry.nameID));
            }
            else if (entry.getType() == "int")
            {
                EditorGUILayout.FloatField(entry.nameID, _object.impl.getIntValue(entry.nameID));
            }
            else
                EditorGUILayout.TextField(entry.nameID, "Unsopported type: "+ entry.getType());
        }

        //Dictionary<string, string> datas = _object.dataMap;
        //if (datas == null)
        //    return;

        //foreach (KeyValuePair<string, string> entry in datas)
        //{
        //    Debug.Log("DicT: " + entry.Key + " " + entry.Value);
        //    if (entry.Value == "string")
        //    {
        //        EditorGUILayout.TextField(entry.Key); 
        //    }
        //    else if (entry.Value == "bool")
        //    {
        //        EditorGUILayout.Toggle(entry.Key, true);
        //    }
        //}
    }
}
