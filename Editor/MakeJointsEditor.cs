#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FizzSDK
{
    [CustomEditor(typeof(JointCreator))]
    public class JointCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var myScript = (JointCreator)target;

            if (!GUILayout.Button("Create Joints!")) return;

            var dialogAccepted = EditorUtility.DisplayDialog(
                "Create Joints",
                "This operation is irreversible - you may want to make a copy of your objects. Continue?",
                "Yes",
                "No"
            );

            if (dialogAccepted)
            {
                myScript.MakeJoints();
            }
        }
    }
}
#endif