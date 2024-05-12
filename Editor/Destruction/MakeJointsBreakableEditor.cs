#if UNITY_EDITOR
using SLZ.Props;
using UnityEditor;
using UnityEngine;

using FizzSDK.Utils;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(MakeJointsBreakable))]
    public class MakeJointsBreakableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                
                "This script adds Prop_Health and destruction logic to all Rigidbodies.",
                MessageType.Info
            );
            
            var myScript = (MakeJointsBreakable)target;
            
            if (myScript.propHealthTemplate)
            {
                if (myScript.propHealthTemplate.BreakEvent != null && myScript.propHealthTemplate.BreakEvent.GetPersistentEventCount() > 0)
                {
                    EditorGUILayout.HelpBox(
                        "Looks like your Prop_Health template's break event isn't empty - any references will not be replaced when it's copied. Please double check them on your dish after cooking.",
                        MessageType.Warning
                    );
                }
            }
            
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Generate Prop_Health template"))
            {
                var template = new GameObject("Prop_Health Template");
                
                var propHealth = template.AddComponent<Prop_Health>();
                MakeJointsBreakable.ApplyDefaultPropHealthValues(propHealth);
                
                template.transform.parent = myScript.gameObject.transform;
                myScript.propHealthTemplate = propHealth;
                
                Undo.RegisterCreatedObjectUndo(template, "Generate Prop_Health template");
            }
        }
    }
}
#endif