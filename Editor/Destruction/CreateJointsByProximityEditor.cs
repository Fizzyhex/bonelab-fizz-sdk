#if UNITY_EDITOR
using UnityEditor;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(CreateJointsByProximity))]
    public class CreateJointsByProximityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Add info box
            EditorGUILayout.HelpBox(
                
                "This script creates joints between objects that are close to each other.",
                MessageType.Info
            );
            
            DrawDefaultInspector();
        }
    }
}
#endif