#if UNITY_EDITOR
using UltEvents;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(GenerateResetUltEventHolder))]
    public class GenerateResetUltEventHolderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "This script generates an UltEventHolder that resets rigidbodies back to their default state (positions, rotations, velocities, Prop_Health and joints).",
                MessageType.Info);
            
            DrawDefaultInspector();
            
            var myScript = (GenerateResetUltEventHolder)target;
            var skillet = myScript.GetComponent<DestructionSkillet>();

            using (new EditorGUI.DisabledGroupScope(!(skillet && skillet.targetGameObject)))
            {
                GUILayout.Space(10);
                GUILayout.Label("Skillet", EditorStyles.boldLabel);
                if (!GUILayout.Button("Insert Placeholder")) return;

                var holderTransform = skillet.targetGameObject.transform.Find(myScript.outputGameObjectName);
                var holderGameObject = holderTransform ? holderTransform.gameObject : new GameObject
                {
                    name = myScript.outputGameObjectName,
                    transform =
                    {
                        parent = skillet.targetGameObject.transform
                    }
                };
                
                holderGameObject.GetOrAddComponent<UltEventHolder>();
                Undo.RegisterCreatedObjectUndo(holderGameObject, "Insert Placeholder");
            }
        }
    }
}
#endif