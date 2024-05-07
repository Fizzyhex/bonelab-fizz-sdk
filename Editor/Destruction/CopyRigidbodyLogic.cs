#if UNITY_EDITOR
using UnityEditor;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(CopyRigidbodyLogic))]
    public class CopyRigidbodyLogicEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "This script takes in a template Rigidbody with logic underneath it and copies it across to all other Rigidbodies.",
                MessageType.Info);
            
            var myScript = (CopyRigidbodyLogic)target;

            if (myScript.template && myScript.template.transform.childCount > 1)
            {
                EditorGUILayout.HelpBox(
                    "Multiple children detected underneath the template - please make sure there's only one!",
                    MessageType.Warning);
            }
            
            DrawDefaultInspector();
        }
    }
}
#endif