#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace FizzSDK
{
    // i hate doing this, but editor scripts don't work with interfaces and idk what else to do D:
    [CustomEditor(typeof(MakeJointsBreakable))]
    public class MakeJointsBreakableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var myScript = (MakeJointsBreakable)target;

            if (GUILayout.Button("Add Components!"))
            {
                var dialogAccepted = EditorUtility.DisplayDialog(
                    "Add Components",
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
}
#endif