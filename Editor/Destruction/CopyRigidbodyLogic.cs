#if UNITY_EDITOR
using UnityEditor;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(CopyUltLogic))]
    public class CopyUltLogicEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "This script takes in a template with UltEvent logic in a GameObject underneath it, and copies it across to all other GameObjects with updated references.",
                MessageType.Info);
            
            var myScript = (CopyUltLogic)target;

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