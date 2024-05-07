#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using FizzSDK.Utils;

namespace FizzSDK.Destruction
{
    [CustomEditor(typeof(DestructionSkillet))]
    public class DestructionSkilletEditor : UnityEditor.Editor
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
                    "The Destruction Skillet is used to output new prefabs using the ingredient scripts you provide it."
                    + "\n\nEach ingredient script will run in whatever order you specify (you'll want to put 'Joint Creator' before 'Make Joints Breakable' for example!)"
                    + "\n\nWhen you add new ingredients to this GameObject, click 'refresh ingredients' to add them to the list.",
                    
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
            
            var myScript = (DestructionSkillet)target;

            GUILayout.Label("Save your dish!", EditorStyles.boldLabel);
            
            if (!myScript.targetGameObject)
            {
                EditorGUILayout.HelpBox("A target GameObject must be assigned!", MessageType.Warning);
            }
            
            using (new EditorGUI.DisabledGroupScope(!myScript.targetGameObject))
            {
                if (myScript.savedPrefabPath.Length > 0)
                {
                    GUILayout.Label($"Last saved to: {myScript.savedPrefabPath}");
                }
                
                if (GUILayout.Button("Save as new Prefab"))
                {
                    var prefabOutputPath = EditorUtility.SaveFilePanel(
                        "Save Destruction Dish", 
                        "Assets", 
                        $"{myScript.targetGameObject.name}_destructible", 
                        "prefab"
                    );
                    
                    if (prefabOutputPath.Length == 0)
                        return;
                    
                    prefabOutputPath = "Assets\\" + PathUtils.MakeRelativePath(prefabOutputPath, Application.dataPath);

                    if (File.Exists(prefabOutputPath))
                    {
                        if (!EditorUtility.DisplayDialog("Overwrite prefab?",
                                "Looks like you're trying to overwrite an existing prefab - you may want to keep a separate non-destructible version. Continue?",
                                "Yes", "No"))
                        {
                            return;
                        }
                    }

                    var prefab = myScript.SaveDishToPrefab(prefabOutputPath);
                    AssetDatabase.Refresh();
                    myScript.savedPrefabPath = prefabOutputPath;
                    Debug.Log($"Saved dish to prefab: {myScript.savedPrefabPath}");
                }

                if (myScript.savedPrefabPath.Length > 0)
                {
                    if (GUILayout.Button("Update Prefab"))
                    {
                        myScript.SaveDishToPrefab(myScript.savedPrefabPath);
                    }
                }
            }
        }
    }
}
#endif