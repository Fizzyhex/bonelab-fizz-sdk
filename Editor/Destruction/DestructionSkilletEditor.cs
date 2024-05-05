#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(DestructionSkillet))]
    public class DestructionSkilletEditor : Editor
    {
        bool _showInformation = false;
        
        void RefreshIngredients()
        {
            var myScript = (DestructionSkillet)target;
            myScript.RefreshIngredients();
        }
        
        public override void OnInspectorGUI()
        {
            _showInformation = EditorGUILayout.Foldout(_showInformation, "Information");

            if (_showInformation)
            {
                var style = EditorStyles.label;
                style.wordWrap = true;

                GUILayout.Label(
                    "The Destruction Skillet is used to output new prefabs using the ingredient scripts you provide it." +
                    "\n\nEach ingredient script will run in whatever order you specify (you'll want to put 'Joint Creator' before 'Make Joints Breakable' for example!)",
                    style
                );
                
                GUILayout.Space(10);
            }
            
            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Refresh ingredients"))
            {
                RefreshIngredients();
            }
            
            GUILayout.Space(10);

            if (!GUILayout.Button("Save dish to Prefab!")) return;

            var myScript = (DestructionSkillet)target;
            myScript.SaveDishToPrefab();
        }
    }
}
#endif