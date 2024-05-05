#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(DestructionSkillet))]
    public class DestructionSkilletEditor : Editor
    {
        private bool _showInformation = false;

        private void RefreshIngredients()
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
            
            if (GUILayout.Button("Save dish to prefab!"))
            {
                var myScript = (DestructionSkillet)target;
                
                var prefabOutputPath =
                    EditorUtility.SaveFilePanel("Save Destruction Dish", "Assets", $"{myScript.gameObject.name}_destructible", "prefab");

                if (prefabOutputPath.Length == 0)
                    return;
                
                // display a warning if the user is saving over an existing prefab
                
                if (File.Exists(prefabOutputPath))
                {
                    if (!EditorUtility.DisplayDialog("Overwrite prefab?",
                        "Looks like you're trying to overwrite an existing prefab - you may want to keep a separate non-destructible version. Continue?",
                        "Yes", "No"))
                    {
                        return;
                    }
                }
                
                myScript.SaveDishToPrefab(prefabOutputPath);
            }
        }
    }
}
#endif