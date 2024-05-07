#if UNITY_EDITOR
using UnityEditor;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(GenerateResetUltEventHolder))]
    public class GenerateResetUltEventHolderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "This script generates an UltEventHolder that will reset the transforms and health of all Rigidbodies underneath the Dish.",
                MessageType.Info);
            
            DrawDefaultInspector();
        }
    }
}
#endif