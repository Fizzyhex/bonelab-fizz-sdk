#if UNITY_EDITOR
using SLZ.Props;
using UnityEditor;
using UnityEngine;

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
            
            DrawDefaultInspector();
            
            var myScript = (MakeJointsBreakable)target;

            if (GUILayout.Button("Generate Prop_Health template"))
            {
                var template = new GameObject("Prop_Health Template");
                
                var propHealth = template.AddComponent<Prop_Health>();
                myScript.ApplyDefaultPropHealthValues(propHealth);
                
                template.transform.parent = myScript.gameObject.transform;
                myScript.propHealthTemplate = propHealth;
            }
        }
    }
}
#endif