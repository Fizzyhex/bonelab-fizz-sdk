﻿#if UNITY_EDITOR
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
            
            var myScript = (DestructionSkillet)target;
            
            var canGenerate = myScript.targetGameObject && myScript.targetGameObject.activeInHierarchy;
            
            if (!myScript.targetGameObject)
            {
                EditorGUILayout.HelpBox("A target GameObject must be assigned!", MessageType.Warning);
                return;
            }

            if (!myScript.targetGameObject.activeInHierarchy)
            {
                EditorGUILayout.HelpBox("The target GameObject is disabled!", MessageType.Warning);
            }
            
            if (myScript.ingredients.Count < myScript.gameObject.GetComponents<DestructionIngredient>().Length)
            {
                EditorGUILayout.HelpBox("Some ingredients are missing from the skillet! Click 'Refresh ingredients' to add them.", MessageType.Warning);
            }
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            if (GUILayout.Button("Refresh ingredients"))
            {
                RefreshIngredients();
            }
            
            GUILayout.Label("Save your dish!", EditorStyles.boldLabel);
            
            using (new EditorGUI.DisabledGroupScope(!canGenerate))
            {
                if (myScript.savedPrefabPath.Length > 0)
                {
                    EditorGUILayout.SelectableLabel($"Last saved to: {myScript.savedPrefabPath}");
                }
                
                if (GUILayout.Button("Generate GameObject in scene"))
                {
                    var newGameObject = myScript.SaveDishIntoScene();
                    Selection.activeGameObject = newGameObject;
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
                    EditorUtility.SetDirty(myScript);
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
            
            EditorGUILayout.EndVertical();
        }
    }
}
#endif