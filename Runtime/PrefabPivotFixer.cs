#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;

namespace FizzSDK
{
    [Obsolete("shit doesn't work D:, pls dont use")]
    public class PrefabPivotFixer : EditorWindow
    {
        private GameObject _prefabTarget;
        private const string MeshDirectory = "Assets/Resources/FixedMeshes";

        //[MenuItem("Tools/Prefab Pivot Fixer")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(PrefabPivotFixer));
            window.titleContent = new GUIContent("Prefab Pivot Fixer");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Choose a prefab file to modify", EditorStyles.boldLabel);
            _prefabTarget = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefabTarget, typeof(GameObject), false);

            if (GUILayout.Button("Fix meshes"))
            {
                var prefabPath = AssetDatabase.GetAssetPath(_prefabTarget);
                ResetPivot(prefabPath);
            }
        }

        private void CreateMeshDirectoryIfDoesntExist()
        {
            // this sucks. 

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(MeshDirectory))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "FixedMeshes");
            }
        }

        private void RecenterPivot(GameObject obj)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogWarning("No mesh filter or mesh found on object: " + obj.name);
                return;
            }

            var oldScale = obj.transform.localScale;

            if (!obj.TryGetComponent<MeshRenderer>(out var _))
            {
                Debug.LogWarning("No mesh renderer found on object: " + obj.name);
                return;
            }

            if (obj.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                boxCollider.center = Vector3.zero;
            }

            if (obj.TryGetComponent<SphereCollider>(out var sphereCollider))
            {
                sphereCollider.center = Vector3.zero;
            }

            var oldMesh = meshFilter.sharedMesh;
            Mesh modifiedMesh = new()
            {
                vertices = oldMesh.vertices
            };

            var center = modifiedMesh.bounds.center;
            var offset = center - obj.transform.localPosition;

            var vertices = modifiedMesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= offset;
            }

            modifiedMesh.vertices = vertices;
            modifiedMesh.RecalculateBounds();

            var meshFilePath = $"{MeshDirectory}/{meshFilter.transform.name}.asset";
            AssetDatabase.CreateAsset(modifiedMesh, meshFilePath);

            Undo.RecordObject(obj.transform, "Fix transform");
            obj.transform.localPosition = offset;
            obj.transform.localScale = oldScale;

            meshFilter.sharedMesh = modifiedMesh;

            EditorUtility.SetDirty(meshFilter);
            EditorUtility.SetDirty(obj);

            Debug.Log($"applied offset to {obj}: {offset}");
        }

        private void ResetPivot(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("Failed to load prefab file at path: " + prefabPath);
                return;
            }

            var prefabInstance = Instantiate(prefab);
            prefabInstance.name = prefab.name;
            var meshFilters = prefabInstance.GetComponentsInChildren<MeshFilter>();

            CreateMeshDirectoryIfDoesntExist();

            foreach (var meshFilter in meshFilters)
            {
                RecenterPivot(meshFilter.gameObject);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Save changes to the prefab
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            DestroyImmediate(prefabInstance);
        }
    }
}

#endif