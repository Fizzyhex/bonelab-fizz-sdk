#if UNITY_EDITOR
using SLZ.Marrow;
using UnityEditor;
using UnityEngine;
using SLZ.VFX;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(MakeJointsBreakable))]
    public class MakeJointsBreakableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                
                "This script adds Object Destructible and destruction logic to all Rigidbodies.",
                MessageType.Info
            );
            
            var myScript = (MakeJointsBreakable)target;
            
            if (myScript.objectDestructibleTemplate)
            {
                if (myScript.objectDestructibleTemplate.OnDestruct != null && myScript.objectDestructibleTemplate.OnDestruct.GetPersistentEventCount() > 0)
                {
                    EditorGUILayout.HelpBox(
                        "Looks like your Object Destructible template's break event isn't empty - any references will not be replaced when it's copied. Please double check them on your dish after cooking.",
                        MessageType.Warning
                    );
                }
            }
            
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Generate Object Destructible template"))
            {
                var template = new GameObject("Object Destructible Template");
                
                var objectDestructible = template.AddComponent<ObjectDestructible>();
                MakeJointsBreakable.ApplyDefaultValues(objectDestructible);
                
                template.transform.parent = myScript.gameObject.transform;
                myScript.objectDestructibleTemplate = objectDestructible;
                
                Undo.RegisterCreatedObjectUndo(template, "Generate Object Destructible template");
            }
        }
    }
}
#endif